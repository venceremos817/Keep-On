using System;
using UnityEngine;

namespace Ohira
{
	public class TutorialEnemySpawner : MonoBehaviour
	{
		[Serializable]
		public enum ENEMY_KIND : int
		{
			POWER = 0,
			REACH,
			MOBILLITY,
			HEAL
		}

		[SerializeField, EnumIndex(typeof(ENEMY_KIND))] private GameObject[] enemys;
		[SerializeField, EnumIndex(typeof(ENEMY_KIND))] private Transform[] enemyPools;


		private void Awake()
		{
			// �X�|�i�[�������ׂẴG�l�~�[������
			AllDisactive();
		}



		#region �p�u���b�N�֐�
		/// <summary>
		/// �G���X�|�[��
		/// </summary>
		/// <param name="kind">�X�|�[�������������</param>
		/// <param name="pos">pos</param>
		/// <param name="rot">rot</param>
		public GameObject SpawnEnemy(ENEMY_KIND kind, Vector3 pos, Vector3 forward)
		{
			GameObject retObj = null;
			// �v�[����T�����Ė��g�p�Ȃ��̂��A�N�e�B�u�ɂ���
			Transform pool = enemyPools[(int)kind];
			bool succes = false;
			for (int i = 0; i < pool.childCount; i++)
			{
				GameObject enemy = pool.GetChild(i).gameObject;
				if (enemy.activeSelf) { continue; }
				enemy.SetActive(true);
				enemy.transform.position = pos;
				enemy.transform.forward = forward;
				retObj = enemy;
				succes = true;
				break;
			}

			// �v�[�����ŃA�N�e�B�u�ɂł��Ȃ���ΐV������������
			if (succes == false)
			{
				GameObject enemy = Instantiate(enemys[(int)kind]);
				enemy.transform.position = pos;
				enemy.transform.forward = forward;
				enemy.transform.parent = pool;
				enemy.name = enemys[(int)kind].name;
				retObj = enemy;
			}

			return retObj;
		}


		/// <summary>
		/// �X�|�i�[�������ׂẴG�l�~�[������
		/// </summary>
		public void AllDisactive()
		{
			for (int i = 0; i < enemyPools.Length; i++)
			{
				for (int j = 0; j < enemyPools[i].childCount; j++)
				{
					GameObject enemy = enemyPools[i].transform.GetChild(j).gameObject;
					CameraController.Instance.RemoveLockOn(enemy);
					enemy.SetActive(false);
				}
			}
		}


		public bool IsEnemyCompleteDestroy()
		{
			for (int i = 0; i < (int)ENEMY_KIND.HEAL; i++)
			{
				for (int j = 0; j < enemyPools[i].childCount; j++)
				{
					GameObject enemy = enemyPools[i].transform.GetChild(j).gameObject;
					if (enemy.activeSelf)
					{
						return false;
					}
				}
			}

			return true;
		}
		#endregion
	}
}