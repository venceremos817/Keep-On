using UnityEngine;

namespace Ohira
{
	#region Require
	// �O��R���|�[�l���g���Y��h�~
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
		[Tooltip("�v���C���[")]
		[SerializeField] private GameObject player = null;
		[Tooltip("�O���̒��S")]
		[SerializeField] private GameObject center = null;      // �O���̒��S

		[SerializeField] private Rigidbody rb = null;           // �C���X�y�N�^�[���������ق��������炵����
		[SerializeField] private Collider bodyCol = null;
																//[SerializeField] private LayerMask obstacleLayerMask;
		[SerializeField] private AudioSource audioSource;

		[SerializeField] private SatelliteIdlingParameters idlingParameters;
		[SerializeField] private SatelliteCommonStealParameters stealParameters;


		[SerializeField] private Style startStyle;
		[SerializeField] private Style style;       // �X�^�C��(ScriptableObject�Ńv���C���[�Ƌ��ʂ̂��̂�����)
		[SerializeField] private GameObject currentWeapon;  // ���݂̕���
		private PlayerWeaponCollision weaponCollision;
		private AfterImageControllerBase afterImageController;
		private PlayerWeaponCollision[] weaponCollisions = new PlayerWeaponCollision[MOBILLITY_WEAPON_NUM];
		private AfterImageControllerBase[] afterImages = new AfterImageControllerBase[MOBILLITY_WEAPON_NUM];
		[SerializeField] private Transform weaponPool;  // ������v�[�����O�����̃I�u�W�F�N�g

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

			// ���b�V���R���C�_�[
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
		/// �X�^�C���ύX
		/// </summary>
		/// <param name="style">�G��Style�X�N���v�^�u���I�u�W�F�N�g</param>
		private void ChangeStyle(Style style)
		{
			this.style.style = style.style;
			InstWeapon(style);
			comboParam.EnqueHistory(style.style);
			ResetReservation = NullFunc;
		}


		/// <summary>
		/// ����̎����ւ�
		/// </summary>
		/// <param name="weapon">�Ԃ�</param>
		private void InstWeapon(Style style)
		{
			// ���݂̕�����I�t
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
			// weaponPool��T��
			foreach (Transform t in weaponPool)
			{
				// ���łɃv�[�����ɂ���ꍇ
				if (t.gameObject.name == weapon.gameObject.name)
				{
					// �A�N�e�B�u�ɂ���
					t.gameObject.SetActive(true);
					t.transform.parent = transform;
					currentWeapon = t.gameObject;
					exist = true;

					// Reach�^
					if (style.style == Style.E_Style.REACH)
					{
						Transform muzzle = currentWeapon.GetComponent<Muzzle>().muzzle;
						Transform beamTrans = attackInfo.beam.transform;
						beamTrans.parent = muzzle;
						beamTrans.localPosition = Vector3.zero;
						beamTrans.localEulerAngles = Vector3.zero;

						// �G�t�F�N�g�n�̈ʒu
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

			//----- �v�[���ɂȂ������ꍇ�V�K���� -----
			if (exist == false)
			{
				if (style.style == Style.E_Style.MOBILITY)
				{
					// �@���^�̏ꍇ4���
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

				// Reach�^
				if (style.style == Style.E_Style.REACH)
				{
					Transform muzzle = currentWeapon.GetComponent<Muzzle>().muzzle;
					Transform beamTrans = attackInfo.beam.transform;
					beamTrans.parent = muzzle;
					beamTrans.localPosition = Vector3.zero;
					beamTrans.localEulerAngles = Vector3.zero;

					// �G�t�F�N�g�n�̈ʒu
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

			// ���W��ݒ�
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

			// ���b�V���R���C�_�[�ό`
			//TransMeshCollider();
		}


		/// <summary>
		/// ���b�V���R���C�_�[�̕ό`
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
		/// �X�^�C�����m�[�}���ɖ߂�
		/// �A�C�h�����O�X�e�[�g�ɂ��Ȃ��Ƃ��͂����\�񂵂Ă���
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