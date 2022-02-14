using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Ohira.Auxiliary;


#region HeaderComent
//==================================================================================
// CameraController
//	�v���C���[�ɒǏ]���A���_�ύX�E���b�N�I�����ł���
// �쐬����	:2021/03/17
// �쐬��	:�啽�N�m
//---------- �X�V���� ----------
// 21/03/16	�쐬�J�n �Q�l(https://tytoman.blogspot.com/2020/05/unity.html)
// 21/03/17	�����������������߂��낢��C��
// 21/03/18 �J�������Ɉڂ��Ă���G�l�~�[���X�g�̒ǉ�(�����ナ�X�g�̕ێ��҂��ς��\������)
// 21/03/19 ���̕��@�����ɂ��ĉ�ʂɉf���Ă��邩���C���[�}�X�N��Enemy�̂��̂��^�[�Q�b�e�B���O���Ƃ���
// 21/03/20	InputSystem�Ή���
// 21/03/28	�J�����̃J�N���C���̂��߃J�������W��Update���Ƃ肠����FixedUpdate�ɂ��Ƃ����Ƃł����Ƃ������@�l����
//==================================================================================
#endregion


/// <summary>
/// �J�����R���g���[��
/// �v���C���[�ɒǏ]���A���_�ύX�E���b�N�I�����ł���
/// Shake()�ŃJ������h�炷���Ƃ���
/// </summary>
namespace Ohira
{
	#region Require
	[RequireComponent(typeof(Camera))]
	#endregion

	public class CameraController : SingletonMono<CameraController>
	{
		#region Inspector
		[Tooltip("�J�������x")]
		[SerializeField] private Vector2Variable sensitivity;
		[SerializeField] private float lockOnSensitivity = 0.005f;
		[Tooltip("�Ǐ]����I�u�W�F�N�g")]
		[SerializeField] private GameObject primaryTarget;
		[Tooltip("���b�N�I�����郌�C���[")]
		[SerializeField] public LayerMask targetLayerMask;
		[Tooltip("���G����")]
		[SerializeField] private float searchRadius = 10f;
		[Tooltip("PrimaryTarget�̉�ʓ��̈ʒu")]
		[SerializeField] private Vector2 primaryLockOnPosition = new Vector2(0f, 0f);
		[Tooltip("SecondaryTarget�̉�ʓ��̈ʒu")]
		[SerializeField] private Vector2 secondaryLockOnPosition = new Vector2(0f, 0.3f);
		private float mem_secondaryLonOnPositionY;
		[Tooltip("�J������PrimaryObject�̋���")]
		[SerializeField] private float distance = 4f;
		[Tooltip("��]���镝�̍���")]
		[SerializeField] private float height = 1.5f;
		[Tooltip("�J������PrimaryTarget��Ǐ]���̒x��")]
		[SerializeField] private float delay = 2f;
		[Tooltip("�J�����̏ォ��`�����ފp�x����")]
		[SerializeField] [Range(0f, 1f)] private float maxVerticalRotationLimit = 0.9f;
		[Tooltip("�J������������`�����ފp�x����")]
		[SerializeField] [Range(0f, 1f)] private float minVerticalRotationLimit = 0.9f;
		[Tooltip("�J�����̓����蔻�������Ώ�")]
		[SerializeField] private LayerMask layerMask;
		[Tooltip("�J�����̓����蔻��̔��a")]
		[SerializeField] private float castRadius = 0.6f;
		[Tooltip("���b�N�I���D��x��")]
		[SerializeField, Range(0f, 1f)] private float lineOfSightLockOnRatio = 0.95f;
		[Tooltip("���b�N�I������SecondaryTarget�̒Ǐ]���x")]
		[SerializeField] [Range(0.001f, 1f)] private float lockOnSpeed = 0.1f;
		[Tooltip("�I�[�g�Ő��ʌ����̂ɂ����鎞��(�b)")]
		[SerializeField] private float autoSec = 0.3f;
		[Tooltip("CameraShake���̌������w�肷��J�[�u")]
		[SerializeField] private AnimationCurve shakeDecayCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
		[Tooltip("�J�[�\�������b�N���邩�ǂ���")]
		[SerializeField] private bool cursorLock = false;
		[Tooltip("�J�������X�^�b�N���邩")]
		[SerializeField] private bool stack = true;
		#endregion


