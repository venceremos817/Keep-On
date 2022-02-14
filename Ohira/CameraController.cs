using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Ohira.Auxiliary;


#region HeaderComent
//==================================================================================
// CameraController
//	プレイヤーに追従し、視点変更・ロックオンができる
// 作成日時	:2021/03/17
// 作成者	:大平哲士
//---------- 更新履歴 ----------
// 21/03/16	作成開始 参考(https://tytoman.blogspot.com/2020/05/unity.html)
// 21/03/17	挙動がおかしいためいろいろ修復
// 21/03/18 カメラ内に移っているエネミーリストの追加(※今後リストの保持者が変わる可能性あり)
// 21/03/19 ↑の方法無しにして画面に映っているかつレイヤーマスクがEnemyのものをターゲッティング候補とする
// 21/03/20	InputSystem対応化
// 21/03/28	カメラのカクつき修正のためカメラ座標のUpdateをとりあえずFixedUpdateにしとくあとでもっといい方法考える
//==================================================================================
#endregion


/// <summary>
/// カメラコントロール
/// プレイヤーに追従し、視点変更・ロックオンができる
/// Shake()でカメラを揺らすことも可
/// </summary>
namespace Ohira
{
	#region Require
	[RequireComponent(typeof(Camera))]
	#endregion

	public class CameraController : SingletonMono<CameraController>
	{
		#region Inspector
		[Tooltip("カメラ感度")]
		[SerializeField] private Vector2Variable sensitivity;
		[SerializeField] private float lockOnSensitivity = 0.005f;
		[Tooltip("追従するオブジェクト")]
		[SerializeField] private GameObject primaryTarget;
		[Tooltip("ロックオンするレイヤー")]
		[SerializeField] public LayerMask targetLayerMask;
		[Tooltip("索敵距離")]
		[SerializeField] private float searchRadius = 10f;
		[Tooltip("PrimaryTargetの画面内の位置")]
		[SerializeField] private Vector2 primaryLockOnPosition = new Vector2(0f, 0f);
		[Tooltip("SecondaryTargetの画面内の位置")]
		[SerializeField] private Vector2 secondaryLockOnPosition = new Vector2(0f, 0.3f);
		private float mem_secondaryLonOnPositionY;
		[Tooltip("カメラとPrimaryObjectの距離")]
		[SerializeField] private float distance = 4f;
		[Tooltip("回転する幅の高さ")]
		[SerializeField] private float height = 1.5f;
		[Tooltip("カメラがPrimaryTargetを追従時の遅延")]
		[SerializeField] private float delay = 2f;
		[Tooltip("カメラの上から覗き込む角度制限")]
		[SerializeField] [Range(0f, 1f)] private float maxVerticalRotationLimit = 0.9f;
		[Tooltip("カメラが下から覗き込む角度制限")]
		[SerializeField] [Range(0f, 1f)] private float minVerticalRotationLimit = 0.9f;
		[Tooltip("カメラの当たり判定をする対象")]
		[SerializeField] private LayerMask layerMask;
		[Tooltip("カメラの当たり判定の半径")]
		[SerializeField] private float castRadius = 0.6f;
		[Tooltip("ロックオン優先度比")]
		[SerializeField, Range(0f, 1f)] private float lineOfSightLockOnRatio = 0.95f;
		[Tooltip("ロックオン時のSecondaryTargetの追従速度")]
		[SerializeField] [Range(0.001f, 1f)] private float lockOnSpeed = 0.1f;
		[Tooltip("オートで正面向くのにかかる時間(秒)")]
		[SerializeField] private float autoSec = 0.3f;
		[Tooltip("CameraShake時の減衰を指定するカーブ")]
		[SerializeField] private AnimationCurve shakeDecayCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
		[Tooltip("カーソルをロックするかどうか")]
		[SerializeField] private bool cursorLock = false;
		[Tooltip("カメラをスタックするか")]
		[SerializeField] private bool stack = true;
		#endregion


