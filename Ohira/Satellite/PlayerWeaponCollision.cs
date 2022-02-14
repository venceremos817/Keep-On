using System.Collections.Generic;
using UnityEngine;


namespace Ohira
{
	[RequireComponent(typeof(AudioSource))]
	public class PlayerWeaponCollision : MonoBehaviour
	{
		[SerializeField, Tooltip("武器自体の攻撃力")] private float elementalyPower = 10;
		[SerializeField] public AttackParam param;
		[SerializeField, Tooltip("攻撃判定用コライダー")] private Collider[] colliders;
		[SerializeField, Tooltip("内側判定補完用コライダー")] private CapsuleCollider complementCollider;
		[SerializeField] private GameObject HitFX;
		[SerializeField] private AudioSource hitAudioSource;
		static public ComboParam comboParam = null;

		private int motionId;
		private float comboMagnificate;
		private List<GameObject> hitedList = new List<GameObject>();    // 当たったリスト
		private bool continuous = false;



		/// <summary>
		/// モーションIDの設定
		/// </summary>
		/// <param name="id"></param>
		public void SetMotionId(int id)
		{
			motionId = id;
		}


		/// <summary>
		/// 攻撃力を得る
		/// </summary>
		/// <returns></returns>
		public float GetPower()
		{
			return elementalyPower * param.times[motionId].motionValue * comboParam.GetComboMagnification();
		}


		public AttackParam.AttackTime GetParam()
		{
			return param.times[motionId];
		}


		/// <summary>
		/// ダメージを与える
		/// </summary>
		/// <param name="other"></param>
		public void CauseDamage(Collider other)
		{
			bool isHit = false;

			if (other.TryGetComponent(out Homare.Enemy enemyCS))
			{
				var hitBody = other.gameObject.transform.root.GetChild(0).gameObject;
				Debug.Log("大平" + hitBody.name);
				// すでに当たっていたら終了
				if (hitedList.Contains(hitBody))
					return;

				enemyCS.OnHitPlayerAttack(GetPower());
				param.times[motionId].cameraQuake.CameraShake();
				isHit = true;

				if (!continuous)
				{
					// あたったリストに追加
					hitedList.Add(hitBody);
				}
			}
			else if (other.TryGetComponent(out BossHitCollider boss))
			{
				var hitBody = other.gameObject.transform.root.GetChild(0).gameObject;
				Debug.Log("大平" + hitBody.name);
				// すでに当たっていたら終了
				if (hitedList.Contains(hitBody))
					return;

				boss.OnHitPlayerAttack(GetPower(), other.ClosestPointOnBounds(this.transform.position));
				param.times[motionId].cameraQuake.CameraShake();
				isHit = true;

				if (!continuous)
				{
					// あたったリストに追加
					hitedList.Add(hitBody);
				}
			}
			//else if (other.TryGetComponent(out Homare.Boss boss))
			//{
			//	boss.OnHitPlayerAttack(GetPower());
			//	param.times[motionId].cameraQuake.CameraShake();
			//}
			else
			{
				if (other.TryGetComponent(out SandBack sandBackCS))
				{
					var hitBody = other.gameObject.transform.root.GetChild(0).gameObject;
					Debug.Log("大平" + hitBody.name);
					// すでに当たっていたら終了
					if (hitedList.Contains(hitBody))
						return;

					sandBackCS.OnHitPlayerAttack(GetPower());
					param.times[motionId].cameraQuake.CameraShake();
					isHit = true;

					if (!continuous)
					{
						// あたったリストに追加
						hitedList.Add(hitBody);
					}
				}
				else
				{
					return;
				}
			}


			// エフェクト発生
			CreateHitFX(other.ClosestPointOnBounds(this.transform.position));

			// 音
			if (isHit)
			{
				PlayHitSE(param.times[motionId].hitSound);
			}

			TimeManager.Instance.ChangeTimeScale(param.times[motionId].hitStopTime);
		}

		/// <summary>
		/// ダメージを与える
		/// </summary>
		/// <param name="target"></param>
		/// <param name="hitPos"></param>
		public void CauseDamage(GameObject target, Vector3 hitPos)
		{
			bool isHit = false;

			if (target.TryGetComponent(out Homare.Enemy enemyCS))
			{
				enemyCS.OnHitPlayerAttack(GetPower());
				param.times[motionId].cameraQuake.CameraShake();
				isHit = true;
			}
			else if (target.TryGetComponent(out Homare.Boss boss))
			{
				boss.OnHitPlayerAttack(GetPower(), hitPos);
				param.times[motionId].cameraQuake.CameraShake();
				isHit = true;
			}
			else
			{
				if (target.TryGetComponent(out SandBack sandBackCS))
				{
					sandBackCS.OnHitPlayerAttack(GetPower());
					param.times[motionId].cameraQuake.CameraShake();
					isHit = true;
				}
				else
				{
					return;
				}
			}

			// エフェクト発生
			CreateHitFX(hitPos);

			// 音
			if (isHit)
			{
				PlayHitSE(param.times[motionId].hitSound);
			}

			TimeManager.Instance.ChangeTimeScale(param.times[motionId].hitStopTime);
		}


		/// <summary>
		/// ヒットエフェクト発生
		/// </summary>
		/// <param name="pos"></param>
		public void CreateHitFX(Vector3 pos)
		{
			GameObject fx = Instantiate(HitFX, pos, Quaternion.identity);
			Destroy(fx, 3f);
		}


		/// <summary>
		/// 攻撃判定の発生
		/// </summary>
		public void Active(bool continuous = false)
		{
			foreach (var col in colliders)
			{
				col.enabled = true;
			}

			this.continuous = continuous;
		}


		/// <summary>
		/// 攻撃判定の終了
		/// </summary>
		public void InActive()
		{
			foreach (var col in colliders)
			{
				col.enabled = false;
			}

			// 当たったリストクリア
			hitedList.Clear();
		}


		public void ClearHitList()
		{
			hitedList.Clear();
		}


		/// <summary>
		/// 内側攻撃判定出すやつ
		/// </summary>
		/// <param name="start"></param>
		public void ComplementCollider(Vector3 start)
		{
			Vector3 center = (start + transform.position) * 0.5f;

			float height = (start - transform.position).magnitude;

			complementCollider.center = transform.InverseTransformPoint(center);
			complementCollider.height = height;

		}



		private void PlayHitSE(AudioClip audioClip)
		{
			//bool played = false;
			//foreach (var audioSource in hitAudioSources)
			//{
			//	if (audioSource.isPlaying)
			//		continue;
			//	audioSource.clip = audioClip;
			//	audioSource.Play();
			//	played = true;
			//}

			//if (!played)
			//{
			//	var audioSource = gameObject.AddComponent<AudioSource>();
			//	hitAudioSources.Add(audioSource);
			//	audioSource.volume = 0.3f;
			//	audioSource.clip = audioClip;
			//	audioSource.Play();
			//}

			hitAudioSource.PlayOneShot(audioClip);
		}



		private void OnTriggerEnter(Collider other)
		{
			CauseDamage(other);
		}
	}
}