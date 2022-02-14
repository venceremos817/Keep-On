using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// �c���o�����ɐݒ肷�邽�߂̃p�����[�^���ۃN���X
/// </summary>
public interface IAfterImageSetupParam { }


/// <summary>
/// �ʏ�̎c��
/// </summary>
[System.Serializable]
public class SimpleAfterImageParam : IAfterImageSetupParam
{
	public SimpleAfterImageParam() { }
	public SimpleAfterImageParam(Transform original)
	{
		transform = original;
	}

	~SimpleAfterImageParam()
	{
		transform = null;
	}

	public Transform transform;
}



/// <summary>
/// �����̃{�[�����g�p����ꍇ
/// </summary>
[System.Serializable]
public class AfterImageSkinnedMeshParam : IAfterImageSetupParam
{
	public AfterImageSkinnedMeshParam() { }
	public AfterImageSkinnedMeshParam(List<Transform> original)
	{
		transforms = original;
	}

	~AfterImageSkinnedMeshParam()
	{
		transforms.Clear();
		transforms = null;
	}

	public List<Transform> transforms = null;
}