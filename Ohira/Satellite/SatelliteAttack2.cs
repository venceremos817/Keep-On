using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ohira.Auxiliary;

namespace Ohira
{
	public partial class Satellite
	{
		public class SatelliteAttack2 : SatelliteAttackBase
		{
			const float MOBILLITY_INERTIA = 0.01f;


			Vector3 lastPlayerPos;

			Vector3[] lastWeaponMoveDirections = new Vector3[MOBILLITY_WEAPON_NUM];

			#region OverRideMethod

			public override void OnEnter(Satellite owner, SatelliteStateBase prevState = null)
			{
				owner.weaponCollision.SetMotionId(1);


				base.OnEnter(owner);
			}


			#region Normal
			/// <summary>
			/// �O����
			/// </summary>
			/// <param name="owner"></param>
			protected override void NormalEnter(Satellite owner)
			{

			}


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected override void NormalOccurence(Satellite owner)
			{

			}


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected override void NormalSustain(Satellite owner)
			{

			}


			/// <summary>
			/// �d��
			/// </summary>
			/// <param name="owner"></param>
			protected override void NormalRigidity(Satellite owner)
			{

			}


			/// <summary>
			/// �㏈��
			/// </summary>
			/// <param name="owner"></param>
			protected override void NormalExit(Satellite owner)
			{

			}
			#endregion



			#region Power
			/// <summary>
			/// �O����
			/// </summary>
			/// <param name="owner"></param>
			protected override void PowerEnter(Satellite owner)
			{
				attackParam = owner.weaponCollision.GetParam();
			}


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected override void PowerOccurence(Satellite owner)
			{
				Vector3 forward;
				if (owner.cameraController.IsLocking())
				{
					// ���b�N�I����
					forward = owner.cameraController.GetPlayerToTarget();
				}
				else
				{
					// ���b�N�I�����ĂȂ��Ƃ�
					forward = owner.cameraController.GetFreeHomingVector(
						attackParam.homing.permissionDistance, attackParam.homing.permissionAngle);
				}
				forward.y = 0f;
				forward = forward.normalized * 3f;

				Vector3 playerPos = owner.center.transform.position;
				Vector3 occurencePos = Quaternion.Euler(0f, -45f, 0f) * forward + playerPos;
				// �����|�C���g�܂ňړ�
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

					// �G�t�F�N�g
					ActiveEffect(owner.currentWeapon, attackParam.sustain);
					// �c��
					owner.afterImageController.isCreate = attackParam.afterImage.isCreate;
					owner.afterImageController.createIntervalTime = attackParam.afterImage.createIntervalTime;
					owner.afterImageController.afterImageLifeTime = attackParam.afterImage.afterImageLifeTime;

					// ��
					//owner.audioSource.clip = attackParam.occurenceSound;
					//owner.audioSource.Play();
					owner.audioSource.PlayOneShot(attackParam.occurenceSound);


					// �R���C�_�[�I��
					owner.weaponCollision.Active(attackParam.consecutive);
				}
			}


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected override void PowerSustain(Satellite owner)
			{
				Vector3 prePos = owner.currentWeapon.transform.position;
				Vector3 forward;
				if (owner.cameraController.IsLocking())
				{
					// ���b�N�I����
					forward = owner.cameraController.GetPlayerToTarget();
				}
				else
				{
					// ���b�N�I�����ĂȂ��Ƃ�
					forward = owner.cameraController.GetFreeHomingVector(
						attackParam.homing.permissionDistance, attackParam.homing.permissionAngle);
				}
				forward.y = 0f;
				forward = forward.normalized * 2f;

				Vector3 centerPos = owner.center.transform.position;
				Vector3 sustainPos = Quaternion.Euler(0f, 45f, 0f) * forward + centerPos;
				if (attackParam.timeComplement && TimeManager.Instance.WasStopped())
				{
					// �L���O�N�����]��
					progressTimeLap = progressUnScaleTimeLap;
				}
				// �������B�|�C���g�܂ňړ�(�x�W�G)
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
					// �R���C�_�[�I�t
					owner.weaponCollision.InActive();
					owner.rb.angularVelocity = Vector3.zero;
					owner.rb.velocity = (pos - prePos).normalized;
					InActiveEffect(owner.currentWeapon);
					owner.afterImageController.isCreate = false;
				}
			}


