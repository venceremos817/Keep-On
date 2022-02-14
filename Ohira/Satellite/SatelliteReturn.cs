using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ohira
{
	public partial class Satellite
	{
		//public class SatelliteReturn : SatelliteStateBase
		//{
		//	#region PrivateVariable
		//	private Vector3 velocity;           // ���x
		//	private float maxCentripetalAccel;  // �^�[�Q�b�g�𒆐S�Ƃ����Ƃ��̍ő���S��
		//	private float propulsion;           // ���i��
		//	private float damping;              // ������
		//	private Transform target;           // �^�[�Q�b�g
		//	private int returnAge = 0;
		//	#endregion

		//	#region OverrideMethod
		//	public override void OnEnter(Satellite owner, SatelliteStateBase prevState = null)
		//	{
		//		Transform target;
		//		target = owner.player.transform;
		//		Vector3 initialVel = (owner.transform.forward).normalized /** 100*/;
		//		//Vector3 initialVel = (owner.preOrbitPos - owner.curOrbitPos);
		//		Vector3 toTarget = owner.transform.position - target.transform.position;
		//		float speed = owner.homingParameters.speed;
		//		Initialize(target, initialVel, speed, toTarget.magnitude * owner.homingParameters.curvePower, 0.1f);

		//		returnAge = 0;
		//	}


		//	public override void OnFixedUpdate(Satellite owner)
		//	{
		//		Vector3 toTarget = target.position - owner.transform.position;      // �^�[�Q�b�g�֌������x�N�g��
		//		Vector3 vn = velocity.normalized;
		//		float dot = Vector3.Dot(toTarget, vn);
		//		Vector3 centripetalAccel = toTarget - (vn * dot);                   // �^�[�Q�b�g�𒆐S�Ƃ����Ƃ��̌��S��(�����x)

		//		// ���S�͂̑傫����1���傫����ΐ��K��(1�����̎��͂��̂܂�)
		//		if (centripetalAccel.magnitude > 1f)
		//		{
		//			centripetalAccel.Normalize();
		//		}
		//		centripetalAccel *= maxCentripetalAccel;
		//		centripetalAccel += vn * propulsion;        // ���i��
		//		centripetalAccel -= velocity * damping;     // ����
		//		velocity += centripetalAccel * Time.fixedDeltaTime;  // ���x�ϕ�
		//		Vector3 position = owner.transform.position + velocity * Time.fixedDeltaTime;

		//		owner.rb.MovePosition(position);
		//		owner.transform.LookAt(target);
		//		//owner.transform.rotation = Quaternion.FromToRotation(velocity, owner.transform.forward) * owner.transform.rotation;


		//		if (returnAge > 10000)
		//			owner.ChangeState(stateIdling);
		//	}


		//	public override void OnExit(Satellite owner, SatelliteStateBase nextState = null)
		//	{
		//	}
		//	#endregion



		//	public override void OnTriggerEnter(Satellite owner, Collider other)
		//	{
		//		if (other.gameObject == target.gameObject)
		//			owner.ChangeState(stateIdling);
		//	}


		//	#region PublicMethod
		//	/// <summary>
		//	/// �z�[�~���O�p�����[�^�̏�����
		//	/// </summary>
		//	/// <param name="target">�^�[�Q�b�g</param>
		//	/// <param name="velocity">���x</param>
		//	/// <param name="speed">����</param>
		//	/// <param name="curvatureRadius">�ȗ����a(�ǔ��̉s��)</param>
		//	/// <param name="damping">����</param>
		//	public void Initialize(Transform target, Vector3 velocity, float speed, float curvatureRadius, float damping)
		//	{
		//		this.target = target;
		//		this.velocity = velocity;
		//		// ����v,���ar�ŉ~��`���Ƃ��A���̌��S�͂�v^2/r
		//		this.maxCentripetalAccel = speed * speed / curvatureRadius;
		//		this.damping = damping;
		//		// �I�[���x��speed�ɂȂ�accel�����߂�
		//		// v = a / k ������ a = v * k
		//		this.propulsion = speed * damping;
		//	}
		//	#endregion


		public class SatelliteReturn : SatelliteHoming
		{
			#region PrivateVariable
			Transform target;
			float period;
			#endregion


			#region OverrideMethod
			public override void OnEnter(Satellite owner, SatelliteStateBase prevState = null)
			{
				Transform target = owner.center.transform;

				owner.currentWeapon.transform.localPosition = Vector3.zero;
				owner.currentWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);


				Vector3 toTarget = owner.transform.position - target.transform.position;
				Vector3 initialVel = owner.stealParameters.velocity * 0.8f;
				float period = toTarget.magnitude * 0.1f;

				owner.colliderr.enabled = false;
				Init(initialVel, target, period);
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
					owner.ChangeState(stateIdling);

					if (owner.healValue > 0f)
						owner.playerScript.OnHeal(owner.healValue);
					owner.healValue = -0.1f;

					return;
				}

				owner.stealParameters.velocity += acceleration * Time.fixedDeltaTime;
				position += owner.stealParameters.velocity * Time.fixedDeltaTime;
				owner.rb.MovePosition(position);

				Rotation(owner, prePos, position);
			}


			public override void OnExit(Satellite owner, SatelliteStateBase nextState = null)
			{
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
			private void Init(Vector3 velocity, Transform target, float period)
			{
				//this.velocity = velocity;
				this.target = target;
				this.period = period;
			}
			#endregion


			public override void OnTriggerEnter(Satellite owner, Collider other)
			{
				if (other.gameObject == target.gameObject)
					owner.ChangeState(stateIdling);
			}
		}
	}
}