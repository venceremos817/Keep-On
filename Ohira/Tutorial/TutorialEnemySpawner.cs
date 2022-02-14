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
			// スポナーがもつすべてのエネミーを消す
			AllDisactive();
		}



		#region パブリック関数
		/// <summary>
		/// 敵をスポーン
		/// </summary>
		/// <param name="kind">スポーンさせたい種類</param>
		/// <param name="pos">pos</param>
		/// <param name="rot">rot</param>
		public GameObject SpawnEnemy(ENEMY_KIND kind, Vector3 pos, Vector3 forward)
		{
			GameObject retObj = null;
			// プールを探索して未使用なものをアクティブにする
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

			// プール内でアクティブにできなければ新しく生成する
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
		/// スポナーが持つすべてのエネミーを消す
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