		#region PublicVariable
		public GameObject secondaryTarget { get; private set; } = null;  // �^�[�Q�b�e�B���O����I�u�W�F�N�g
		[System.NonSerialized] public Transform lockOnTransform;
		[System.NonSerialized] public bool isLockOn = false;
		public bool enableSteal { get; private set; } = false;      // ���b�N�I�����Ă����D�����Ԃɂ��邩
		public EnemyInfo enemyInfo;
		#endregion


		#region PrivateVariable
		private Camera cam;                         // ���삷��J����
		private Transform orbitalCenter;            // �J�����O���̒��S
		private Vector2 primPosOffset;              // 
		private Vector2 secPosOffset;               // 
		private float fixedDistance;                // ���ۂ̋O�����S�ƃJ�����̋���(��Q���Ƃ��ɓ�����ƒZ���Ȃ����肷�邽��)
		private float frustumHeight;                // ���E(����)
		private float frustumWidth;                 // ���E(�L��)
		private float lastAspect;                   // �ŐV�̃J�����̃A�X��
		private float shakePower;                   // �J�����U���̑傫��
		private float shakeSpeed;                   // �J�����U���̑���
		private float shakeLifeTime;                // �J�����U���������鎞��
		private float shakeAge;                     // �J�����U���o�ߎ���
		private bool shake = false;                 // �J�����U�������ǂ���

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
		// �J���������̃f���Q�[�g
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
				// �J�����X�^�b�N����
				cam.GetUniversalAdditionalCameraData().cameraStack.Clear();
			}

			SetFrustumSize();
			SetPositionOffset();
			orbitalCenter.position = primaryTarget.transform.position + Vector3.up * height;            // �J�����N���̒��S �����l�̓v���C���[�̓���
			transform.position = orbitalCenter.position - orbitalCenter.forward * fixedDistance;
			transform.LookAt(orbitalCenter);
			transform.position = transform.position - transform.right * primPosOffset.x - transform.up * primPosOffset.y;
			behavior = FreeLookCamera;
			isLockOn = false;

