using UnityEngine;
using Ohira.Auxiliary;
using Homare;

namespace Ohira
{
	public class SandBack : MonoBehaviour
	{
		[SerializeField, Tooltip("武器	つけたり消したりする")] private GameObject[] weapons;
		[SerializeField, Tooltip("ステータス")] public EnemyStatus status;

		private float _Hp;
		public float Hp
		{
			get => _Hp;
			private set
			{
				_Hp = Mathf.Min(value, status.maxHp);
				if (_Hp <= 0)
				{
					_Hp = 0;
					Death();
				}
			}
		}

		private DamageText damageText;

		[System.NonSerialized] public bool isWeapon;



		private void Start()
		{
			Init();
		}

		private void OnEnable()
		{
			Init();
		}

		private void Update()
		{
			transform.forward = transform.ApproachRotate(TutorialManager.Instance.playerTrans, 0.3f);
		}


		public void Init()
		{
			isWeapon = true;
			SetActiveWeapons(true);
			Hp = status.maxHp;
		}

		#region パブリック関数
		/// <summary>
		/// 奪われたとき
		/// </summary>
		public void OnHitPlayerSteal()
		{
			isWeapon = false;
			SetActiveWeapons(false);
		}


		public void OnHitPlayerAttack(float damage)
		{
			Hp -= damage;

			foreach (Transform t in TutorialManager.Instance.damageUICanvus.transform)
			{
				if (!t.gameObject.activeSelf)
				{
					damageText = t.GetComponent<DamageText>();
					t.gameObject.SetActive(true);
					break;
				}
			}
			damageText?.GetDrawPos(transform.position + new Vector3(0, 1.5f, 0), damage);
		}
		#endregion

		#region プライベート関数
		private void SetActiveWeapons(bool active)
		{
			foreach (var weapon in weapons)
			{
				weapon.SetActive(active);
			}
		}


		private void Death()
		{
			CameraController.Instance.RemoveLockOn(gameObject);
			gameObject.SetActive(false);
		}
		#endregion
	}
}