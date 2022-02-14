using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.VFX;
using Ohira.Auxiliary;

namespace Ohira
{
	public partial class Satellite
	{
		public class SatelliteAttack1 : SatelliteAttackBase
		{
			const float MOBILLITY_INERTIA = 0.01f;
			float beamCharge = 0f;

			#region OverRideMethod

			public override void OnEnter(Satellite owner, SatelliteStateBase prevState = null)
			{
				owner.weaponCollision.SetMotionId(0);


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
					forward = owner.cameraController.GetFreeHomingVector(
						attackParam.homing.permissionDistance, attackParam.homing.permissionAngle);
				}
				forward.y = 0;
				forward = forward.normalized * 3f;

				Vector3 playerPos = owner.center.transform.position;
				Vector3 occurencePos = Quaternion.Euler(0f, 45f, 0f) * forward + playerPos;
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

					// コライダーオン
					owner.weaponCollision.Active(attackParam.consecutive);
					// エフェクト
					ActiveEffect(owner.currentWeapon, attackParam.sustain);
					// 残像
					owner.afterImageController.isCreate = attackParam.afterImage.isCreate;
					owner.afterImageController.createIntervalTime = attackParam.afterImage.createIntervalTime;
					owner.afterImageController.afterImageLifeTime = attackParam.afterImage.afterImageLifeTime;
					// 発生音
					//owner.audioSource.clip = attackParam.occurenceSound;
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
				Vector3 forward;
				if (owner.cameraController.IsLocking())
				{
					// ロックオン時
					forward = owner.cameraController.GetPlayerToTarget();
				}
				else
				{
					// ロックオンしてないとき
					forward = owner.cameraController.GetFreeHomingVector(
						attackParam.homing.permissionDistance, attackParam.homing.permissionAngle);
				}
				forward.y = 0;
				forward = forward.normalized * 2f;

				Vector3 centerPos = owner.center.transform.position;
				Vector3 sustainPos = Quaternion.Euler(0f, -60f, 0f) * forward + centerPos;
				if (attackParam.timeComplement && TimeManager.Instance.WasStopped())
				{
					// キングクリムゾン
					progressTimeLap = progressUnScaleTimeLap;
				}
				// 持続到達ポイントまで移動(ベジエ)
				float t = progressTimeLap / attackParam.sustain;
				Vector3 rerayPos = forward * 1.5f + centerPos;
				Vector3 firstVec = Vector3.Lerp(startPosition, rerayPos, t);
				Vector3 secondVec = Vector3.Lerp(rerayPos, sustainPos, t);
				Vector3 pos = Vector3.Lerp(firstVec, secondVec, t);
				owner.rb.MovePosition(pos);
				owner.transform.forward = (pos - centerPos).normalized;

				owner.weaponCollision.ComplementCollider(owner.center.transform.position);

				if (t >= 1f)
				{
					fixUpdate = PowerRigidity;
					progressTimeLap = 0f;
					progressUnScaleTimeLap = 0f;
					// コライダーオフ
					owner.weaponCollision.InActive();
					// エフェクト
					InActiveEffect(owner.currentWeapon);
					// 残像
					owner.afterImageController.isCreate = attackParam.afterImage.isCreate;
					owner.afterImageController.createIntervalTime = attackParam.afterImage.createIntervalTime;
					owner.afterImageController.afterImageLifeTime = attackParam.afterImage.afterImageLifeTime;


					owner.rb.angularVelocity = Vector3.zero;
					owner.rb.velocity = (pos - prePos).normalized;
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
					if (derive)
					{
						owner.ChangeState(stateAttack2);
					}
					else
					{
						owner.ChangeState(stateReturn);
					}
				}
			}


			protected override void PowerExit(Satellite owner, SatelliteStateBase nextState)
			{
				owner.afterImageController.isCreate = false;
			}
			#endregion


			#region MOBILITY
#if false
			#region Generic
			protected override void MobillityEnter(Satellite owner)
			{
				// 操作する武器を配列に取っておく
				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					var weapon = owner.currentWeapon.transform.GetChild(i).gameObject;
					controlWeapons[i] = weapon.gameObject;
					startPositions[i] = weapon.transform.position;
					startRotations[i] = weapon.transform.rotation;
					//Debug.Log("大平weaponName" + controlWeapons[i].name);
					//weapon.GetComponent<MeshCollider>().enabled = false;

				}
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
					//forward = owner.center.transform.forward;
					forward = owner.cameraController.GetFreeHomingVector(
						attackParam.homing.permissionDistance, attackParam.homing.permissionAngle);
					//targetPos = forward * 3f + owner.center.transform.position;
					targetPos = forward + owner.center.transform.position;
				}

