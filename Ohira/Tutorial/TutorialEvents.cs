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
	/// すべての敵をクリア
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
	/// 機動敵をスポーン
	/// </summary>
	public void SpawnMobillity()
	{
		TutorialManager.Instance.SpawnMobillity();
	}

	/// <summary>
	/// リーチ敵をスポーン
	/// </summary>
	public void SpawnReach()
	{
		TutorialManager.Instance.SpawnReach();
	}

	/// <summary>
	/// パワー敵をスポーン
	/// </summary>
	public void SpawnPower()
	{
		TutorialManager.Instance.SpawnPower();
	}

	/// <summary>
	/// ヒールドローンをスポーン
	/// </summary>
	public void SpawnHeal()
	{
		TutorialManager.Instance.SpawnHeal();
	}

	/// <summary>
	/// 3種の敵をスポーン
	/// </summary>
	public void SpawnEnemies()
	{
		TutorialManager.Instance.SpawnEnemies();
	}

	/// <summary>
	/// プレイヤーの全行動を不可にする
	/// </summary>
	public void DisablePlayerAction()
	{
		TutorialManager.Instance.DisablePlayerAction();
	//	tutorialManagerCS.DisablePlayerAction();
	}

	/// <summary>
	/// プレイヤーの攻撃を不可にする
	/// </summary>
	public void DisablePlayerAttack()
	{
		TutorialManager.Instance.DisableAttack();
	}

	/// <summary>
	/// プレイヤーのすべての行動を許可する
	/// </summary>
	public void EnablePlayerAction()
	{
		TutorialManager.Instance.EnablePlayerAction();
	//	tutorialManagerCS.EnablePlayerAction();
	}

	/// <summary>
	/// プレイヤーの移動を許可する
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
	/// プレイヤーの攻撃を許可する
	/// </summary>
	public void EnablePlayerAttack()
	{
		TutorialManager.Instance.EnablePlayerAttack();
	//	tutorialManagerCS.EnablePlayerAttack();
	}

	/// <summary>
	/// プレイヤーの奪取を許可する
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
	/// 現時点のプレイヤーHPを記録する
	/// </summary>
	public void MemortPlayerHP()
	{
		TutorialManager.Instance.MemoryPlayerHP();
	}

	/// <summary>
	/// すべての敵を倒していればOK
	/// </summary>
	/// <param name="condition"></param>
	public void IsEnemyCompleteDestroy(TutorialPart.Condition condition)
	{
		TutorialManager.Instance.IsEnemyCompleteDestroy(condition);
	}

	/// <summary>
	/// 回復すればOK
	/// </summary>
	/// <param name="condition"></param>
	public void IsHealed(TutorialPart.Condition condition)
	{
		TutorialManager.Instance.IsHealed(condition);
	}
}