using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ohira
{
	public partial class Satellite
	{
		public class SatelliteAttack3 : SatelliteAttackBase
		{
			#region PrivateVal
			private float mem_PowAngle;
			#endregion


			#region OverRideMethod

			public override void OnEnter(Satellite owner, SatelliteStateBase prevState = null)
			{
				owner.weaponCollision.SetMotionId(2);


				base.OnEnter(owner);
			}


			#region NORMAL
			/// <summary>
			/// 前処理
			/// </summary>
			/// <param name="owner"></param>
			protected override void NormalEnter(Satellite owner)
			{

			}


			/// <summary>
			/// 発生
			/// </summary>
			/// <param name="owner"></param>
			protected override void NormalOccurence(Satellite owner)
			{

			}


			/// <summary>
			/// 持続
			/// </summary>
			/// <param name="owner"></param>
			protected override void NormalSustain(Satellite owner)
			{

			}


			/// <summary>
			/// 硬直
			/// </summary>
			/// <param name="owner"></param>
			protected override void NormalRigidity(Satellite owner)
			{

			}


			/// <summary>
			/// 後処理
			/// </summary>
			/// <param name="owner"></param>
			protected override void NormalExit(Satellite owner)
			{

			}
			#endregion



			#region POWER
			protected override void PowerEnter(Satellite owner)
			{
				attackParam = owner.weaponCollision.GetParam();
			}


			/// <summary>
			/// 発生
			/// </summary>
			/// <param name="owner"></param>
			protected override void PowerOccurence(Satellite owner)
			{
				Vector3 forward;
				if (owner.cameraController.IsLocking())
				{
					// ロックオン時
					forward = owner.cameraController.GetPlayerToTarget();
				}
				else
				{
					// ロックオンしてないとき
					//forward = owner.center.transform.forward * 3f;
					forward = owner.cameraController.GetFreeHomingVector(attackParam.homing.permissionDistance, attackParam.homing.permissionAngle);
				}
				forward.y = 0f;
				forward = forward.normalized * 3f;

				Vector3 playerPos = owner.center.transform.position;
				Vector3 occurencePos = Quaternion.Euler(0f, 90f, 0f) * forward + playerPos;
				// 発生ポイントまで移動
				float t = progressTimeLap / attackParam.occurence;
				Vector3 pos = Vector3.Lerp(startPosition, occurencePos, t);
				owner.rb.MovePosition(pos);
				Vector3 playerToWeapon = (owner.transform.position - playerPos).normalized;

				Vector3 rot = Vector3.Lerp(owner.transform.forward, playerToWeapon, t);
				owner.transform.forward = rot;


				if (t >= 1f)
				{
					fixUpdate = PowerSustain;
					progressTimeLap = 0f;
					progressUnScaleTimeLap = 0f;

					startPosition = owner.transform.position;
					startRotation = owner.transform.rotation;

					Vector2 from = new Vector2(forward.x, forward.z);
					Vector2 to = new Vector2(playerToWeapon.x, playerToWeapon.z);
					mem_PowAngle = Vector2.Angle(from, to);
					float cross = from.x * to.y - from.y * to.x;
					mem_PowAngle = (cross != 0) ? mem_PowAngle * Mathf.Sign(cross) : mem_PowAngle;
					//Debug.Log("大平mem_PowAngle" + mem_PowAngle);

					// コライダーオン
					owner.weaponCollision.Active(attackParam.consecutive);
					// エフェクト
					ActiveEffect(owner.currentWeapon, attackParam.sustain);
					// 残像
					owner.afterImageController.isCreate = attackParam.afterImage.isCreate;
					owner.afterImageController.createIntervalTime = attackParam.afterImage.createIntervalTime;
					owner.afterImageController.afterImageLifeTime = attackParam.afterImage.afterImageLifeTime;
					// 音
					//owner.audioSource.clip = attackParam.occurenceSound;
					//owner.audioSource.Play();
					owner.audioSource.PlayOneShot(attackParam.occurenceSound);
				}
			}


			/// <summary>
			/// 持続
			/// </summary>
			/// <param name="owner"></param>
			protected override void PowerSustain(Satellite owner)
			{
				Vector3 prePos = owner.currentWeapon.transform.position;
				const float kaiten = 2;
				if (attackParam.timeComplement && TimeManager.Instance.WasStopped())
				{
					// キングクリムゾン
					progressTimeLap = progressUnScaleTimeLap;
				}
				float t = progressTimeLap / attackParam.sustain;

				Vector3 pos = Vector3.zero;
				float f = mem_PowAngle + t * 2 * Mathf.PI * kaiten;
				pos.x = Mathf.Cos(f);
				pos.z = Mathf.Sin(f);
				pos *= 3;
				pos += owner.center.transform.position;

				Vector3 playerToWeapon = (owner.currentWeapon.transform.position - owner.center.transform.position).normalized;
				owner.currentWeapon.transform.forward = playerToWeapon;

				owner.rb.MovePosition(pos);

				owner.weaponCollision.ComplementCollider(owner.center.transform.position);

				if (t >= 1f)
				{
					fixUpdate = PowerRigidity;
					progressTimeLap = 0f;
					progressUnScaleTimeLap = 0f;
					// コライダーオフ
					owner.weaponCollision.InActive();
					//owner.rb.velocity = Vector3.zero;
					owner.rb.angularVelocity = Vector3.zero;
					owner.rb.velocity = (pos - prePos).normalized;
					// エフェクト
					InActiveEffect(owner.currentWeapon);
					// 残像
					owner.afterImageController.isCreate = false;
				}
			}


			/// <summary>
			/// 硬直
			/// </summary>
			/// <param name="owner"></param>
			protected override void PowerRigidity(Satellite owner)
			{
				if (progressTimeLap >= attackParam.rigidity)
				{
					owner.ChangeState(stateReturn);
				}
			}


			protected override void PowerExit(Satellite owner, SatelliteStateBase nextState)
			{
				owner.afterImageController.isCreate = false;
			}
			#endregion


			#region MOBILITY
			protected override void MobillityEnter(Satellite owner)
			{
				foreach (var weaponCol in owner.weaponCollisions)
				{
					weaponCol.SetMotionId(2);
				}

				// 操作する武器を配列に取っておく
				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					var weapon = owner.currentWeapon.transform.GetChild(i).gameObject;
					controlWeapons[i] = weapon.gameObject;
					startPositions[i] = weapon.transform.position;
					startRotations[i] = weapon.transform.rotation;
				}
				attackParam = owner.weaponCollisions[0].GetParam();
				injection = 0;
			}


			/// <summary>
			/// 発生
			/// </summary>
			/// <param name="owner"></param>
			protected override void MobillityOccurence(Satellite owner)
			{
				Vector3 forward;
				Vector3 targetPos;
				if (owner.cameraController.IsLocking())
				{
					// ロックオン時
					forward = owner.cameraController.GetPlayerToTarget();
					//forward.y = 0f;
					forward.Normalize();
					targetPos = owner.cameraController.lockOnTransform.position;
				}
				else
				{
					// ロックオンしてないとき
					forward = owner.center.transform.forward;
					targetPos = forward * 3f + owner.center.transform.position;
				}

				float t = progressTimeLap / attackParam.occurence;
				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					float angle = 90f - 60f * i;
					Vector3 occurencePos;

					occurencePos = Quaternion.AngleAxis(angle, forward) * Vector3.up * 3f + owner.center.transform.position;

					Vector3 pos = Vector3.Lerp(startPositions[i], occurencePos, t);

					controlWeapons[i].transform.position = pos;
					controlWeapons[i].transform.LookAt(targetPos);
				}

				if (t >= 1f)
				{
					for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
					{
						startPositions[i] = controlWeapons[i].transform.position;
						startRotations[i] = controlWeapons[i].transform.rotation;
						controlWeapons[i].GetComponentInChildren<MeshCollider>().enabled = true;
						var rb = controlWeapons[i].GetComponent<Rigidbody>();
						// リジッドボディの演算有効
						rb.constraints = RigidbodyConstraints.None;
					}

					fixUpdate = MobillitySustain;
					progressTimeLap = 0f;
					progressUnScaleTimeLap = 0f;
				}
			}


			/// <summary>
			/// 持続
			/// </summary>
			/// <param name="owner"></param>
			protected override void MobillitySustain(Satellite owner)
			{
				float t = progressTimeLap / attackParam.sustain;

				if (injection < MOBILLITY_WEAPON_NUM &&
					(float)injection / (MOBILLITY_WEAPON_NUM + 1) <= t)
				{
					Vector3 force;
					if (owner.cameraController.IsLocking())
					{
						// ロックオン時
						Vector3 targetPos;

						targetPos = owner.cameraController.lockOnTransform.position;
						force = (targetPos - controlWeapons[injection].transform.position).normalized;
					}
					else
					{
						// ロックオンしてないとき
						Vector3 targetPos;

						targetPos = owner.center.transform.position + owner.center.transform.forward * 3f;
						force = (targetPos - controlWeapons[injection].transform.position).normalized;
					}

					controlWeapons[injection].transform.forward = force.normalized;
					controlWeapons[injection].GetComponent<Rigidbody>().AddForce(force * 50f, ForceMode.VelocityChange);

					injection++;
				}


				if (t >= 1f)
				{
					for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
					{
						startPositions[i] = controlWeapons[i].transform.position;
						startRotations[i] = controlWeapons[i].transform.rotation;
						controlWeapons[i].GetComponentInChildren<MeshCollider>().enabled = false;
						var rb = controlWeapons[i].GetComponent<Rigidbody>();
						// リジッドボディの演算無効
						rb.constraints = RigidbodyConstraints.FreezeAll;
					}

					fixUpdate = MobillityRigidity;
					progressTimeLap = 0f;
					progressUnScaleTimeLap = 0f;
				}
			}


			/// <summary>
			/// 硬直
			/// </summary>
			/// <param name="owner"></param>
			protected override void MobillityRigidity(Satellite owner)
			{
				if (progressTimeLap >= attackParam.rigidity)
				{
					owner.ChangeState(stateReturn);
				}
			}


			protected override void MobillityExit(Satellite owner, SatelliteStateBase nextState)
			{
				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					Rigidbody rb = controlWeapons[i].GetComponent<Rigidbody>();
					rb.velocity = Vector3.zero;
					rb.angularVelocity = Vector3.zero;
					controlWeapons[i].transform.localPosition = Vector3.zero;
					controlWeapons[i].transform.localEulerAngles = Vector3.zero;
				}
			}
			#endregion


			#region REACH
			protected override void ReachEnter(Satellite owner)
			{
				owner.rb.maxAngularVelocity = float.MaxValue;
				owner.currentWeapon.GetComponent<Rigidbody>().maxAngularVelocity = float.MaxValue;
				attackParam = owner.weaponCollision.GetParam();
			}



			/// <summary>
			/// 発生
			/// </summary>
			/// <param name="owner"></param>
			protected override void ReachOccurence(Satellite owner)
			{
				Vector3 forward;
				if (owner.cameraController.IsLocking())
				{
					// ロックオン時
					forward = owner.cameraController.GetPlayerToTarget();
					//forward.y = 0f;
					forward = forward.normalized * 3f;
				}
				else
				{
					// ロックオンしてないとき
					forward = owner.center.transform.forward * 3f;
				}
				Transform player = owner.center.transform;
				Vector3 occurencePos = Quaternion.Euler(0f, 45f, 0f) * forward + player.position;
				// 発生ポイントまで移動
				float t = progressTimeLap / attackParam.occurence;
				Vector3 pos = Vector3.Lerp(startPosition, occurencePos, t);
				owner.rb.MovePosition(pos);
				owner.currentWeapon.GetComponent<Rigidbody>().MovePosition(pos);
				Vector3 playerToWeapon = (owner.transform.position - player.position).normalized;
				Vector3 rot = Vector3.Lerp(startRotation.eulerAngles, /*Quaternion.Euler(0f, 45f, 0f) * forward*/playerToWeapon, t);
				owner.transform.LookAt(rot);

				if (t >= 1f)
				{
					fixUpdate = ReachSustain;
					progressTimeLap = 0f;
					progressUnScaleTimeLap = 0f;

					startPosition = owner.transform.position;
					startRotation = owner.transform.rotation;

					// コライダーオン
					owner.weaponCollision.Active(attackParam.consecutive);
				}
			}


			/// <summary>
			/// 持続
			/// </summary>
			/// <param name="owner"></param>
			protected override void ReachSustain(Satellite owner)
			{
				Vector3 forward;
				if (owner.cameraController.IsLocking())
				{
					// ロックオン時
					forward = owner.cameraController.GetPlayerToTarget();
					//forward.y = 0f;
					forward = forward.normalized * 3f;
				}
				else
				{
					// ロックオンしてないとき
					forward = owner.center.transform.forward * 3f;
				}

				Transform player = owner.center.transform;
				Vector3 sustainPos = forward + player.position;
				// 持続到達ポイントまで移動
				float t = progressTimeLap / attackParam.sustain;
				Vector3 pos = Vector3.Lerp(startPosition, sustainPos, t);
				owner.rb.MovePosition(pos);
				owner.currentWeapon.GetComponent<Rigidbody>().MovePosition(pos);

				owner.rb.angularVelocity = new Vector3(0f, 50f, 0f);
				owner.currentWeapon.GetComponent<Rigidbody>().angularVelocity = new Vector3(0f, 50f, 0f);

				if (t >= 1f)
				{
					fixUpdate = ReachRigidity;
					progressTimeLap = 0f;
					progressUnScaleTimeLap = 0f;
					// コライダーオフ
					owner.weaponCollision.InActive();
				}
			}


			/// <summary>
			/// 硬直
			/// </summary>
			/// <param name="owner"></param>
			protected override void ReachRigidity(Satellite owner)
			{
				if (progressTimeLap >= attackParam.rigidity)
				{
					owner.ChangeState(stateReturn);
				}
			}



			protected override void ReachExit(Satellite owner, SatelliteStateBase nextState)
			{
				owner.rb.maxAngularVelocity = 7f;
				owner.currentWeapon.GetComponent<Rigidbody>().maxAngularVelocity = 7f;
			}
			#endregion

			#endregion
		}
	}
}