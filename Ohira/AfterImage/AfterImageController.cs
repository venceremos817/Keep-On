using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 残像の操作用コンポーネント
/// </summary>
public class AfterImageController : AfterImageControllerBase
{
	[SerializeField, Header("残像の発生源となるオブジェクト")] private Transform _originalTransform = null;

	protected override void SetupParam()
	{
		_param = new SimpleAfterImageParam(_originalTransform);
	}
}
