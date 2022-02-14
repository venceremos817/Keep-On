using UnityEngine;
using Homare;

namespace Ohira
{
	[RequireComponent(typeof(BoxCollider))]
	public class EnemyDeadWall : MonoBehaviour
	{
		private void OnTriggerStay(Collider other)
		{
			if (other.TryGetComponent(out Enemy enemyCS))
			{
				enemyCS.OnHitPlayerAttack(5000);
			}
			else if (other.TryGetComponent(out SandBack sandBackCS))
			{
				sandBackCS.OnHitPlayerAttack(5000);
			}
			//else if (other.TryGetComponent(out BossHitCollider boss))
			//{
			//	// É{ÉX
			//	boss.OnHitPlayerAttack(50000, boss.transform.position);
			//}			
		}
	}
}