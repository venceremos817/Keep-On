using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ohira;

public partial class TutorialEvents : SingletonMono<TutorialEvents>
{
	//TutorialManager tutorialManagerCS;


	protected override void Awake()
	{
		base.Awake();
	}

	
	/// <summary>
	/// ���ׂĂ̓G���N���A
	/// </summary>
	public void ClearEnemy()
	{
		TutorialManager.Instance.ClearAllEnemy();
	//	tutorialManagerCS.ClearAllEnemy();
	}


	public void ClearUI()
	{
		TutorialManager.Instance.ClearUI();
	}

	/// <summary>
	/// �@���G���X�|�[��
	/// </summary>
	public void SpawnMobillity()
	{
		TutorialManager.Instance.SpawnMobillity();
	}

	/// <summary>
	/// ���[�`�G���X�|�[��
	/// </summary>
	public void SpawnReach()
	{
		TutorialManager.Instance.SpawnReach();
	}

	/// <summary>
	/// �p���[�G���X�|�[��
	/// </summary>
	public void SpawnPower()
	{
		TutorialManager.Instance.SpawnPower();
	}

	/// <summary>
	/// �q�[���h���[�����X�|�[��
	/// </summary>
	public void SpawnHeal()
	{
		TutorialManager.Instance.SpawnHeal();
	}

	/// <summary>
	/// 3��̓G���X�|�[��
	/// </summary>
	public void SpawnEnemies()
	{
		TutorialManager.Instance.SpawnEnemies();
	}

	/// <summary>
	/// �v���C���[�̑S�s����s�ɂ���
	/// </summary>
	public void DisablePlayerAction()
	{
		TutorialManager.Instance.DisablePlayerAction();
	//	tutorialManagerCS.DisablePlayerAction();
	}

	/// <summary>
	/// �v���C���[�̍U����s�ɂ���
	/// </summary>
	public void DisablePlayerAttack()
	{
		TutorialManager.Instance.DisableAttack();
	}

	/// <summary>
	/// �v���C���[�̂��ׂĂ̍s����������
	/// </summary>
	public void EnablePlayerAction()
	{
		TutorialManager.Instance.EnablePlayerAction();
	//	tutorialManagerCS.EnablePlayerAction();
	}

	/// <summary>
	/// �v���C���[�̈ړ���������
	/// </summary>
	public void EnablePlayerMove()
	{
		TutorialManager.Instance.EnablePlayerMove();
	//	tutorialManagerCS.EnablePlayerMove();
	}


	public void DisablePlayerMove()
	{
		TutorialManager.Instance.DisablePlayerMove();
	}

	/// <summary>
	/// �v���C���[�̍U����������
	/// </summary>
	public void EnablePlayerAttack()
	{
		TutorialManager.Instance.EnablePlayerAttack();
	//	tutorialManagerCS.EnablePlayerAttack();
	}

	/// <summary>
	/// �v���C���[�̒D���������
	/// </summary>
	public void	 EnablePlayerSteal()
	{
		TutorialManager.Instance.EnablePlayerSteal();
	//	tutorialManagerCS.EnablePlayerSteal();
	}


	public void EnablePlayerJump()
	{
		TutorialManager.Instance.EnablePlayerJump();
	}

	public void PlayerCauseDamage(float damage)
	{
		TutorialManager.Instance.PlayerCauseDamage(damage);
	}


	public void EnableComboGuage()
	{
		TutorialManager.Instance.EnableComboGuage();
	}


	public void DisableComboGuage()
	{
		TutorialManager.Instance.DisableComboGuage();
	}

	/// <summary>
	/// �����_�̃v���C���[HP���L�^����
	/// </summary>
	public void MemortPlayerHP()
	{
		TutorialManager.Instance.MemoryPlayerHP();
	}

	/// <summary>
	/// ���ׂĂ̓G��|���Ă����OK
	/// </summary>
	/// <param name="condition"></param>
	public void IsEnemyCompleteDestroy(TutorialPart.Condition condition)
	{
		TutorialManager.Instance.IsEnemyCompleteDestroy(condition);
	}

	/// <summary>
	/// �񕜂����OK
	/// </summary>
	/// <param name="condition"></param>
	public void IsHealed(TutorialPart.Condition condition)
	{
		TutorialManager.Instance.IsHealed(condition);
	}
}