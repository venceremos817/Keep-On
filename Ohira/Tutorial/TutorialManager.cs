using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.VFX;

namespace Ohira
{
	public partial class TutorialManager : SingletonMono<TutorialManager>
	{
		#region ��
		/// <summary>
		/// �`���[�g���A���p�[�g�̗�
		/// </summary>
		public enum E_TUTORIAL_PART
		{
			MOBILLITY = 0,      //	�@���̃p�[�g
			REACH,              //	���[�`�̃p�[�g
			POWER,              //	�p���[�̃p�[�g
			HEAL,               //	�񕜂̃p�[�g
			EXTERMINATION,      //	�r�Ńp�[�g
		}
		#endregion

		static bool alreadyFinish = false;

		#region �p�u���b�N�ϐ�
		[SerializeField] public Transform playerTrans;
		[SerializeField] public GameObject damageUICanvus;

		#endregion

		#region �v���C�x�[�g�ϐ�
		[SerializeField] private TutorialEnemySpawner spawner;
		[SerializeField] private TutorialEnemySpawner spawner2;
		[SerializeField] private Maeda.Player playerCS = null;
		[SerializeField] private Satellite satelliteCS = null;
		[SerializeField] private Style playerStyle;
		[SerializeField] public TutorialPart tutorialPart;
		//[SerializeField] private GameObject successUI;
		[SerializeField] private VisualEffect successEffect;
		[SerializeField] private GameObject pauseCanvas;
		private float mem_PlayerHP = 0f;
		public E_TUTORIAL_PART e_TUTORIAL_PART;     // ���݂̃p�[�g��
		bool steping = false;
		//	private bool[] already;
		private PlayerControler input = null;
		#endregion


		private void Start()
		{
			tutorialPart.Init();

			// �@���p�[�g����J�n
			e_TUTORIAL_PART = E_TUTORIAL_PART.MOBILLITY;

			ClearAllEnemy();
			tutorialPart.parts[(int)e_TUTORIAL_PART].enterFunc?.Invoke();
			//StartMobillityPart();
			steping = false;

			var soundManager = Mizuno.SoundManager.Instance;
			soundManager.PlayBGMWithFade("BGM_Tutorial_Melo_ON");

			input = new PlayerControler();
			input.Enable();
			pauseCanvas.SetActive(false);

			//if (alreadyFinish)
			//{
			//	StartCoroutine(
			//		DelayMethod(0.5f, () => pauseCanvas.SetActive(true))
			//		);
			//}
		}


		public void Update()
		{
			if (!IsPlayerLive())
			{
				ReStartPart(e_TUTORIAL_PART);
			}

			TutorialPart.Part part = tutorialPart.parts[(int)e_TUTORIAL_PART];
			for (int i = 0; i < part.conditions.Length; i++)
			{
				if (part.conditions[i].achievement)
				{
					// �N���A�ς݂Ȃ�X�L�b�v
					continue;
				}
				else
				{
					// �N���A����
					part.conditions[i].achivementCondition?.Invoke(part.conditions[i]);
				}
				if (part.conditions[i].achievement)
				{
					// �N���A���̏���
					part.conditions[i].conditionExit?.Invoke();
					part.conditions[i].conditionExitFromCS?.Invoke();
					ConditionUIManager.Instance.CheckOn(i);
					//already[i] = true;
				}
			}

			// �S�B�����Ă���Ύ��̃p�[�g��
			if (!steping && tutorialPart.Complete(part) && e_TUTORIAL_PART < E_TUTORIAL_PART.EXTERMINATION + 1)
			{
				ShowSuccess();
				StepPart();
			}


			//----- �|�[�Y��� ON / OFF -----
			if (input.Menu.Pause.triggered)
			{
				pauseCanvas.SetActive(!pauseCanvas.activeSelf);
			}
		}


		private void OnDisable()
		{
			alreadyFinish = true;
		}


		private bool IsPlayerLive()
		{
			return playerCS.GetHp() > 0f;
		}


		#region �p�u���b�N�֐�
		public void ReStartPart(E_TUTORIAL_PART e_TUTORIAL_PART)
		{
			this.e_TUTORIAL_PART = e_TUTORIAL_PART;

			playerCS.OnHitEnemyAttack(-100, playerCS.transform.position);
			playerCS.ReBorn();
			playerCS.transform.position = new Vector3(0f, 0.25f, -5f);
			ClearAllEnemy();
			tutorialPart.parts[(int)e_TUTORIAL_PART].enterFunc?.Invoke();
			//StartMobillityPart();
			steping = false;
		}


		/// <summary>
		/// �`���[�g���A���p�[�g��i�߂�
		/// </summary>
		public void StepPart()
		{
			steping = true;
			StartCoroutine(DelayMethod(1f, () =>
			   {
				   tutorialPart.parts[(int)e_TUTORIAL_PART].exitFunc?.Invoke();
				   e_TUTORIAL_PART++;
				   //tutorialPart.parts[(int)e_TUTORIAL_PART].enterFunc?.Invoke();
				   tutorialPart.parts[(int)e_TUTORIAL_PART].enterFunc?.Invoke();
				   steping = false;
			   }));
		}


		public void GameScene()
		{
			Mikami.TransitionManager.Instance.StartTransaction(Mikami.Scenes.Game);
			var soundManager = Mizuno.SoundManager.Instance;
			soundManager.StopBGMWithFade(1f);
		}

		/// <summary>
		/// �n���ꂽ�������w�莞�Ԍ�Ɏ��s����
		/// </summary>
		/// <param name="time">�x������[�b]</param>
		/// <param name="action">���s����������</param>
		/// <returns></returns>
		private IEnumerator DelayMethod(float time, Action action)
		{
			yield return new WaitForSeconds(time);
			action();
		}


