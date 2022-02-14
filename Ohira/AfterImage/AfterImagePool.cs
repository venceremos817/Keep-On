using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Toolkit;

public class AfterImagePool : ObjectPool<AfterImageBase>
{
	private AfterImageBase _gameObject = null;
	private Transform _parent = null;


	public AfterImagePool(AfterImageBase gameObject,Transform parent)
	{
		SetPoolObject(gameObject);
		SetParent(parent);
	}

	~AfterImagePool()
	{
		Clear();
	}



	protected override AfterImageBase CreateInstance()
	{
		if (_gameObject==null)
		{
			Debug.LogError("��������GameObject��null�ł�");
			return null;
		}
		if (_parent==null)
		{
			Debug.LogError("�eTransform��null�ł�");
			return null;
		}

		AfterImageBase afterImage = GameObject.Instantiate<AfterImageBase>(_gameObject, _parent, true);
		afterImage.Initialize();
		return afterImage;
	}


	/// <summary>
	/// �����I�u�W�F�N�g�̐ݒ�
	/// </summary>
	/// <param name="gameObject"></param>
	public void SetPoolObject(AfterImageBase gameObject)
	{
		_gameObject = gameObject;
	}


	/// <summary>
	/// �����I�u�W�F�N�g�̐e�I�u�W�F�N�g�̐ݒ�
	/// </summary>
	/// <param name="parent"></param>
	public void SetParent(Transform parent)
	{
		_parent = parent;
	}
}
