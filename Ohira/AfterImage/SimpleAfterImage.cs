using System;
using UnityEngine;

/// <summary>
/// �c���p�I�u�W�F�N�g�ɃA�^�b�`����R���|�[�l���g
/// </summary>
public class SimpleAfterImage : AfterImageBase
{
	[SerializeField] private MeshRenderer _meshRenderer = null;

	/// <summary>
	/// ����������
	/// </summary>
	public override void Initialize()
	{
		if (isInitialized)
		{
			return;
		}
		base.Initialize();

		if (_meshRenderer == null)
		{
			throw new Exception("Mesh Renderer null");
		}

		// �}�e���A����ݒ�
		_meshRenderer.material = _material;
		isInitialized = true;
	}

	/// <summary>
	/// �c���̊J�n���̐ݒ菈��
	/// </summary>
	public override void Setup(IAfterImageSetupParam param)
	{
		if (param is SimpleAfterImageParam simpleParam)
		{
			transform.position = simpleParam.transform.position;
			transform.rotation = simpleParam.transform.rotation;
			transform.localScale = simpleParam.transform.localScale;
		}
		else
		{
			Debug.LogError("������SimpleAfterImageParam�ɃL���X�g�o���܂���ł���");
		}

		rate = 0f;
	}
}
