using UnityEngine;

namespace Ohira
{
	public abstract class SatelliteStateBase
	{
		/// <summary>
		/// �X�e�[�g�J�n���ɌĂ΂��
		/// </summary>
		/// <param name="prevState">�O�̏��</param>
		public virtual void OnEnter(Satellite owner, SatelliteStateBase prevState = null) { }

		/// <summary>
		/// ���t���[���Ă΂��
		/// </summary>
		public virtual void OnFixedUpdate(Satellite owner) { }


		public virtual void OnUpdate(Satellite owner) { }

		/// <summary>
		/// �X�e�[�g�I�����ɌĂ΂��
		/// </summary>
		/// <param name="">���̏��</param>
		public virtual void OnExit(Satellite owner, SatelliteStateBase nextState = null) { }


		#region OnPhyisics
		public virtual void OnTriggerEnter(Satellite owner, Collider other) { }
		#endregion


		#region OnInputSystem
		// �L�[�������ꂽ�Ƃ�
		public virtual void OnSteal(Satellite owner) { }

		public virtual void OnAttack(Satellite owner) { }
		#endregion





	}
}