		/// <summary>
		/// �V�[����̃`���[�g���A���G�����ׂď���
		/// </summary>
		public void ClearAllEnemy()
		{
			spawner.AllDisactive();
			spawner2.AllDisactive();
		}

		#region �C�x���g�p
		#region �s�������n
		public void DisablePlayerAction()
		{
			playerCS.input.Player.Disable();
			satelliteCS.input.Player.Disable();
		}

		public void DisableAttack()
		{
			playerCS.input.Player.Attack.Disable();
			satelliteCS.input.Player.Attack.Disable();
		}

		public void EnablePlayerAction()
		{
			playerCS.input.Enable();
			satelliteCS.input.Player.Enable();
		}

		public void EnablePlayerMove()
		{
			playerCS.input.Player.Move.Enable();
		}

		public void DisablePlayerMove()
		{
			if (playerCS.input == null)
				StartCoroutine(DelayMethod(0.01f, () => DisablePlayerMove()));
			else
				playerCS.input.Player.Move.Disable();
		}

		public void EnablePlayerAttack()
		{
			playerCS.input.Player.Attack.Enable();
			satelliteCS.input.Player.Attack.Enable();
		}

		public void EnablePlayerSteal()
		{
			playerCS.input.Player.Steal.Enable();
			satelliteCS.input.Player.Steal.Enable();
		}


		public void EnablePlayerJump()
		{
			playerCS.input.Player.Jump.Enable();
			satelliteCS.input.Player.Jump.Enable();
		}


		public void EnableComboGuage()
		{
			playerCS.isDecraseComboGauge = true;
		}


		public void DisableComboGuage()
		{
			playerCS.isDecraseComboGauge = false;
		}
		#endregion

		#region �X�|�[���n

		public void SpawnMobillity()
		{
			Vector3 pos = playerTrans.position - playerTrans.transform.position.normalized * 4f;
			pos.y = 5;
			spawner.SpawnEnemy(TutorialEnemySpawner.ENEMY_KIND.MOBILLITY, pos, Vector3.back);
		}

		public void SpawnPower()
		{
			Vector3 pos = playerTrans.position - playerTrans.transform.position.normalized * 4f;
			pos.y = 5;
			spawner.SpawnEnemy(TutorialEnemySpawner.ENEMY_KIND.POWER, pos, Vector3.back);
		}

		public void SpawnReach()
		{
			Vector3 pos = playerTrans.position - playerTrans.transform.position.normalized * 4f;
			pos.y = 5;
			spawner.SpawnEnemy(TutorialEnemySpawner.ENEMY_KIND.REACH, pos, Vector3.back);
		}

		public void SpawnHeal()
		{
			Vector3 pos = new Vector3(-18.55f, 3.4f, 0.76f);
			GameObject enemy = spawner.SpawnEnemy(TutorialEnemySpawner.ENEMY_KIND.HEAL, pos, Vector3.forward);
			CameraController.Instance.ForcedForcus(enemy);
		}

		public void SpawnEnemies()
		{
			for (TutorialEnemySpawner.ENEMY_KIND kind = 0; kind < TutorialEnemySpawner.ENEMY_KIND.HEAL; kind++)
			{
				for (int i = 0; i < 2; i++)
				{
					float range = UnityEngine.Random.Range(0f, 20f);
					float rand = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
					Vector3 pos = new Vector3();
					pos.x = Mathf.Cos(rand) * range;
					pos.y = 5f;
					pos.z = Mathf.Sin(rand) * range;
					Vector3 forward = pos.normalized;
					forward.y = 0f;
					spawner2.SpawnEnemy(kind, pos, forward.normalized);
				}
			}
		}
		#endregion


		public void PlayerCauseDamage(float damage)
		{
			playerCS.OnHitEnemyAttack(damage, playerCS.transform.position);
		}


		public void MemoryPlayerHP()
		{
			mem_PlayerHP = playerCS.GetHp();
		}



		#region �I������
		public void IsLockOn(TutorialPart.Condition condition)
		{
			condition.achievement = CameraController.Instance.IsLocking();
		}

		public void IsMobillityStyle(TutorialPart.Condition condition)
		{
			condition.achievement = playerStyle.style == Style.E_Style.MOBILITY;
		}


		public void IsReachStyle(TutorialPart.Condition condition)
		{
			condition.achievement = playerStyle.style == Style.E_Style.REACH;
		}


		public void IsPowerStyle(TutorialPart.Condition condition)
		{
			condition.achievement = playerStyle.style == Style.E_Style.POWER;
		}

		public void IsEnemyCompleteDestroy(TutorialPart.Condition condition)
		{
			condition.achievement = spawner.IsEnemyCompleteDestroy() && spawner2.IsEnemyCompleteDestroy();
		}



		public void IsHealed(TutorialPart.Condition condition)
		{
			condition.achievement = mem_PlayerHP < playerCS.GetHp();
		}


		public void ClearUI()
		{
			ConditionUIManager.Instance.DestroyAll();
			//successUI.SetActive(false);
			successEffect.Stop();
		}

		public void CreateUI()
		{
			ConditionUIManager.Instance.CreateTutorialPartUI();
		}


		public void ShowSuccess()
		{
			//successUI.SetActive(true);
			successEffect.Play();
		}
		#endregion
		#endregion


		#region �|�[�Y�E�B���h�E�p
		public void ClosePause()
		{
			pauseCanvas.SetActive(false);
		}


		public void SkipTutorial()
		{
			TimeManager.Instance.NormalTime();
			GameScene();
		}
		#endregion
		#endregion
	}
}