				float t = progressTime / attackParam.occurence;
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
					progressTime = 0f;
				}
			}


			/// <summary>
			/// 持続
			/// </summary>
			/// <param name="owner"></param>
			protected override void MobillitySustain(Satellite owner)
			{
				float t = progressTime / attackParam.sustain;

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
						//targetPos = owner.center.transform.position + owner.center.transform.forward * 3f;
						targetPos = owner.center.transform.position +
							owner.cameraController.GetFreeHomingVector(attackParam.homing.permissionDistance, attackParam.homing.permissionAngle);
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
						// リジッドボディの演算無効
						//var rb = controlWeapons[i].GetComponent<Rigidbody>();
						//rb.constraints = RigidbodyConstraints.FreezeAll;
					}

					fixUpdate = MobillityRigidity;
					progressTime = 0f;
				}
			}


			/// <summary>
			/// 硬直
			/// </summary>
			/// <param name="owner"></param>
			protected override void MobillityRigidity(Satellite owner)
			{
				if (derive)
				{

				}
				if (progressTime >= attackParam.rigidity)
				{
					owner.ChangeState(stateReturn);
				}
			}


			protected override void MobillityExit(Satellite owner)
			{
				owner.currentWeapon.transform.localPosition = Vector3.zero;
				owner.currentWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
				//Debug.Log("大平current" + owner.currentWeapon.name);
				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					Rigidbody rb = controlWeapons[i].GetComponent<Rigidbody>();
					rb.velocity = Vector3.zero;
					rb.angularVelocity = Vector3.zero;
					controlWeapons[i].transform.localPosition = Vector3.zero;
					controlWeapons[i].transform.localRotation = Quaternion.Euler(Vector3.zero);
					//Debug.Log("大平weaponPos" + controlWeapons[i].transform.localPosition);
					// リジッドボディの演算無効
					//var rb = controlWeapons[i].GetComponent<Rigidbody>();
					rb.constraints = RigidbodyConstraints.FreezeAll;
				}
				//owner.currentWeapon.transform.localPosition = Vector3.zero;
				//owner.currentWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);

			}
			#endregion
