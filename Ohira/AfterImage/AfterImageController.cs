using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// �c���̑���p�R���|�[�l���g
/// </summary>
public class AfterImageController : AfterImageControllerBase
{
	[SerializeField, Header("�c���̔������ƂȂ�I�u�W�F�N�g")] private Transform _originalTransform = null;

	protected override void SetupParam()
	{
		_param = new SimpleAfterImageParam(_originalTransform);
	}
}