		#region PublicVariable
		public GameObject secondaryTarget { get; private set; } = null;  // ターゲッティングするオブジェクト
		[System.NonSerialized] public Transform lockOnTransform;
		[System.NonSerialized] public bool isLockOn = false;
		public bool enableSteal { get; private set; } = false;      // ロックオンしてるやつが奪える状態にあるか
		public EnemyInfo enemyInfo;
		#endregion


		#region PrivateVariable
		private Camera cam;                         // 操作するカメラ
		private Transform orbitalCenter;            // カメラ軌道の中心
		private Vector2 primPosOffset;              // 
		private Vector2 secPosOffset;               // 
		private float fixedDistance;                // 実際の軌道中心とカメラの距離(障害物とうに当たると短くなったりするため)
		private float frustumHeight;                // 視界(高さ)
		private float frustumWidth;                 // 視界(広さ)
		private float lastAspect;                   // 最新のカメラのアス比
		private float shakePower;                   // カメラ振動の大きさ
		private float shakeSpeed;                   // カメラ振動の速さ
		private float shakeLifeTime;                // カメラ振動をさせる時間
		private float shakeAge;                     // カメラ振動経過時間
		private bool shake = false;                 // カメラ振動中かどうか

		private float mem_y;
		private float mem_rotX;
		private float mem_rotZ;

		private float progressTime;
		private Quaternion autoStart;
		private Quaternion toAuto;
		public PlayerControler input;

		private Homare.Enemy enemyCS = null;
		private Homare.Boss bossCS = null;
		private SandBack sandBackCS = null;
		#endregion


		#region Delegate
		// カメラ挙動のデリゲート
		private delegate void CameraBehavior();
		private CameraBehavior behavior;
		#endregion





		#region PrivateMethod
		private void Start()
		{
			mem_secondaryLonOnPositionY = secondaryLockOnPosition.y;
			cam = GetComponent<Camera>();
			orbitalCenter = new GameObject().transform;
			orbitalCenter.name = "CameraOrbitalCenter";

			if (!stack)
			{
				// カメラスタック解除
				cam.GetUniversalAdditionalCameraData().cameraStack.Clear();
			}

			SetFrustumSize();
			SetPositionOffset();
			orbitalCenter.position = primaryTarget.transform.position + Vector3.up * height;            // カメラ起動の中心 初期値はプレイヤーの頭上
			transform.position = orbitalCenter.position - orbitalCenter.forward * fixedDistance;
			transform.LookAt(orbitalCenter);
			transform.position = transform.position - transform.right * primPosOffset.x - transform.up * primPosOffset.y;
			behavior = FreeLookCamera;
			isLockOn = false;

			input = new PlayerControler();
			input.Enable();
		}



