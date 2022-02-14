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
		//	private Vector3 velocity;           // 速度
		//	private float maxCentripetalAccel;  // ターゲットを中心としたときの最大向心力
		//	private float propulsion;           // 推進力
		//	private float damping;              // 減衰率
		//	private Transform target;           // ターゲット
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
		//		Vector3 toTarget = target.position - owner.transform.position;      // ターゲットへ向かうベクトル
		//		Vector3 vn = velocity.normalized;
		//		float dot = Vector3.Dot(toTarget, vn);
		//		Vector3 centripetalAccel = toTarget - (vn * dot);                   // ターゲットを中心としたときの向心力(加速度)

		//		// 向心力の大きさが1より大きければ正規化(1未満の時はそのまま)
		//		if (centripetalAccel.magnitude > 1f)
		//		{
		//			centripetalAccel.Normalize();
		//		}
		//		centripetalAccel *= maxCentripetalAccel;
		//		centripetalAccel += vn * propulsion;        // 推進力
		//		centripetalAccel -= velocity * damping;     // 減衰
		//		velocity += centripetalAccel * Time.fixedDeltaTime;  // 速度積分
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
		//	/// ホーミングパラメータの初期化
		//	/// </summary>
		//	/// <param name="target">ターゲット</param>
		//	/// <param name="velocity">速度</param>
		//	/// <param name="speed">速さ</param>
		//	/// <param name="curvatureRadius">曲率半径(追尾の鋭さ)</param>
		//	/// <param name="damping">減衰</param>
		//	public void Initialize(Transform target, Vector3 velocity, float speed, float curvatureRadius, float damping)
		//	{
		//		this.target = target;
		//		this.velocity = velocity;
		//		// 速さv,半径rで円を描くとき、その向心力はv^2/r
		//		this.maxCentripetalAccel = speed * speed / curvatureRadius;
		//		this.damping = damping;
		//		// 終端速度がspeedになるaccelを求める
		//		// v = a / k だから a = v * k
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