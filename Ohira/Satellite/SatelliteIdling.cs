using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ohira
{
	[System.Serializable]
	class SatelliteIdlingParameters
	{
		[Tooltip("軌道半径")]
		[SerializeField] public float orbitRadius = 5f;        // 軌道半径
		[Tooltip("半径が広がる速さ")]
		[SerializeField] public float lerpRadius = 30f;
		[Tooltip("公転速度")]
		[SerializeField] public float revolutionSpeed = 1f;    // 公転速度
		[Tooltip("自転速度")]
		[SerializeField] public float rotateSpeed = 90f;       // 自転速度
		[Tooltip("揺らぎ周期")]
		[SerializeField] public float fluctuationSpeed = 1.5f;
		[Tooltip("揺らぎ大きさ")]
		[SerializeField] public float fluctuationSize = 0.5f;
	}

	public partial class Satellite
	{
		public class SatelliteIdling : SatelliteStateBase
		{
			#region PrivateVariable
			private float radius;
			private Vector3 preOrbitPos;
			private Vector3 curOrbitPos;
			private Vector3[] preOrbitPoses = new Vector3[MOBILLITY_WEAPON_NUM];
			private Vector3[] curOrbitPoses = new Vector3[MOBILLITY_WEAPON_NUM];
			private Rigidbody[] multipleWeaponsRb = new Rigidbody[MOBILLITY_WEAPON_NUM];
			private bool multiple = false;
			#endregion

			delegate void Idling(Satellite owner);
			Idling IdlingUpdate;


			#region OverrideMethod
			public override void OnEnter(Satellite owner, SatelliteStateBase prevState = null)
			{
#if false
				if (owner.style.style == Style.E_Style.MOBILITY)
				{
					GameObject[] weapons = new GameObject[MOBILLITY_WEAPON_NUM];
					for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
					{
						weapons[i] = owner.currentWeapon.transform.GetChild(i).gameObject;
					}
					weapons[0].GetComponent<HingeJoint>().connectedBody = owner.rb;
					for (int i = 1; i < MOBILLITY_WEAPON_NUM; i++)
					{
						var joint = weapons[i].GetComponent<HingeJoint>();
						joint.connectedBody = weapons[i - 1].GetComponent<Rigidbody>();
					}
				}
#endif

				radius = 0;
				preOrbitPos = curOrbitPos = new Vector3
				(
				Mathf.Sin(Time.time * owner.idlingParameters.revolutionSpeed),
				0,
				Mathf.Cos(Time.time * owner.idlingParameters.revolutionSpeed)) * radius;
				owner.transform.position = owner.center.transform.position + curOrbitPos;

				owner.colliderr.enabled = false;
				owner.rb.velocity = Vector3.zero;
				owner.rb.angularVelocity = Vector3.zero;
				owner.currentWeapon.transform.localPosition = Vector3.zero;
				owner.currentWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);

				owner.ActiveIdlingTrail();

				if (owner.style.style == Style.E_Style.MOBILITY)
				{
					IdlingUpdate = MultipleIdling;

					for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
					{
						multipleWeaponsRb[i] = owner.currentWeapon.transform.GetChild(i).GetComponent<Rigidbody>();
					}
					multiple = true;
				}
				else
				{
					IdlingUpdate = SingleIdling;
					multiple = false;
				}
			}


			public override void OnFixedUpdate(Satellite owner)
			{
				//SingleIdling(owner);
				IdlingUpdate(owner);

				radius = Mathf.Lerp(radius, owner.idlingParameters.orbitRadius, owner.idlingParameters.lerpRadius);

			}


			public override void OnUpdate(Satellite owner)
			{
				if (owner.input.Player.Attack.triggered)
				{
					OnAttack(owner);
				}
				else if (owner.input.Player.Steal.triggered)
				{
					OnSteal(owner);
				}
			}


			public override void OnExit(Satellite owner, SatelliteStateBase nextState = null)
			{
				owner.rb.velocity = Vector3.zero;
				owner.rb.angularVelocity = Vector3.zero;

				owner.currentWeapon.transform.localPosition = Vector3.zero;
				owner.currentWeapon.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);


				owner.InActiveIdlingTrail();

				if (multiple)
				{
					for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
					{
						multipleWeaponsRb[i].transform.localPosition = Vector3.zero;
						multipleWeaponsRb[i].transform.localEulerAngles = Vector3.zero;
					}
				}
			}
			#endregion


			#region PrivateMethod
			/// <summary>
			/// アイドリング状態
			/// </summary>
			private void SingleIdling(Satellite owner)
			{
				RevolutionSingle(owner);
				RotationSingle(owner);
			}


			private void MultipleIdling(Satellite owner)
			{
				RotationMultiple(owner);
				RevolutionMultiple(owner);
			}


			/// <summary>
			/// プレイヤーを中心とした公転
			/// </summary>
			private void RevolutionSingle(Satellite owner)
			{
				preOrbitPos = curOrbitPos;
				Vector3 orbitPos = new Vector3
					(
					Mathf.Sin(Time.time * owner.idlingParameters.revolutionSpeed) * radius,
					Mathf.Sin(Time.time * owner.idlingParameters.fluctuationSpeed) * owner.idlingParameters.fluctuationSize,
					Mathf.Cos(Time.time * owner.idlingParameters.revolutionSpeed) * radius);
				curOrbitPos = orbitPos;

				owner.rb.MovePosition(orbitPos + owner.center.transform.position);
			}


			/// <summary>
			/// これ自体の自転
			/// 円の接線を軸に自転する
			/// </summary>
			private void RotationSingle(Satellite owner)
			{
				Vector3 tangent = (curOrbitPos - preOrbitPos).normalized;    // 接線

				tangent = Vector3.Lerp(owner.transform.forward, tangent, 0.3f);

				owner.transform.forward = tangent;          // 接線方向を向かせる
				owner.rb.angularVelocity = owner.transform.forward * owner.idlingParameters.rotateSpeed;    // 旋回
			}


			private void RevolutionMultiple(Satellite owner)
			{
				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					preOrbitPoses[i] = curOrbitPoses[i];
					float diffPeriod = (2.0f * Mathf.PI) * ((float)i / MOBILLITY_WEAPON_NUM);   // 周期のズレ

					Vector3 orbitPos = new Vector3
						(
						Mathf.Sin(Time.time * owner.idlingParameters.revolutionSpeed + diffPeriod) * radius,
						Mathf.Sin(Time.time * owner.idlingParameters.fluctuationSpeed) * owner.idlingParameters.fluctuationSize,
						Mathf.Cos(Time.time * owner.idlingParameters.revolutionSpeed + diffPeriod) * radius);
					curOrbitPoses[i] = orbitPos;

					//mobillityWeaponsRb[i].MovePosition(orbitPos + owner.center.transform.position);
					multipleWeaponsRb[i].transform.position = orbitPos + owner.center.transform.position;
				}

				owner.transform.position = owner.center.transform.position;
			}


			private void RotationMultiple(Satellite owner)
			{
				for (int i = 0; i < MOBILLITY_WEAPON_NUM; i++)
				{
					Vector3 tangent = (curOrbitPoses[i] - preOrbitPoses[i]).normalized;     // 接線

					tangent = Vector3.Lerp(multipleWeaponsRb[i].gameObject.transform.forward, tangent, 0.3f);

					multipleWeaponsRb[i].gameObject.transform.forward = tangent;
					//mobillityWeaponsRb[i].angularVelocity = tangent * owner.idlingParameters.rotateSpeed;       // 旋回
				}
			}
			#endregion



			public override void OnSteal(Satellite owner)
			{
				owner.ChangeState(stateHoming);
			}


			public override void OnAttack(Satellite owner)
			{
				owner.ChangeState(stateAttack1);
			}
		}
	}
}