		/// <summary>
		/// カメラ挙動はプレイヤー等の移動が終わってから更新するため、LateUpdate
		/// </summary>
		private void LateUpdate()
		{
			if (cursorLock)
			{
				// カーソルをロック
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
			else
			{
				// カーソルをアンロック
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}

			if (cam.aspect != lastAspect) SetFrustumSize();

			behavior();

			ObstacleDetection();
			SetPositionOffset();
			//MoveCamera();
			SetLastValues();
			if (shake) Shake();
		}


		private void FixedUpdate()
		{
			MoveCamera();
		}


		private void Update()
		{
			float rightStickX = input.Camera.Switch.ReadValue<Vector2>().x;

			if (input.Camera.LockOn.triggered)
			{
				OnLockOn();
			}
			else if (input.Camera.Auto.triggered)
			{
				OnAuto();
			}
			else if (input.Camera.FreeLook.triggered)
			{
				OnFreeLook();
			}
			else if (input.Camera.Rotation.triggered && !rightStickX.IsInRnage(-8f, 8f))
			{
				if (rightStickX.IsMoreThan(8f))
				{
					OnReSearch(true);
				}
				else
				{
					OnReSearch(false);
				}
			}
		}





		/// <summary>
		/// カメラと障害物の衝突/押し出し
		/// 障害物はレイヤーマスクで識別
		/// </summary>
		private void ObstacleDetection()
		{
			RaycastHit hitInfo;
			if (Physics.SphereCast(orbitalCenter.position, castRadius, transform.position - orbitalCenter.position, out hitInfo, distance, layerMask) ||
				Physics.Raycast(orbitalCenter.position, transform.position - orbitalCenter.position, out hitInfo, distance, layerMask))
			{
				fixedDistance = Mathf.Sqrt(primPosOffset.x * primPosOffset.x + primPosOffset.y * primPosOffset.y + hitInfo.distance * hitInfo.distance);
			}
			else
			{
				fixedDistance = distance;
			}
		}





		/// <summary>
		/// 視界の更新
		/// </summary>
		private void SetFrustumSize()
		{
			frustumHeight = 2 * Mathf.Tan(cam.fieldOfView * .5f * Mathf.Deg2Rad);
			frustumWidth = frustumHeight * cam.aspect;
		}





		/// <summary>
		/// プレイヤーとターゲットの画面位置
		/// </summary>
		private void SetPositionOffset()
		{
			primPosOffset.x = frustumWidth * fixedDistance * primaryLockOnPosition.x;
			primPosOffset.y = frustumHeight * fixedDistance * primaryLockOnPosition.y;
			secPosOffset.x = frustumWidth * fixedDistance * secondaryLockOnPosition.x - primaryLockOnPosition.x;
			secPosOffset.y = frustumHeight * fixedDistance * secondaryLockOnPosition.y - primaryLockOnPosition.x;
		}





		/// <summary>
		/// カメラの移動
		/// </summary>
		private void MoveCamera()
		{
			var camTgtPos = primaryTarget.transform.position + Vector3.up * height;
			var curVel = Vector3.zero;
			orbitalCenter.position = Vector3.SmoothDamp(orbitalCenter.position, camTgtPos, ref curVel, delay * Time.fixedDeltaTime);
			//orbitalCenter.position = Vector3.Lerp(orbitalCenter.position, camTgtPos , delay * Time.deltaTime);
			transform.position = orbitalCenter.position - orbitalCenter.forward * fixedDistance;
			if (isLockOn)
			{
				Vector3 pos = transform.position;
				//pos.y = mem_y;
				transform.position = pos;
				orbitalCenter.position = transform.position + orbitalCenter.forward * fixedDistance;
			}
			transform.LookAt(orbitalCenter);
			transform.position = transform.position - transform.right * primPosOffset.x - transform.up * primPosOffset.y;
		}





		/// <summary>
		/// フリールック時の挙動
		/// </summary>
		private void FreeLookCamera()
		{
			Vector2 rotateVal = input.Camera.Rotation.ReadValue<Vector2>();
			rotateVal.x *= sensitivity.value.x;
			rotateVal.y *= sensitivity.value.y;
			RotateCamera(rotateVal);
		}





		/// <summary>
		/// オート時の挙動
		/// </summary>
		private void AutoCamera()
		{
			float t = progressTime / autoSec;
			orbitalCenter.rotation = Quaternion.Lerp(autoStart, toAuto, t);

			progressTime += Time.deltaTime;

			if (t > 1f)
				OnFreeLook();
		}





		/// <summary>
		/// ロックオン時の挙動
		/// </summary>
		private void LockOnCamera()
		{
			// ロックオン対象がなければフリールックに移行
			if (secondaryTarget == null)
			{
				behavior = FreeLookCamera;
				isLockOn = false;
				return;
			}

			secondaryLockOnPosition.y -= input.Camera.Rotation.ReadValue<Vector2>().y * lockOnSensitivity;
			secondaryLockOnPosition.y = secondaryLockOnPosition.y.Clamp(-0.45f, 0.5f);
			Vector2 rotateVal = input.Camera.Rotation.ReadValue<Vector2>();
			rotateVal.x = 0f;
			rotateVal.y *= sensitivity.value.y;
			RotateCamera(rotateVal);

			// カメラからターゲットのベクトル
			Vector3 tgtVecFromCam = transform.forward +
				transform.right * frustumWidth * (secondaryLockOnPosition.x - primaryLockOnPosition.x) +
				transform.up * frustumHeight * (secondaryLockOnPosition.y - primaryLockOnPosition.y);

			Vector3 secTgtDir = lockOnTransform.position + Vector3.up * height - orbitalCenter.transform.position;

			float ratio = Mathf.Sqrt(secTgtDir.sqrMagnitude + tgtVecFromCam.sqrMagnitude);

			Vector3 tgtFwd = secTgtDir -
				transform.right * ratio * frustumWidth * (secondaryLockOnPosition.x - primaryLockOnPosition.x) -
				transform.up * ratio * frustumHeight * (secondaryLockOnPosition.y - primaryLockOnPosition.y);

			orbitalCenter.rotation = Quaternion.Slerp(orbitalCenter.rotation, Quaternion.LookRotation(tgtFwd, Vector3.up), lockOnSpeed);

			CheckSteal();
		}





		/// <summary>
		/// ロックオンするターゲットの探索
		/// カメラに移っているもので最短距離
		/// </summary>
		private GameObject SearchTarget()
		{
			// プレイヤー位置から索敵範囲内にいる敵をリスト化する
			List<GameObject> hits = Physics.SphereCastAll(
				primaryTarget.transform.position,
				searchRadius,
				primaryTarget.transform.forward,
				0.01f,
				targetLayerMask
				).Select(h => h.transform.gameObject).ToList();

			// 作成したリストからカメラに映っているものを抽出
			hits = FilterTargetObject(hits);

			// カメラからレイを出し、そのレイから最も近いオブジェクト
			Ray ray = new Ray(transform.position, transform.forward);
			//	Debug.DrawRay(ray.origin, ray.direction * float.MaxValue, Color.cyan, 10);
			return GetNearestObjectIncludeRation(ray, hits);
		}





		/// <summary>
		/// GameObjectリストを受け取り、メインカメラに移っているものだけを抽出する
		/// </summary>
		/// <param name="gameobjectList">仕分けたいGameObjectリスト</param>
		/// <returns>メインカメラに移っているものだけ抽出したリスト</returns>
		private List<GameObject> FilterTargetObject(List<GameObject> gameobjectList)
		{
			return gameobjectList.Where(h =>
			{
				Vector3 screenPoint = cam.WorldToViewportPoint(h.transform.position);
				Vector3 camToEnemy = h.transform.position - transform.position;
				return
				(screenPoint.x > 0f && screenPoint.x < 1f && screenPoint.y > 0f && screenPoint.y < 1f) &&
				Vector3.Dot(transform.forward, camToEnemy) > 0f;
			}).ToList();
		}


		private GameObject ReSearchTarget(bool lr)
		{
			// プレイヤー位置から索敵範囲内にいる敵をリスト化する
			List<GameObject> hits = Physics.SphereCastAll(
				primaryTarget.transform.position,
				searchRadius,
				primaryTarget.transform.forward,
				0.01f,
				targetLayerMask
				).Select(h => h.transform.gameObject).ToList();

			hits.Remove(secondaryTarget);
			hits = FilterTargetObject(hits, lr);

			// カメラからレイを出し、そのレイから最も近いオブジェクト
			Ray ray = new Ray(transform.position, transform.forward);
			return GetNearestObject(ray, hits);
		}

		private List<GameObject> FilterTargetObject(List<GameObject> gameobjectList, bool lr)
		{
			float minX = 0f;
			float maxX = 0.5f;
			if (lr)
			{
				minX = 0.5f;
				maxX = 1f;
			}

			return gameobjectList.Where(h =>
			{
				Vector3 screenPoint = cam.WorldToViewportPoint(h.transform.position);
				Vector3 camToEnemy = h.transform.position - transform.position;
				return
				(screenPoint.x > minX && screenPoint.x < maxX && screenPoint.y > 0f && screenPoint.y < 1f) &&
				Vector3.Dot(transform.forward, camToEnemy) > 0f;
			}).ToList();
		}






		/// <summary>
		/// ある点から最も近いオブジェクトを返す
		/// </summary>
		/// <param name="pos">始点</param>
		/// <param name="gameObjectsList">調べたいリスト</param>
		/// <returns>最も近いオブジェクト</returns>
		private GameObject GetNearestObject(Vector3 pos, List<GameObject> gameObjectsList)
		{
			if (0 < gameObjectsList.Count())
			{
				float minDistance = float.MaxValue;
				GameObject nearObj = null;

				foreach (var obj in gameObjectsList)
				{
					float distance = Vector3.Distance(pos, obj.transform.position);

					if (distance < minDistance)
					{
						minDistance = distance;
						nearObj = obj.transform.gameObject;
					}
				}

				return nearObj;
			}
			else
			{
				return null;
			}
		}





		/// <summary>
		/// rayから最も近いオブジェクトを返す
		/// </summary>
		/// <param name="ray">ray</param>
		/// <param name="gameObjectsList">調べたいリスト</param>
		/// <returns>最も近いオブジェクト</returns>
		private GameObject GetNearestObjectIncludeRation(Ray ray, List<GameObject> gameObjectsList)
		{
			if (0 < gameObjectsList.Count())
			{
				float minDistance = float.MaxValue;
				GameObject nearObj = null;

				foreach (var obj in gameObjectsList)
				{
					float rayToEnemyDistance = Vector3.Cross(ray.direction, obj.transform.position - ray.origin).magnitude;       // レイと敵の距離
					float playerToEnemyDistance = (obj.transform.position - primaryTarget.transform.position).magnitude; // プレイヤーと敵の距離
																														 //distance += (obj.transform.position - primaryTarget.transform.position).magnitude;
					float priority = rayToEnemyDistance * lineOfSightLockOnRatio + playerToEnemyDistance * (1f - lineOfSightLockOnRatio);

					if (priority < minDistance)
					{
						minDistance = priority;
						nearObj = obj.transform.gameObject;
					}

				}

				return nearObj;
			}
			else
			{
				return null;
			}
		}


		private GameObject GetNearestObject(Ray ray, List<GameObject> gameObjects)
		{
			if (0 < gameObjects.Count())
			{
				float minDistance = float.MaxValue;
				GameObject nearObj = null;
				//const float ratio = 0.95f;

				foreach (var obj in gameObjects)
				{
					float distance = Vector3.Cross(ray.direction, obj.transform.position - ray.origin).magnitude;       // カメラから飛ばしたレイと敵の距離
					float cameraToEnemyDistance = (obj.transform.position - primaryTarget.transform.position).magnitude; // プレイヤーと敵の距離
																														 //distance += (obj.transform.position - primaryTarget.transform.position).magnitude;
					float priority = distance;// * ratio + cameraToEnemyDistance * (1f - ratio);

					if (priority < minDistance)
					{
						minDistance = priority;
						nearObj = obj.transform.gameObject;
					}

				}

				return nearObj;
			}
			else
			{
				return null;
			}
		}





		/// <summary>
		/// カメラアス比等変更
		/// </summary>
		private void SetLastValues()
		{
			lastAspect = cam.aspect;
		}





		/// <summary>
		/// カメラ振動挙動
		/// </summary>
		private void Shake()
		{
			//shakeAge += Time.deltaTime;
			shakeAge += Time.unscaledDeltaTime;
			var time = Time.time * shakeSpeed;
			var noiseX = (Mathf.PerlinNoise(time, time) - .5f) * shakePower * shakeDecayCurve.Evaluate(shakeAge / shakeLifeTime);
			var noiseY = (Mathf.PerlinNoise(time, time + 128) - .5f) * shakePower * shakePower * shakeDecayCurve.Evaluate(shakeAge / shakeLifeTime);
			transform.rotation = Quaternion.Euler(noiseX, noiseY, 0) * transform.rotation;
		}





		/// <summary>
		/// カメラ振動開始
		/// </summary>
		/// <param name="waitTime">振動時間</param>
		/// <returns>振動経過時間</returns>
		IEnumerator SetShake(float waitTime)
		{
			shake = true;
			yield return new WaitForSeconds(waitTime);
			shake = false;
		}
		#endregion





		#region PublicMethod
		/// <summary>
		/// InputSystemの入力などを引数としてカメラの視点を回転する
		/// </summary>
		/// <param name="input">カメラ移動量</param>
		public void RotateCamera(Vector2 input)
		{
			input.y *= -1;
			var isExMaxLim = input.y < 0 && transform.forward.y > minVerticalRotationLimit;
			var isExMinLim = input.y > 0 && transform.forward.y < -maxVerticalRotationLimit;
			if (isExMaxLim || isExMinLim) input.y = 0;
			input *= Time.deltaTime;
			orbitalCenter.RotateAround(orbitalCenter.position, Vector3.up, input.x);
			orbitalCenter.RotateAround(orbitalCenter.position, transform.right, input.y);
		}





		/// <summary>
		/// カメラを振動させる
		/// </summary>
		/// <param name="power">振動の大きさ</param>
		/// <param name="speed">振動の速さ</param>
		/// <param name="shakeTime">振動時間</param>
		public void ShakeCamera(float power, float speed, float shakeTime)
		{
			shakePower = power;
			shakeSpeed = speed;
			shakeLifeTime = shakeTime;
			shakeAge = 0;
			IEnumerator setShale = SetShake(shakeTime);
			StartCoroutine(setShale);
		}


		public void InputDisable()
		{
			input.Disable();
		}


		public void InputEnable()
		{
			input.Enable();
		}


		public Vector3 GetPlayerToTarget()
		{
			//return secondaryTarget.transform.position - primaryTarget.transform.position;
			return lockOnTransform.position - primaryTarget.transform.position;
		}


		/// <summary>
		/// ロックオンしているかどうか
		/// </summary>
		/// <returns>true: ロックオン中	false:	ロックオンしてない</returns>
		public bool IsLocking()
		{
			return lockOnTransform;
		}



		/// <summary>
		/// カメラがフリー時にホーミング許容範囲に敵がいればそのTransformを返す
		/// </summary>
		/// <param name="forward">プレイヤーの向いている方向</param>
		/// <param name="permissionDistance">許容距離</param>
		/// <param name="permissionAngle">許容角度</param>
		/// <returns>プレイヤーから敵へのベクトル	該当なしのときは前方</returns>
		public Vector3 GetFreeHomingVector(float permissionDistance, float permissionAngle)
		{
			Vector3 ret = primaryTarget.transform.forward;
			Vector2 forward = new Vector2(primaryTarget.transform.forward.x, primaryTarget.transform.forward.z).normalized;

			// プレイヤー位置から索敵範囲内にいる敵をリスト化する
			List<GameObject> hits = Physics.SphereCastAll(
				primaryTarget.transform.position,
				distance,
				primaryTarget.transform.forward,
				0.01f,
				targetLayerMask
				).Select(h => h.transform.gameObject).ToList();

			if (0 < hits.Count())
			{
				float minDistance = permissionDistance;

				// 許容範囲にあって、かつ一番近い敵を探索
				foreach (var obj in hits)
				{
					float distance = (primaryTarget.transform.position - obj.transform.position).magnitude;

					if (distance < minDistance)
					{
						Vector3 playerToEnemy = obj.transform.position - primaryTarget.transform.position;
						// 自分のforwardと自分から敵への角度
						float angle = Mathf.Abs(Vector2.Angle(forward, new Vector2(playerToEnemy.x, playerToEnemy.z).normalized));
						//Debug.Log("大平angle" + angle);
						if (angle < permissionAngle)
						{
							minDistance = distance;
							if (obj.TryGetComponent(out LockOnPoint lockOnPoint))
							{
								ret = lockOnPoint.lockOnPoint.position - primaryTarget.transform.position;
							}
						}
					}
				}
			}

			return ret;
		}


		public Transform GetFreeHomingTransform(float permissionDistance, float permissionAngle, Transform guarantee)
		{
			Transform ret = guarantee;
			Vector2 forward = new Vector2(primaryTarget.transform.forward.x, primaryTarget.transform.forward.z).normalized;

			// プレイヤー位置から索敵範囲内にいる敵をリスト化する
			List<GameObject> hits = Physics.SphereCastAll(
				primaryTarget.transform.position,
				distance,
				primaryTarget.transform.forward,
				0.01f,
				targetLayerMask
				).Select(h => h.transform.gameObject).ToList();

			if (0 < hits.Count())
			{
				float minDistance = permissionDistance;

				// 許容範囲にあって、かつ一番近い敵を探索
				foreach (var obj in hits)
				{
					float distance = (primaryTarget.transform.position - obj.transform.position).magnitude;

					if (distance < minDistance)
					{
						Vector3 playerToEnemy = obj.transform.position - primaryTarget.transform.position;
						// 自分のforwardと自分から敵への角度
						float angle = Mathf.Abs(Vector2.Angle(forward, new Vector2(playerToEnemy.x, playerToEnemy.z).normalized));
						//Debug.Log("大平angle" + angle);
						if (angle < permissionAngle)
						{
							minDistance = distance;
							if (obj.TryGetComponent(out LockOnPoint lockOnPoint))
							{
								ret = lockOnPoint.lockOnPoint;
							}
						}
					}
				}
			}

			return ret;
		}


		public Vector3 GetFreeLocOnVector()
		{
			Vector3 ret = primaryTarget.transform.forward;

			var target = SearchTarget();
			if (target)
			{
				if (target.TryGetComponent(out LockOnPoint lockOnPoint))
				{
					ret = lockOnPoint.lockOnPoint.position - primaryTarget.transform.position;
				}
			}

			return ret;
		}


		public void ForcedForcus(GameObject target)
		{
			secondaryTarget = target;
			LockOnPoint lockOnPoint;
			if (!(lockOnPoint = secondaryTarget.GetComponentInChildren<LockOnPoint>()))
			{
				OnFreeLook();
				return;
			}
			secondaryLockOnPosition.y = mem_secondaryLonOnPositionY;
			lockOnTransform = lockOnPoint.lockOnPoint;
			behavior = LockOnCamera;
			isLockOn = true;

			mem_y = transform.position.y;
			mem_rotX = transform.rotation.x;
			mem_rotZ = transform.rotation.y;
		}


		public void OnResusltStart()
		{
			input.Disable();
			OnFreeLook();
		}


		private void TryGetTargetScripts()
		{
			secondaryTarget.TryGetComponent(out enemyCS);
			secondaryTarget.TryGetComponent(out bossCS);
			secondaryTarget.TryGetComponent(out sandBackCS);
		}


		/// <summary>
		/// ロックオンしている相手が奪える状態にあるか調べる
		/// </summary>
		private void CheckSteal()
		{
			Debug.Log("大平" + enableSteal);
			if (enemyCS != null)
			{
				enableSteal = enemyCS.isWeapon;
				enemyInfo.maxHP = enemyCS.enemyStatus.maxHp;
				enemyInfo.currentHP = enemyCS.Hp;
				enemyInfo.kind = EnemyInfo.Kind.COMMON;
				return;
			}
			if (bossCS != null)
			{
				enableSteal = bossCS.isDown;
				enemyInfo.maxHP = bossCS.MaxHp;
				enemyInfo.currentHP = bossCS.Hp;
				enemyInfo.kind = EnemyInfo.Kind.BOSS;
				return;
			}
			if (sandBackCS != null)
			{
				enableSteal = sandBackCS.isWeapon;
				enemyInfo.maxHP = sandBackCS.status.maxHp;
				enemyInfo.currentHP = sandBackCS.Hp;
				enemyInfo.kind = EnemyInfo.Kind.COMMON;
				return;
			}
			else
			{
				enemyInfo.kind = EnemyInfo.Kind.HEAL;
			}

		}
		#endregion


		#region StaticMethod
		/// <summary>
		/// ロックオンを外す
		/// ロックオンされてなければ何も起きない
		/// </summary>
		/// <param name="obj">外したいゲームオブジェクト</param>
		public void RemoveLockOn(GameObject obj)
		{
			if (obj == secondaryTarget)
			{
				OnFreeLook();
			}
		}
		#endregion



		#region Input
		/// <summary>
		/// フリールックに切り替え
		/// </summary>
		private void OnFreeLook()
		{
			behavior = FreeLookCamera;
			secondaryTarget = null;
			lockOnTransform = null;
			isLockOn = false;
		}


		/// <summary>
		/// オートモードに切り替え
		/// </summary>
		private void OnAuto()
		{
			behavior = AutoCamera;
			secondaryTarget = null;
			isLockOn = false;
			progressTime = 0f;
			autoStart = orbitalCenter.rotation;
			toAuto = Quaternion.LookRotation(primaryTarget.transform.forward, Vector3.up);
		}


		/// <summary>
		/// ロックオンモードに切り替え
		/// </summary>
		private void OnLockOn()
		{
			if (isLockOn)
			{
				OnFreeLook();
				return;
			}

			secondaryTarget = SearchTarget();
			if (secondaryTarget == null)
			{
				OnFreeLook();
				return;
			}
			if (!secondaryTarget.TryGetComponent(out LockOnPoint lockOnPoint))
			{
				OnFreeLook();
				return;
			}
			secondaryLockOnPosition.y = mem_secondaryLonOnPositionY;
			lockOnTransform = lockOnPoint.lockOnPoint;
			behavior = LockOnCamera;
			isLockOn = true;

			TryGetTargetScripts();

			mem_y = transform.position.y;
			mem_rotX = transform.rotation.x;
			mem_rotZ = transform.rotation.y;

            Mizuno.SoundManager.Instance.PlayMenuSe("SE_PlayerLockOn");
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="lr">false : left , true  : right</param>
		private void OnReSearch(bool lr)
		{
			if (!IsLocking())
			{
				OnFreeLook();
				return;
			}

			var temp = ReSearchTarget(lr);
			if (temp == null)
			{
				return;
			}
			if (!temp.TryGetComponent(out LockOnPoint lockOnPoint))
			{
				return;
			}

			secondaryTarget = temp;
			lockOnTransform = lockOnPoint.lockOnPoint;
			behavior = LockOnCamera;
			isLockOn = true;

			TryGetTargetScripts();

			mem_y = transform.position.y;
			mem_rotX = transform.rotation.x;
			mem_rotZ = transform.rotation.y;

			Mizuno.SoundManager.Instance.PlayMenuSe("SE_PlayerLockOn");
		}
		#endregion
	}
}



public struct EnemyInfo
{
	public float maxHP;
	public float currentHP;
	public Kind kind;


	public enum Kind
	{
		COMMON,
		HEAL,
		BOSS
	}
}