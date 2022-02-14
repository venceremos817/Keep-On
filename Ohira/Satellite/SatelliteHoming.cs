using UnityEngine;

namespace Ohira
{
	//[System.Serializable]
	//class SatelliteHomingParameters
	//{
	//	public float countPerMeter = 1f;
	//	public float speed = 1f;
	//	public float curvatureRadius = 1f;
	//	public float damping = 0.1f;
	//	public float impact = 1f;
	//}

	[System.Serializable]
	class SatelliteCommonStealParameters
	{
		//public float speed = 1f;
		//[Tooltip("�U���̋���")]
		//[Range(0.0001f, 0.5f)] public float curvePower = 0.3f;
		[System.NonSerialized] public Vector3 velocity;
		public Transform defaultTarget;
	}


	public partial class Satellite
	{
		public class SatelliteHoming : SatelliteStateBase
		{
			#region PrivateVariable
			private Transform target;
			//bool lockOn = false;
			private float period;
			#endregion


			#region OverrideMethod
			public override void OnEnter(Satellite owner, SatelliteStateBase prevState = null)
			{
				Transform target;
				if (owner.cameraController.IsLocking() == true)
				{
					// ���b�N�I�����Ă���΃^�[�Q�b�g��
					target = owner.cameraController.lockOnTransform;
					//	lockOn = true;
				}
				else
				{
					// ���b�N�I�����ĂȂ���ΑO���ɔ�΂�
					//target = owner.transform;
					//target.transform.position += owner.transform.forward * 10;

					//target = owner.stealParameters.defaultTarget;
					target = owner.cameraController.GetFreeHomingTransform(10, 45, owner.stealParameters.defaultTarget);
					//	lockOn = false;
				}
				// �v���C���[����O���֔�΂�
				Vector3 toTarget = owner.transform.position - target.transform.position;
				Vector3 initialVel = (owner.transform.position - owner.center.transform.position).normalized * toTarget.magnitude * 0.5f /** 10*/;
				float period = toTarget.magnitude * 0.05f;

				owner.colliderr.enabled = true;
				Init(owner, initialVel, target, period);
				owner.ActiveIdlingTrail();
				owner.healValue = -0.1f;
			}


			public override void OnFixedUpdate(Satellite owner)
			{
				Vector3 acceleration = Vector3.zero;
				Vector3 prePos = owner.transform.position;
				Vector3 position = owner.transform.position;

				var diff = target.position - position;
				acceleration += (diff - owner.stealParameters.velocity * period) * 2f
					/ (period * period);

				period -= Time.fixedDeltaTime;
				if (period < 0f)
				{
					owner.ChangeState(stateReturn);
					return;
				}

				owner.stealParameters.velocity += acceleration * Time.fixedDeltaTime;
				position += owner.stealParameters.velocity * Time.fixedDeltaTime;
				owner.rb.MovePosition(position);

				Rotation(owner, prePos, position);

				ObstacleDetection(owner, prePos, position);
			}


			public override void OnExit(Satellite owner, SatelliteStateBase nextState = null)
			{
				owner.colliderr.enabled = false;
				owner.currentWeapon.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			}
			#endregion

			#region PrivateMethod
			/// <summary>
			/// 
			/// </summary>
			/// <param name="position"></param>
			/// <param name="velocity"></param>
			/// <param name="target"></param>
			/// <param name="period"></param>
			private void Init(Satellite owner, Vector3 velocity, Transform target, float period)
			{
				owner.stealParameters.velocity = velocity;
				this.target = target;
				this.period = period;
			}


			/// <summary>
			/// �i�s�����������Ȃ�����񂷂�
			/// </summary>
			/// <param name="owner"></param>
			/// <param name="prePos"></param>
			protected void Rotation(Satellite owner, Vector3 prePos, Vector3 curPos)
			{
				Vector3 direction = (curPos - prePos).normalized;    // �i�s����
				direction = Vector3.Lerp(owner.transform.forward, direction, 0.3f);
				owner.transform.forward = direction;          // �i�s��������������
				owner.rb.angularVelocity = owner.transform.forward * owner.idlingParameters.rotateSpeed * 10;    // ����
			}


			private void ObstacleDetection(Satellite owner, Vector3 prePos, Vector3 pos)
			{
				//Vector3 direction = prePos - pos;
				//RaycastHit hitInfo;
				//if (Physics.SphereCast(prePos, 0.3f, direction, out hitInfo, direction.magnitude, owner.obstacleLayerMask) ||
				//	Physics.Raycast(pos, direction, out hitInfo, direction.magnitude, owner.obstacleLayerMask))
				//{
				//	owner.transform.position += hitInfo.normal * hitInfo.distance;
				////	Debug.Break();
				//}
			}
			#endregion


			#region OnHit
			public override void OnTriggerEnter(Satellite owner, Collider other)
			{
				Steal(owner, other);
			}


			private void Steal(Satellite owner, Collider other)
			{
				bool isEnemy = other.gameObject.TryGetComponent(out StyleHolder style);
				if (isEnemy)
				{
					// �ʏ�G3��
					if (style.style.style != Style.E_Style.HEAL)
					{
						Homare.Enemy enemyCS;// = other.GetComponent<Homare.Enemy>();
						isEnemy = other.TryGetComponent(out enemyCS);
						if (isEnemy && enemyCS.isWeapon)
						{
							// ���ʓG
							owner.playerScript.OnStartComboTime();
							owner.ChangeStyle(style.style);
							owner.ChangeState(stateReturn);
							owner.ResetReservation = owner.NullFunc;

							enemyCS.OnHitPlayerSteal();

							// �G�t�F�N�g
							OrbManager.Instance.EmitOrb(owner.transform.position, style.style.style);
							owner.GetComponent<EffectOperate>().PlayEffect(0);

                            // �T�E���h
                            owner.PlaySE("SE_Heal");
						}
						else
						{
							// �T���h�o�b�N
							isEnemy = other.TryGetComponent(out SandBack sandBackCS);
							if (isEnemy && sandBackCS.isWeapon)
							{
								owner.playerScript.OnStartComboTime();
								owner.ChangeStyle(style.style);
								owner.ChangeState(stateReturn);
								owner.ResetReservation = owner.NullFunc;

								sandBackCS.OnHitPlayerSteal();

								// �G�t�F�N�g
								OrbManager.Instance.EmitOrb(owner.transform.position, style.style.style);
								owner.GetComponent<EffectOperate>().PlayEffect(0);
							}
						}
					}
					else
					{
						// �q�[���G
						// �߂��قǉ񕜂���
						//owner.healValue = 1.0f / Vector3.Distance(owner.center.transform.position, other.transform.position) * 200;
                        // �񕜗ʈ��
						owner.healValue = 20;
						owner.ResetReservation = owner.NullFunc;
						owner.resetRequest = false;

					}

				}
				else if (other.TryGetComponent(out BossHitCollider boss))
				{
					// �{�X
					if (boss.OnHitPlayerSteal())
					{
						style = other.gameObject.transform.root.GetChild(0).GetComponent<StyleHolder>();
						owner.playerScript.OnStartComboTime();
						owner.ChangeStyle(style.style);
						owner.ChangeState(stateReturn);
						owner.ResetReservation = owner.NullFunc;

						// �G�t�F�N�g
						OrbManager.Instance.EmitOrb(owner.transform.position, style.style.style);
						owner.GetComponent<EffectOperate>().PlayEffect(0);

                        // �T�E���h
                        owner.PlaySE("SE_Heal");
                    }
					else
					{
						owner.stealParameters.velocity *= -1f;
					}
				}
			}
			#endregion
		}
	}
}