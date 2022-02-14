using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;
using Unity.Collections;

/// <summary>
/// �c���p�I�u�W�F�N�g�̊��N���X
/// </summary>
public abstract class AfterImageBase : MonoBehaviour
{
	[SerializeField] protected Shader _shader = null;
	[SerializeField, RangeReactiveProperty(0f, 1f)] protected FloatReactiveProperty _rate = new FloatReactiveProperty(0f);
	[SerializeField] protected Gradient _gradient = null;
	[SerializeField, ReadOnly] protected Material _material = null;


	protected int _baseColorID = -1;

	public float rate { get => _rate.Value; set => _rate.Value = Mathf.Clamp01(value); }
	public bool isInitialized { get; protected set; } = false;


	private void Awake()
	{
		Initialize();
	}


	private void OnDestroy()
	{
		if (_material != null)
		{
			Destroy(_material);
		}

		_rate.Dispose();
	}


	/// <summary>
	/// ����������
	/// </summary>
	public virtual void Initialize()
	{
		if (isInitialized)
		{
			return;
		}

		if (_shader == null)
		{
			throw new Exception("Shader null");
		}

		// ID�擾
		_baseColorID = Shader.PropertyToID("_BaseColor");
		// �V�F�[�_�[�����Ƀ}�e���A�����쐬
		_material = new Material(_shader);
		// Rate�ɕω�����������Color��ݒ肷��悤�ɓo�^
		_rate.Subscribe(value => _material.SetColor(_baseColorID, _gradient.Evaluate(value)));
		rate = 0f;
	}


	/// <summary>
	/// �c���̊J�n���̐ݒ菈��
	/// </summary>
	/// <param name="param"></param>
	public abstract void Setup(IAfterImageSetupParam param);
}