#else
			/// <summary>
			/// 開始
			/// </summary>
			/// <param name="owner"></param>
			protected override void MobillityEnter(Satellite owner)
			{
				foreach (var weaponCol in owner.weaponCollisions)
				{
					weaponCol.SetMotionId(0);
				}

				// 操作する武器を配列に取っておく
				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					var weapon = owner.currentWeapon.transform.GetChild(i).gameObject;
					controlWeapons[i] = weapon.gameObject;
					startPositions[i] = weapon.transform.position;
					startRotations[i] = weapon.transform.rotation;
				}
				injection = 0;
				attackParam = owner.weaponCollisions[0].GetParam();

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
					targetPos = owner.cameraController.lockOnTransform.position;
				}
				else
				{
					// ロックオンしてないとき
					forward = owner.cameraController.GetFreeHomingVector(
						attackParam.homing.permissionDistance, attackParam.homing.permissionAngle);
					targetPos = forward + owner.center.transform.position;
				}
				forward.y = 0f;
				forward.Normalize();

				float t = progressTimeLap / attackParam.occurence;
				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					float angle = 135f + 90f * i;

					Vector3 occurencePos;

					occurencePos = Quaternion.AngleAxis(angle, Vector3.up) * forward * 2f + owner.center.transform.position;

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
						owner.weaponCollisions[i].Active(attackParam.consecutive);
						// エフェクト
						// 残像
						owner.afterImages[i].isCreate = attackParam.afterImage.isCreate;
						owner.afterImages[i].createIntervalTime = attackParam.afterImage.createIntervalTime;
						owner.afterImages[i].afterImageLifeTime = attackParam.afterImage.afterImageLifeTime;
					}

					fixUpdate = MobillitySustain;
					progressTimeLap = 0f;
					progressUnScaleTimeLap = 0f;

					// 発生音
					owner.audioSource.PlayOneShot(attackParam.occurenceSound);
					secondAlready = false;
				}
			}


			bool secondAlready = false;
			/// <summary>
			/// 持続
			/// </summary>
			/// <param name="owner"></param>
			protected override void MobillitySustain(Satellite owner)
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
					forward = owner.cameraController.GetFreeHomingVector(
						attackParam.homing.permissionDistance, attackParam.homing.permissionAngle);
				}
				forward.y = 0f;
				forward.Normalize();

				Vector3 rerayPos = forward * 5f + owner.center.transform.position;
				Vector3 sustainPos;
				Vector3 prePos;

				float t = progressTimeLap / attackParam.sustain;
				if (t <= 0.5f)
				{
					float workT = t * 2;
					sustainPos = Quaternion.AngleAxis(225f, Vector3.up) * forward * 2f + owner.center.transform.position;
					Vector3 start = Quaternion.AngleAxis(135f, Vector3.up) * forward * 2f + owner.center.transform.position;
					Vector3 firstVec = Vector3.Lerp(start, rerayPos, workT);
					Vector3 secondVec = Vector3.Lerp(rerayPos, sustainPos, workT);
					Vector3 pos = Vector3.Lerp(firstVec, secondVec, workT);
					prePos = controlWeapons[0].transform.position;
					controlWeapons[0].transform.forward = (pos - prePos).normalized;
					controlWeapons[0].transform.position = pos;
				}
				else
				{
					if (!secondAlready)
					{   // 発生音
						owner.audioSource.PlayOneShot(attackParam.occurenceSound);
					}
					secondAlready = true;
					float workT = (t - 0.5f) * 2f;
					sustainPos = Quaternion.AngleAxis(135f, Vector3.up) * forward * 2f + owner.center.transform.position;
					Vector3 start = Quaternion.AngleAxis(225f, Vector3.up) * forward * 2f + owner.center.transform.position;
					Vector3 firstVec = Vector3.Lerp(start, rerayPos, workT);
					Vector3 secondVec = Vector3.Lerp(rerayPos, sustainPos, workT);
					Vector3 pos = Vector3.Lerp(firstVec, secondVec, workT);
					prePos = controlWeapons[1].transform.position;
					controlWeapons[1].transform.forward = (pos - prePos).normalized;
					controlWeapons[1].transform.position = pos;

					controlWeapons[0].transform.position += controlWeapons[0].transform.forward.normalized * MOBILLITY_INERTIA;
				}

				if (t >= 1f)
				{
					for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
					{
						startPositions[i] = controlWeapons[i].transform.position;
						startRotations[i] = controlWeapons[i].transform.rotation;
						owner.weaponCollisions[i].InActive();
						// リジッドボディの演算無効
						//var rb = controlWeapons[i].GetComponent<Rigidbody>();
						//rb.constraints = RigidbodyConstraints.FreezeAll;
						owner.afterImages[i].isCreate = false;
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
				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					controlWeapons[i].transform.position += controlWeapons[i].transform.forward.normalized * MOBILLITY_INERTIA;
				}

				if (progressTimeLap >= attackParam.rigidity)
				{
					if (derive)
					{
						owner.ChangeState(stateAttack2);
					}
					else
					{
						owner.ChangeState(stateReturn);
					}
				}
			}


			/// <summary>
			/// 終了
			/// </summary>
			/// <param name="owner"></param>
			protected override void MobillityExit(Satellite owner, SatelliteStateBase nextState)
			{
				if (nextState == stateAttack2)
				{

				}
				else
				{
					owner.transform.position = controlWeapons[0].transform.position;
					owner.currentWeapon.transform.position = controlWeapons[0].transform.position;

					for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
					{
						controlWeapons[i].transform.localPosition = Vector3.zero;
						controlWeapons[i].transform.localRotation = Quaternion.Euler(Vector3.zero);
						owner.weaponCollisions[i].InActive();
						owner.afterImages[i].isCreate = false;
					}
				}
			}
#endif

			#endregion


			#region REACH