			input = new PlayerControler();
			input.Enable();
		}



		/// <summary>
		/// �J���������̓v���C���[���̈ړ����I����Ă���X�V���邽�߁ALateUpdate
		/// </summary>
		private void LateUpdate()
		{
			if (cursorLock)
			{
				// �J�[�\�������b�N
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
			else
			{
				// �J�[�\�����A�����b�N
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
		/// �J�����Ə�Q���̏Փ�/�����o��
		/// ��Q���̓��C���[�}�X�N�Ŏ���
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
		/// ���E�̍X�V
		/// </summary>
		private void SetFrustumSize()
		{
			frustumHeight = 2 * Mathf.Tan(cam.fieldOfView * .5f * Mathf.Deg2Rad);
			frustumWidth = frustumHeight * cam.aspect;
		}





		/// <summary>
		/// �v���C���[�ƃ^�[�Q�b�g�̉�ʈʒu
		/// </summary>
		private void SetPositionOffset()
		{
			primPosOffset.x = frustumWidth * fixedDistance * primaryLockOnPosition.x;
			primPosOffset.y = frustumHeight * fixedDistance * primaryLockOnPosition.y;
			secPosOffset.x = frustumWidth * fixedDistance * secondaryLockOnPosition.x - primaryLockOnPosition.x;
			secPosOffset.y = frustumHeight * fixedDistance * secondaryLockOnPosition.y - primaryLockOnPosition.x;
		}





		/// <summary>
		/// �J�����̈ړ�
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
		/// �t���[���b�N���̋���
		/// </summary>
		private void FreeLookCamera()
		{
			Vector2 rotateVal = input.Camera.Rotation.ReadValue<Vector2>();
			rotateVal.x *= sensitivity.value.x;
			rotateVal.y *= sensitivity.value.y;
			RotateCamera(rotateVal);
		}





		/// <summary>
		/// �I�[�g���̋���
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
		/// ���b�N�I�����̋���
		/// </summary>
		private void LockOnCamera()
		{
			// ���b�N�I���Ώۂ��Ȃ���΃t���[���b�N�Ɉڍs
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

			// �J��������^�[�Q�b�g�̃x�N�g��
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
		/// ���b�N�I������^�[�Q�b�g�̒T��
		/// �J�����Ɉڂ��Ă�����̂ōŒZ����
		/// </summary>
		private GameObject SearchTarget()
		{
			// �v���C���[�ʒu������G�͈͓��ɂ���G�����X�g������
			List<GameObject> hits = Physics.SphereCastAll(
				primaryTarget.transform.position,
				searchRadius,
				primaryTarget.transform.forward,
				0.01f,
				targetLayerMask
				).Select(h => h.transform.gameObject).ToList();

			// �쐬�������X�g����J�����ɉf���Ă�����̂𒊏o
			hits = FilterTargetObject(hits);

			// �J�������烌�C���o���A���̃��C����ł��߂��I�u�W�F�N�g
			Ray ray = new Ray(transform.position, transform.forward);
			//	Debug.DrawRay(ray.origin, ray.direction * float.MaxValue, Color.cyan, 10);
			return GetNearestObjectIncludeRation(ray, hits);
		}





		/// <summary>
		/// GameObject���X�g���󂯎��A���C���J�����Ɉڂ��Ă�����̂����𒊏o����
		/// </summary>
		/// <param name="gameobjectList">�d��������GameObject���X�g</param>
		/// <returns>���C���J�����Ɉڂ��Ă�����̂������o�������X�g</returns>
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
			// �v���C���[�ʒu������G�͈͓��ɂ���G�����X�g������
			List<GameObject> hits = Physics.SphereCastAll(
				primaryTarget.transform.position,
				searchRadius,
				primaryTarget.transform.forward,
				0.01f,
				targetLayerMask
				).Select(h => h.transform.gameObject).ToList();

			hits.Remove(secondaryTarget);
			hits = FilterTargetObject(hits, lr);

			// �J�������烌�C���o���A���̃��C����ł��߂��I�u�W�F�N�g
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
		/// ����_����ł��߂��I�u�W�F�N�g��Ԃ�
		/// </summary>
		/// <param name="pos">�n�_</param>
		/// <param name="gameObjectsList">���ׂ������X�g</param>
		/// <returns>�ł��߂��I�u�W�F�N�g</returns>
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
		/// ray����ł��߂��I�u�W�F�N�g��Ԃ�
		/// </summary>
		/// <param name="ray">ray</param>
		/// <param name="gameObjectsList">���ׂ������X�g</param>
		/// <returns>�ł��߂��I�u�W�F�N�g</returns>
		private GameObject GetNearestObjectIncludeRation(Ray ray, List<GameObject> gameObjectsList)
		{
			if (0 < gameObjectsList.Count())
			{
				float minDistance = float.MaxValue;
				GameObject nearObj = null;

				foreach (var obj in gameObjectsList)
				{
					float rayToEnemyDistance = Vector3.Cross(ray.direction, obj.transform.position - ray.origin).magnitude;       // ���C�ƓG�̋���
					float playerToEnemyDistance = (obj.transform.position - primaryTarget.transform.position).magnitude; // �v���C���[�ƓG�̋���
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
					float distance = Vector3.Cross(ray.direction, obj.transform.position - ray.origin).magnitude;       // �J���������΂������C�ƓG�̋���
					float cameraToEnemyDistance = (obj.transform.position - primaryTarget.transform.position).magnitude; // �v���C���[�ƓG�̋���
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
		/// �J�����A�X�䓙�ύX
		/// </summary>
		private void SetLastValues()
		{
			lastAspect = cam.aspect;
		}





		/// <summary>
		/// �J�����U������
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
		/// �J�����U���J�n
		/// </summary>
		/// <param name="waitTime">�U������</param>
		/// <returns>�U���o�ߎ���</returns>
		IEnumerator SetShake(float waitTime)
		{
			shake = true;
			yield return new WaitForSeconds(waitTime);
			shake = false;
		}
		#endregion





		#region PublicMethod
		/// <summary>
		/// InputSystem�̓��͂Ȃǂ������Ƃ��ăJ�����̎��_����]����
		/// </summary>
		/// <param name="input">�J�����ړ���</param>
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
		/// �J������U��������
		/// </summary>
		/// <param name="power">�U���̑傫��</param>
		/// <param name="speed">�U���̑���</param>
		/// <param name="shakeTime">�U������</param>
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
		/// ���b�N�I�����Ă��邩�ǂ���
		/// </summary>
		/// <returns>true: ���b�N�I����	false:	���b�N�I�����ĂȂ�</returns>
		public bool IsLocking()
		{
			return lockOnTransform;
		}



		/// <summary>
		/// �J�������t���[���Ƀz�[�~���O���e�͈͂ɓG������΂���Transform��Ԃ�
		/// </summary>
		/// <param name="forward">�v���C���[�̌����Ă������</param>
		/// <param name="permissionDistance">���e����</param>
		/// <param name="permissionAngle">���e�p�x</param>
		/// <returns>�v���C���[����G�ւ̃x�N�g��	�Y���Ȃ��̂Ƃ��͑O��</returns>
		public Vector3 GetFreeHomingVector(float permissionDistance, float permissionAngle)
		{
			Vector3 ret = primaryTarget.transform.forward;
			Vector2 forward = new Vector2(primaryTarget.transform.forward.x, primaryTarget.transform.forward.z).normalized;

			// �v���C���[�ʒu������G�͈͓��ɂ���G�����X�g������
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

				// ���e�͈͂ɂ����āA����ԋ߂��G��T��
				foreach (var obj in hits)
				{
					float distance = (primaryTarget.transform.position - obj.transform.position).magnitude;

					if (distance < minDistance)
					{
						Vector3 playerToEnemy = obj.transform.position - primaryTarget.transform.position;
						// ������forward�Ǝ�������G�ւ̊p�x
						float angle = Mathf.Abs(Vector2.Angle(forward, new Vector2(playerToEnemy.x, playerToEnemy.z).normalized));
						//Debug.Log("�啽angle" + angle);
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

			// �v���C���[�ʒu������G�͈͓��ɂ���G�����X�g������
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

				// ���e�͈͂ɂ����āA����ԋ߂��G��T��
				foreach (var obj in hits)
				{
					float distance = (primaryTarget.transform.position - obj.transform.position).magnitude;

					if (distance < minDistance)
					{
						Vector3 playerToEnemy = obj.transform.position - primaryTarget.transform.position;
						// ������forward�Ǝ�������G�ւ̊p�x
						float angle = Mathf.Abs(Vector2.Angle(forward, new Vector2(playerToEnemy.x, playerToEnemy.z).normalized));
						//Debug.Log("�啽angle" + angle);
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
		/// ���b�N�I�����Ă��鑊�肪�D�����Ԃɂ��邩���ׂ�
		/// </summary>
		private void CheckSteal()
		{
			Debug.Log("�啽" + enableSteal);
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
		/// ���b�N�I�����O��
		/// ���b�N�I������ĂȂ���Ή����N���Ȃ�
		/// </summary>
		/// <param name="obj">�O�������Q�[���I�u�W�F�N�g</param>
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
		/// �t���[���b�N�ɐ؂�ւ�
		/// </summary>
		private void OnFreeLook()
		{
			behavior = FreeLookCamera;
			secondaryTarget = null;
			lockOnTransform = null;
			isLockOn = false;
		}


		/// <summary>
		/// �I�[�g���[�h�ɐ؂�ւ�
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
		/// ���b�N�I�����[�h�ɐ؂�ւ�
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