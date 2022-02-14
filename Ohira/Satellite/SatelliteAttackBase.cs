using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ohira.Auxiliary;

namespace Ohira
{
	public partial class Satellite
	{
		[System.Serializable]
		class AttackInfo
		{
			[SerializeField] public LayerMask enemyLayer;
			[SerializeField] public GameObject beam;
			[SerializeField,Tooltip("�r�[���̃_���[�W���肪�o��Ԋu(�b)")] public float beamDamageRate = 0.2f;
		}

		public abstract class SatelliteAttackBase : SatelliteStateBase
		{

			#region ProtectedVal
			#region Delegate
			protected delegate void AttackEnter(Satellite owner);
			protected AttackEnter enter;
			protected delegate void AttackUpdate(Satellite owner);
			protected AttackUpdate fixUpdate;
			protected delegate void AttackExit(Satellite owner, SatelliteStateBase nextState = null);
			protected AttackExit exit;
			#endregion

			protected AttackParam.AttackTime attackParam;
			protected float progressTime;
			protected float progressTimeLap;
			protected float progressUnScaleTimeLap;
			protected Vector3 startPosition;
			protected Quaternion startRotation;
			protected bool derive = false;        // �h�����邩
			// Mobility�p
			protected GameObject[] controlWeapons = new GameObject[MOBILLITY_WEAPON_NUM];
			protected Vector3[] startPositions = new Vector3[MOBILLITY_WEAPON_NUM];
			protected Quaternion[] startRotations = new Quaternion[MOBILLITY_WEAPON_NUM];
			protected int injection;
			// Reach�p
			protected Transform muzzle;

			#endregion



			public override void OnEnter(Satellite owner, SatelliteStateBase prevState = null)
			{
				// �X�^�C���̏�Ԃɍ��킹�ăf���Q�[�g��ݒ�
				switch (owner.style.style)
				{
					case Style.E_Style.NORMAL:
						enter = PowerEnter;//NormalEnter;
						fixUpdate = PowerOccurence; ;//NormalUpdate;
						exit = PowerExit;//NormalExit;
						break;

					case Style.E_Style.POWER:
						enter = PowerEnter;
						fixUpdate = PowerOccurence;
						exit = PowerExit;
						break;

					case Style.E_Style.MOBILITY:
						enter = MobillityEnter;
						fixUpdate = MobillityOccurence;
						exit = MobillityExit;
						break;

					case Style.E_Style.REACH:
						enter = ReachEnter;
						fixUpdate = ReachOccurence;
						exit = ReachExit;
						break;
				}
				owner.colliderr.enabled = false;
				startPosition = owner.transform.position;
				startRotation = owner.transform.rotation;
				progressTime = 0f;
				progressTimeLap = 0f;
				progressUnScaleTimeLap = 0f;
				derive = false;
				enter(owner);
			}


			public override void OnUpdate(Satellite owner)
			{
				if (owner.input.Player.Attack.triggered)
				{
					OnAttack(owner);
				}
			}



			public override void OnFixedUpdate(Satellite owner)
			{
				progressTime += Time.fixedDeltaTime;
				progressTimeLap += Time.fixedDeltaTime;
				//startRealTime = Time.realtimeSinceStartup;
				progressUnScaleTimeLap += Time.fixedUnscaledDeltaTime;
				fixUpdate(owner);
			}


			public override void OnExit(Satellite owner, SatelliteStateBase nextState = null)
			{
				exit(owner, nextState);
			}


			#region Normal
			/// <summary>
			/// �O����
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void NormalEnter(Satellite owner);


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void NormalOccurence(Satellite owner);


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void NormalSustain(Satellite owner);


			/// <summary>
			/// �d��
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void NormalRigidity(Satellite owner);


			/// <summary>
			/// �㏈��
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void NormalExit(Satellite owner);
			#endregion



			#region Power
			/// <summary>
			/// �O����
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void PowerEnter(Satellite owner);


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void PowerOccurence(Satellite owner);


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void PowerSustain(Satellite owner);


			/// <summary>
			/// �d��
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void PowerRigidity(Satellite owner);


			/// <summary>
			/// �㏈��
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void PowerExit(Satellite owner, SatelliteStateBase nextState);
			#endregion




			#region Mobilility
			/// <summary>
			/// �O����
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void MobillityEnter(Satellite owner);


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void MobillityOccurence(Satellite owner);


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void MobillitySustain(Satellite owner);


			/// <summary>
			/// �d��
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void MobillityRigidity(Satellite owner);


			/// <summary>
			/// �㏈��
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void MobillityExit(Satellite owner, SatelliteStateBase nextState = null);
			#endregion



			#region Reach
			/// <summary>
			/// �O����
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void ReachEnter(Satellite owner);


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void ReachOccurence(Satellite owner);


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void ReachSustain(Satellite owner);


			/// <summary>
			/// �d��
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void ReachRigidity(Satellite owner);


			/// <summary>
			/// �㏈��
			/// </summary>
			/// <param name="owner"></param>
			protected abstract void ReachExit(Satellite owner, SatelliteStateBase nextState);



			#endregion


			protected void InputReception()
			{
				if (progressTime.IsInRnage(attackParam.bufferInput.start, attackParam.bufferInput.end))
					derive = true;
			}



			protected void ActiveEffect(GameObject obj, float time)
			{
				var trail = obj.GetComponent<TrailRenderer>();
				trail.enabled = true;
				trail.time = 0;
				trail.time = time;
			}


			protected void InActiveEffect(GameObject obj)
			{
				var trai = obj.GetComponent<TrailRenderer>();
				trai.enabled = false;
			}




			#region OnInput
			public override void OnAttack(Satellite owner)
			{
				//derive = true;
				InputReception();
			}
			#endregion
		}
	}
}