using UnityEngine;

namespace Ohira
{
	#region Require
	// 前提コンポーネントつけ忘れ防止
	[RequireComponent(typeof(Rigidbody))]
	#endregion

	public partial class Satellite : MonoBehaviour
	{
		#region StateInstance
		private static readonly SatelliteIdling stateIdling = new SatelliteIdling();
		private static readonly SatelliteHoming stateHoming = new SatelliteHoming();
		private static readonly SatelliteReturn stateReturn = new SatelliteReturn();
		private static readonly SatelliteAttack1 stateAttack1 = new SatelliteAttack1();
		private static readonly SatelliteAttack2 stateAttack2 = new SatelliteAttack2();
		private static readonly SatelliteAttack3 stateAttack3 = new SatelliteAttack3();
		private static readonly SatelliteFall satelliteFall = new SatelliteFall();
		#endregion

		#region constant
		const int MOBILLITY_WEAPON_NUM = 2;
		#endregion


		#region Inspector
		[Tooltip("プレイヤー")]
		[SerializeField] private GameObject player = null;
		[Tooltip("軌道の中心")]
		[SerializeField] private GameObject center = null;      // 軌道の中心

		[SerializeField] private Rigidbody rb = null;           // インスペクターからやったほうが早いらしいよ
		[SerializeField] private Collider bodyCol = null;
																//[SerializeField] private LayerMask obstacleLayerMask;
		[SerializeField] private AudioSource audioSource;

		[SerializeField] private SatelliteIdlingParameters idlingParameters;
		[SerializeField] private SatelliteCommonStealParameters stealParameters;


		[SerializeField] private Style startStyle;
		[SerializeField] private Style style;       // スタイル(ScriptableObjectでプレイヤーと共通のものを持つ)
		[SerializeField] private GameObject currentWeapon;  // 現在の武器
		private PlayerWeaponCollision weaponCollision;
		private AfterImageControllerBase afterImageController;
		private PlayerWeaponCollision[] weaponCollisions = new PlayerWeaponCollision[MOBILLITY_WEAPON_NUM];
		private AfterImageControllerBase[] afterImages = new AfterImageControllerBase[MOBILLITY_WEAPON_NUM];
		[SerializeField] private Transform weaponPool;  // 武器をプーリングする空のオブジェクト

		[SerializeField] private AttackInfo attackInfo;


		[SerializeField, EnumIndex(typeof(E_SATELLITE_TRAIL))] private Trail[] idleTrailRenderer = null;
		[SerializeField, EnumIndex(typeof(E_SATELLITE_VFX))] private GameObject[] vfxEffects;
		[SerializeField, EnumIndex(typeof(E_SATELLITE_BEAMCHARGE_VFX))] private GameObject[] vfxCharges;
		[SerializeField] private ComboParam comboParam;

		#endregion


		#region PrivateVariable
		private CameraController cameraController;
		private SatelliteStateBase currentState;
		private SphereCollider colliderr;
		private Maeda.Player playerScript;

		private float healValue = 0f;
		private bool resetRequest;

		delegate void Reservation();
		private Reservation ResetReservation;
		#endregion

		public PlayerControler input = null;



		#region PrivateMethod
		private void Awake()
		{
			//ChangeStyle(startStyle);
			cameraController = CameraController.Instance;
			ResetReservation = NullFunc;
			playerScript = player.GetComponent<Maeda.Player>();
		}

		private void Start()
		{
			if (rb == null)
				rb = GetComponent<Rigidbody>();
			rb.useGravity = false;
			rb.isKinematic = false;

			// メッシュコライダー
			colliderr = GetComponent<SphereCollider>();
			colliderr.enabled = false;

			ChangeStyle(startStyle);
			currentState = stateIdling;
			currentState.OnEnter(this);
			//ChangeState(stateIdling);


			input = new PlayerControler();
			input.Enable();

			PlayerWeaponCollision.comboParam = comboParam;
			resetRequest = true;
		}


		private void FixedUpdate()
		{
		//	preOrbitPos = curOrbitPos;
			currentState.OnFixedUpdate(this);
			//	curOrbitPos = transform.position;
			ResetReservation();
			resetRequest = true;
		}


		private void Update()
		{
			currentState.OnUpdate(this);
		}

		//private void LateUpdate()
		//{
		//}


		private void ChangeState(SatelliteStateBase nextState)
		{
			currentState.OnExit(this, nextState);
			nextState.OnEnter(this, currentState);
			currentState = nextState;
		}


		/// <summary>
		/// スタイル変更
		/// </summary>
		/// <param name="style">敵のStyleスクリプタブルオブジェクト</param>
		private void ChangeStyle(Style style)
		{
			this.style.style = style.style;
			InstWeapon(style);
			comboParam.EnqueHistory(style.style);
			ResetReservation = NullFunc;
		}


		/// <summary>
		/// 武器の持ち替え
		/// </summary>
		/// <param name="weapon">ぶき</param>
		private void InstWeapon(Style style)
		{
			// 現在の武器をオフ
			currentWeapon.SetActive(false);
			currentWeapon.transform.parent = weaponPool.transform;

			if (afterImageController != null)
			{
				//afterImageController.DisposeImages();
				afterImageController = null;
			}
			else
			{
				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					if (afterImages[i] == null)
					{
						continue;
					}
					//afterImages[i].DisposeImages();
					afterImages[i] = null;
				}
			}
			