#if false
			#region Generic
			protected override void ReachEnter(Satellite owner)
			{
				owner.rb.maxAngularVelocity = float.MaxValue;
				owner.currentWeapon.GetComponent<Rigidbody>().maxAngularVelocity = float.MaxValue;
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
				float t = progressTime / attackParam.occurence;
				Vector3 pos = Vector3.Lerp(startPosition, occurencePos, t);
				owner.rb.MovePosition(pos);
				owner.currentWeapon.GetComponent<Rigidbody>().MovePosition(pos);
				Vector3 playerToWeapon = (owner.transform.position - player.position).normalized;
				Vector3 rot = Vector3.Lerp(startRotation.eulerAngles, /*Quaternion.Euler(0f, 45f, 0f) * forward*/playerToWeapon, t);
				owner.transform.LookAt(rot);

				if (t >= 1f)
				{
					fixUpdate = ReachSustain;
					progressTime = 0f;

					startPosition = owner.transform.position;
					startRotation = owner.transform.rotation;

					// コライダーオン
					owner.currentWeapon.GetComponentInChildren<MeshCollider>().enabled = true;
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
				float t = progressTime / attackParam.sustain;
				Vector3 pos = Vector3.Lerp(startPosition, sustainPos, t);
				owner.rb.MovePosition(pos);
				owner.currentWeapon.GetComponent<Rigidbody>().MovePosition(pos);

				//owner.transform.forward = (owner.transform.position - player.transform.position).normalized;
				//owner.rb.AddTorque(0, 5f, 0f, ForceMode.VelocityChange);
				owner.rb.angularVelocity = new Vector3(0f, 50f, 0f);
				owner.currentWeapon.GetComponent<Rigidbody>().angularVelocity = new Vector3(0f, 50f, 0f);

				if (t >= 1f)
				{
					fixUpdate = ReachRigidity;
					progressTime = 0f;
					// コライダーオフ
					owner.currentWeapon.GetComponentInChildren<MeshCollider>().enabled = false;
					//owner.rb.velocity = Vector3.zero;
					//owner.rb.angularVelocity = Vector3.zero;
				}
			}


			/// <summary>
			/// 硬直
			/// </summary>
			/// <param name="owner"></param>
			protected override void ReachRigidity(Satellite owner)
			{
				if (derive)
				{

				}
				if (progressTime >= attackParam.rigidity)
				{
					owner.ChangeState(stateReturn);
				}
			}



			protected override void ReachExit(Satellite owner)
			{
				owner.rb.maxAngularVelocity = 7f;
				owner.currentWeapon.GetComponent<Rigidbody>().maxAngularVelocity = 7f;
			}
			#endregion
#endif
			private float beamCool = 0f;
			private int reachLevel = 0;
			private float beamTickness = 0f;

			protected override void ReachEnter(Satellite owner)
			{
				beamCool = 0f;
				beamCharge = 0f;
				reachLevel = 0;
				muzzle = owner.currentWeapon.GetComponent<Muzzle>().muzzle;
				startPosition = owner.transform.position;
				owner.attackInfo.beam.transform.localEulerAngles = Vector3.zero;
				owner.attackInfo.beam.transform.localPosition = Vector3.zero;

				attackParam = owner.weaponCollision.GetParam();

				//		owner.vfxEffects[(int)E_SATELLITE_VFX.LASER_CHARGE].GetComponent<VisualEffect>().Play();
				owner.vfxCharges[(int)E_SATELLITE_BEAMCHARGE_VFX.CHARGE0].GetComponent<VisualEffect>().Play();
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
				}
				else
				{
					// ロックオンしてないとき
					//	forward = owner.cameraController.GetFreeHomingVector(
					//		attackParam.homing.permissionDistance, attackParam.homing.permissionAngle);
					forward = owner.cameraController.GetFreeLocOnVector();
				}
				Vector3 playerPos = owner.center.transform.position;
				Vector3 targetPos = playerPos + forward;
				forward.y = 0f;
				forward.Normalize();

				Vector3 occurencePos = forward + playerPos;
				float t = progressTimeLap / attackParam.occurence;
				Vector3 pos = Vector3.Lerp(startPosition, occurencePos, t);
				owner.rb.MovePosition(pos);
				Vector3 playerToWeapon = (owner.transform.position - playerPos).normalized;
				//	Vector3 rot = Vector3.Lerp(owner.transform.forward, playerToWeapon, t);
				Vector3 rot = Vector3.Lerp(owner.transform.forward, targetPos - owner.transform.position, t);
				// 高さの合わせ
				//rot.y = 0f;
				owner.transform.forward = rot;

				// チャージ
				beamCharge += Time.fixedDeltaTime;
				if (reachLevel < 2 && owner.weaponCollision.param.times[reachLevel].occurence < beamCharge)
				{
					owner.vfxCharges[reachLevel].GetComponent<VisualEffect>().Stop();
					reachLevel++;
					owner.vfxCharges[reachLevel].GetComponent<VisualEffect>().Play();
					beamCharge = 0f;
				}


				if (owner.input.Player.BeamCharge.ReadValue<float>().IsLessThan(0.2f))
				{
					if (t >= 1f)
					{
						fixUpdate = ReachSustain;
						progressTimeLap = 0f;
						progressUnScaleTimeLap = 0f;
						owner.attackInfo.beam.SetActive(true);
						owner.attackInfo.beam.transform.forward = owner.transform.forward;
						owner.attackInfo.beam.GetComponent<LineRenderer>().startWidth = 0.6f;

						owner.weaponCollision.SetMotionId(reachLevel);
						attackParam = owner.weaponCollision.GetParam();

						// ビジュアルエフェクト
						VisualEffect releaseVfx = owner.vfxEffects[(int)E_SATELLITE_VFX.LASER_RELEASE].GetComponent<VisualEffect>();
						releaseVfx.transform.localScale = Vector3.one * (reachLevel + 1);
						releaseVfx.Play();
						Mizuno.SoundManager.Instance.PlayMenuSe("SE_Beam");

						// 発生音
						owner.audioSource.PlayOneShot(attackParam.occurenceSound);
					}
					else
					{
						owner.ChangeState(stateReturn);
					}
					//owner.vfxEffects[(int)E_SATELLITE_VFX.LASER_CHARGE].GetComponent<VisualEffect>().Stop();
					VisualEffect vfx = owner.vfxCharges[reachLevel].GetComponent<VisualEffect>();
					vfx.Stop();

					//owner.attackInfo.beam.GetComponent<LineRenderer>().colorGradient = vfx.GetGradient("MainColor");
				}
			}


			/// <summary>
			/// 持続
			/// </summary>
			/// <param name="owner"></param>
			protected override void ReachSustain(Satellite owner)
			{
				float t = progressTimeLap / attackParam.sustain;

				// 太さ
				beamTickness = 0.6f * (reachLevel + 1);

				LineRenderer lineRenderer = owner.attackInfo.beam.GetComponent<LineRenderer>();
				var curve = new AnimationCurve();
				for (int i = 0; i < lineRenderer.widthCurve.length; i++)
				{
					curve.AddKey(i, beamTickness);
				}
				lineRenderer.widthCurve = curve;



				Vector3 scale = new Vector3(beamTickness, beamTickness, owner.attackInfo.beam.transform.localScale.z + 5f);

				owner.attackInfo.beam.transform.localScale = scale;

				BeamFunction(owner, owner.attackInfo.beam.transform.localScale.z, beamTickness);

				if (t >= 1f)
				{
					fixUpdate = ReachRigidity;
					progressTimeLap = 0f;
					progressUnScaleTimeLap = 0f;

					// ビジュアルエフェクト
					owner.vfxEffects[(int)E_SATELLITE_VFX.LASER_RELEASE].GetComponent<VisualEffect>().Stop();
				}
			}


			/// <summary>
			/// 硬直
			/// </summary>
			/// <param name="owner"></param>
			protected override void ReachRigidity(Satellite owner)
			{
				float t = progressTimeLap / attackParam.rigidity;

				float thickness = Mathf.Lerp(beamTickness, 0.0f, t);
				owner.attackInfo.beam.GetComponent<LineRenderer>().startWidth = thickness;

				if (t >= 1f)
				{
					owner.ChangeState(stateReturn);

					progressTimeLap = 0f;
					progressUnScaleTimeLap = 0f;
					owner.attackInfo.beam.SetActive(false);
				}
			}


			protected override void ReachExit(Satellite owner, SatelliteStateBase nextState)
			{
				owner.attackInfo.beam.SetActive(false);
				owner.attackInfo.beam.transform.localScale = Vector3.zero;
			}


			private void BeamFunction(Satellite owner, float length, float thickness)
			{
				if (beamCool < owner.attackInfo.beamDamageRate)
				{
					beamCool += Time.fixedDeltaTime;
					return;
				}
				beamCool = 0f;

				thickness *= 0.5f;
				Vector3 halfExtens = new Vector3(thickness, 1f, thickness);
				Transform transform = owner.currentWeapon.transform;

				RaycastHit[] hits = Physics.BoxCastAll(transform.position, halfExtens, transform.forward, transform.rotation, length, owner.attackInfo.enemyLayer);


				foreach (RaycastHit hit in hits)
				{
					GameObject target = hit.transform.gameObject;
					owner.weaponCollision.CauseDamage(target, hit.point);
				}
			}




			#endregion REACH

			#endregion
		}
	}
}