			/// <summary>
			/// �d��
			/// </summary>
			/// <param name="owner"></param>
			protected override void PowerRigidity(Satellite owner)
			{
				if (progressTimeLap >= attackParam.rigidity)
				{
					if (derive)
					{
						owner.ChangeState(stateAttack3);
					}
					else
					{
						owner.ChangeState(stateReturn);
					}
				}
			}


			/// <summary>
			/// �㏈��
			/// </summary>
			/// <param name="owner"></param>
			protected override void PowerExit(Satellite owner, SatelliteStateBase nextState)
			{
				owner.afterImageController.isCreate = false;
			}
			#endregion



			#region Mobillity
			/// <summary>
			/// �O����
			/// </summary>
			/// <param name="owner"></param>
			protected override void MobillityEnter(Satellite owner)
			{
				foreach (PlayerWeaponCollision weaponCol in owner.weaponCollisions)
				{
					weaponCol.SetMotionId(1);
				}
				// ���삷�镐���z��Ɏ���Ă���
				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					var weapon = owner.currentWeapon.transform.GetChild(i).gameObject;
					controlWeapons[i] = weapon.gameObject;
					startPositions[i] = weapon.transform.position;
					startRotations[i] = weapon.transform.rotation;
				}
				injection = 0;
				attackParam = owner.weaponCollisions[0].GetParam();
				//	Debug.Break();
			}


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected override void MobillityOccurence(Satellite owner)
			{
				Vector3 forward;
				if (CameraController.Instance.IsLocking())
				{
					// ���b�N�I����
					forward = CameraController.Instance.GetPlayerToTarget();
				}
				else
				{
					// ���b�N�I�����ĂȂ��Ƃ�
					forward = CameraController.Instance.GetFreeHomingVector(
						attackParam.homing.permissionDistance, attackParam.homing.permissionAngle);
				}
				forward.y = 0f;
				forward.Normalize();

				float t = progressTimeLap / attackParam.occurence;

				Vector3[] pos = new Vector3[2];
				pos[0] = Vector3.Lerp(startPositions[0], owner.center.transform.position + Quaternion.AngleAxis(225f, Vector3.up) * forward, t);
				pos[1] = Vector3.Lerp(startPositions[1], owner.center.transform.position + Quaternion.AngleAxis(135f, Vector3.up) * forward, t);

				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					controlWeapons[i].transform.forward = (pos[i] - controlWeapons[i].transform.position).normalized;
					controlWeapons[i].transform.position = pos[i];
				}

				if (t >= 1f)
				{
					for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
					{
						startPositions[i] = controlWeapons[i].transform.position;
						startRotations[i] = controlWeapons[i].transform.rotation;
						// �q�b�g����
						owner.weaponCollisions[i].Active(attackParam.consecutive);
						// �G�t�F�N�g
						// �c��
						owner.afterImages[i].isCreate = attackParam.afterImage.isCreate;
						owner.afterImages[i].createIntervalTime = attackParam.afterImage.createIntervalTime;
						owner.afterImages[i].afterImageLifeTime = attackParam.afterImage.afterImageLifeTime;
					}

					fixUpdate = MobillitySustain;
					progressTimeLap = 0f;
					progressUnScaleTimeLap = 0f;
					lastPlayerPos = owner.center.transform.position;
				}
			}


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected override void MobillitySustain(Satellite owner)
			{
				const float hit = 4 * Mathf.PI;

				Vector3 forward;
				if (CameraController.Instance.IsLocking())
				{
					// ���b�N�I����
					forward = CameraController.Instance.GetPlayerToTarget();
				}
				else
				{
					// ���b�N�I�����ĂȂ��Ƃ�
					forward = CameraController.Instance.GetFreeHomingVector(
						attackParam.homing.permissionDistance, attackParam.homing.permissionAngle);
				}
				forward.y = 0f;
				forward = forward.normalized * 4f;

				Vector3 OccurencePos = owner.center.transform.position + forward;
				//Vector3 OccurencePos = lastPlayerPos + forward;

				float t = progressTimeLap / attackParam.sustain;

				Vector3[] pos = new Vector3[MOBILLITY_WEAPON_NUM];
				Vector3 diff = -lastPlayerPos + owner.center.transform.position;
				float period = hit * t;
				float prePeriod = hit * ((progressTime - Time.fixedDeltaTime) / attackParam.sustain);
				float cos = Mathf.Cos(period);
				float sin = Mathf.Sin(period);
				if (cos.IsMoreThan(0f) && Mathf.Cos(prePeriod).IsLessThan(0f))
				{
					// ������
					owner.audioSource.PlayOneShot(attackParam.occurenceSound);
				}
				if (sin.IsMoreThan(0f) && Mathf.Sin(prePeriod).IsLessThan(0f))
				{
					// ������
					owner.audioSource.PlayOneShot(attackParam.occurenceSound);
				}
				pos[0] = Vector3.Lerp(startPositions[0] + diff, OccurencePos, Mathf.Abs(cos));
				pos[1] = Vector3.Lerp(startPositions[1] + diff, OccurencePos, Mathf.Abs(sin));

				Vector3 prePos;
				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					controlWeapons[i].transform.forward = forward;
					prePos = controlWeapons[i].transform.position;
					controlWeapons[i].transform.position = pos[i];

					lastWeaponMoveDirections[i] = (prePos - pos[i]);
				}

				if (t >= 1f)
				{
					for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
					{
						startPositions[i] = controlWeapons[i].transform.position;
						//owner.weaponCollisions[i].Active();
						startRotations[i] = controlWeapons[i].transform.rotation;
						owner.weaponCollisions[i].InActive();
						// �G�t�F�N�g
						// �c��
						owner.afterImages[i].isCreate = false;
					}

					fixUpdate = MobillityRigidity;
					progressTimeLap = 0f;
					progressUnScaleTimeLap = 0f;
				}
			}


			/// <summary>
			/// �d��
			/// </summary>
			/// <param name="owner"></param>
			protected override void MobillityRigidity(Satellite owner)
			{
				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					controlWeapons[i].transform.position += lastWeaponMoveDirections[i].normalized * MOBILLITY_INERTIA;
				}

				if (progressTimeLap >= attackParam.rigidity)
				{
					owner.ChangeState(stateReturn);
				}
			}


			/// <summary>
			/// �㏈��
			/// </summary>
			/// <param name="owner"></param>
			protected override void MobillityExit(Satellite owner, SatelliteStateBase nextState)
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
			#endregion



			#region Reach
			/// <summary>
			/// �O����
			/// </summary>
			/// <param name="owner"></param>
			protected override void ReachEnter(Satellite owner)
			{
				attackParam = owner.weaponCollision.GetParam();
			}


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected override void ReachOccurence(Satellite owner)
			{

			}


			/// <summary>
			/// ����
			/// </summary>
			/// <param name="owner"></param>
			protected override void ReachSustain(Satellite owner)
			{

			}


			/// <summary>
			/// �d��
			/// </summary>
			/// <param name="owner"></param>
			protected override void ReachRigidity(Satellite owner)
			{

			}


			/// <summary>
			/// �㏈��
			/// </summary>
			/// <param name="owner"></param>
			protected override void ReachExit(Satellite owner, SatelliteStateBase nextState)
			{

			}
			#endregion


			#endregion
		}

	}

}