			GameObject weapon = style.weapon;
			bool exist = false;
			// weaponPoolを探索
			foreach (Transform t in weaponPool)
			{
				// すでにプール内にある場合
				if (t.gameObject.name == weapon.gameObject.name)
				{
					// アクティブにする
					t.gameObject.SetActive(true);
					t.transform.parent = transform;
					currentWeapon = t.gameObject;
					exist = true;

					// Reach型
					if (style.style == Style.E_Style.REACH)
					{
						Transform muzzle = currentWeapon.GetComponent<Muzzle>().muzzle;
						Transform beamTrans = attackInfo.beam.transform;
						beamTrans.parent = muzzle;
						beamTrans.localPosition = Vector3.zero;
						beamTrans.localEulerAngles = Vector3.zero;

						// エフェクト系の位置
						for (int i = 0; i < vfxEffects.Length; i++)
						{
							Transform vfxTrans = vfxEffects[i].transform;
							vfxTrans.parent = muzzle;
							vfxTrans.localPosition = Vector3.zero;
						}
						foreach (var vfx in vfxCharges)
						{
							Transform vfxTrans = vfx.transform;
							vfxTrans.parent = muzzle;
							vfxTrans.localPosition = Vector3.zero;
						}
					}
				}
			}

			//----- プールになかった場合新規生成 -----
			if (exist == false)
			{
				if (style.style == Style.E_Style.MOBILITY)
				{
					// 機動型の場合4つ作る
					GameObject parent = new GameObject(weapon.name);
					parent.transform.parent = transform;
					currentWeapon = parent;
					for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
					{
						var newWeapon = Instantiate(weapon);
						newWeapon.transform.parent = parent.transform;
						newWeapon.gameObject.name = weapon.name + "_" + i;
					}
				}
				else
				{
					var newWeapon = Instantiate(weapon);
					newWeapon.transform.parent = transform;
					newWeapon.gameObject.name = weapon.name;
					currentWeapon = newWeapon;
				}

				// Reach型
				if (style.style == Style.E_Style.REACH)
				{
					Transform muzzle = currentWeapon.GetComponent<Muzzle>().muzzle;
					Transform beamTrans = attackInfo.beam.transform;
					beamTrans.parent = muzzle;
					beamTrans.localPosition = Vector3.zero;
					beamTrans.localEulerAngles = Vector3.zero;

					// エフェクト系の位置
					for (int i = 0; i < vfxEffects.Length; i++)
					{
						Transform vfxTrans = vfxEffects[i].transform;
						vfxTrans.parent = muzzle;
						vfxTrans.localPosition = Vector3.zero;
					}
					foreach (var vfx in vfxCharges)
					{
						Transform vfxTrans = vfx.transform;
						vfxTrans.parent = muzzle;
						vfxTrans.localPosition = Vector3.zero;
					}
				}
			}

			// 座標を設定
			currentWeapon.transform.localPosition = new Vector3(0f, 0f, 0f);
			//	currentWeapon.transform.position = weaponPool.transform.position;
			currentWeapon.transform.rotation = weaponPool.transform.rotation;

			if (style.style == Style.E_Style.MOBILITY)
			{
				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					var weapo = currentWeapon.transform.GetChild(i).gameObject;

					weaponCollisions[i] = weapo.GetComponentInChildren<PlayerWeaponCollision>();
					afterImages[i] = weapo.GetComponent<AfterImageController>();
				}
			}
			else
			{
				weaponCollision = currentWeapon.GetComponentInChildren<PlayerWeaponCollision>();
				afterImageController = currentWeapon.GetComponent<AfterImageController>();
			}

			// メッシュコライダー変形
			//TransMeshCollider();
		}


		/// <summary>
		/// メッシュコライダーの変形
		/// </summary>
		//private void TransMeshCollider(GameObject obj)
		//{
		//	colliderr = obj.GetComponent<MeshCollider>();
		//}



		private void ActiveIdlingTrail()
		{
			for (int i = 0; i < idleTrailRenderer.Length; i++)
			{
				idleTrailRenderer[i].trailRenderer.time = idleTrailRenderer[i].regulationTime;
			}
		}


		private void InActiveIdlingTrail()
		{
			for (int i = 0; i < idleTrailRenderer.Length; i++)
			{
				idleTrailRenderer[i].trailRenderer.time = 0f;
			}
		}


		private void NullFunc()
		{

		}
		#endregion


		#region PublicMethod
		/// <summary>
		/// スタイルをノーマルに戻す
		/// アイドリングステートにいないときはこれを予約しておく
		/// </summary>
		public void ResetSatelliteStyle()
		{
			if (resetRequest == false)
				return;

			if (currentState != stateIdling)
			{
				ResetReservation = ResetSatelliteStyle;
				return;
			}
			ChangeStyle(startStyle);
			ChangeState(stateReturn);
			ResetReservation = NullFunc;
		}


		public void OnResultStart()
		{
			input.Disable();
			ChangeState(satelliteFall);
		}

        public void PlaySE(string se_name)
        {
            var soundManager = Mizuno.SoundManager.Instance;
         
            soundManager.PlayMenuSe(se_name);
        }

		public void InputDisable()
		{
			input.Disable();
		}

		public void InputEnable()
		{
			input.Enable();
		}

        #endregion

        #region OnPhysics
        private void OnTriggerEnter(Collider other)
		{
			currentState.OnTriggerEnter(this, other);
		}
		#endregion


		#region OnInputSystem
		private void OnSteal()
		{
			currentState.OnSteal(this);
		}


		private void OnAttack()
		{
			currentState.OnAttack(this);
		}
		#endregion






		#region Struct
		[System.Serializable]
		struct Trail
		{
			public TrailRenderer trailRenderer;
			public float regulationTime;
		}
		#endregion



	


	}
}