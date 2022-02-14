using UnityEngine;

namespace Ohira
{
	public abstract class SatelliteStateBase
	{
		/// <summary>
		/// ステート開始時に呼ばれる
		/// </summary>
		/// <param name="prevState">前の状態</param>
		public virtual void OnEnter(Satellite owner, SatelliteStateBase prevState = null) { }

		/// <summary>
		/// 毎フレーム呼ばれる
		/// </summary>
		public virtual void OnFixedUpdate(Satellite owner) { }


		public virtual void OnUpdate(Satellite owner) { }

		/// <summary>
		/// ステート終了時に呼ばれる
		/// </summary>
		/// <param name="">次の状態</param>
		public virtual void OnExit(Satellite owner, SatelliteStateBase nextState = null) { }


		#region OnPhyisics
		public virtual void OnTriggerEnter(Satellite owner, Collider other) { }
		#endregion


		#region OnInputSystem
		// キーが押されたとき
		public virtual void OnSteal(Satellite owner) { }

		public virtual void OnAttack(Satellite owner) { }
		#endregion





	}
}