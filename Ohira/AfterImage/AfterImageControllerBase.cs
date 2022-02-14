using System;
using System.Threading;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;
using Unity.Collections;

/// <summary>
/// �c���̑���p�R���|�[�l���g
/// </summary>
[RequireComponent(typeof(ObservableUpdateTrigger), typeof(ObservableDestroyTrigger))]
public abstract class AfterImageControllerBase : MonoBehaviour
{
	[SerializeField, Header("����������c���I�u�W�F�N�g")] protected AfterImageBase _afterImage = null;
	[SerializeField, Header("�c���̐e�I�u�W�F�N�g")] static protected Transform _afterImageParent = null;
	[SerializeField, Header("���O������")] protected int _preLoadCount = 5;
	[SerializeField, Header("�c���̐����Ԋu")] protected float _createIntervalTime = 0.1f;
	[SerializeField, Header("�c���̐�������")] protected float _afterImageLifeTime = 0.2f;
	[SerializeField, Header("�c���𐶐����邩")] protected BoolReactiveProperty _isCreate = new BoolReactiveProperty(false);
	[SerializeReference, Header("����������c���I�u�W�F�N�g"), ReadOnly] protected IAfterImageSetupParam _param = null;

	protected AfterImagePool _pool = null;
	protected CompositeDisposable _disposable = new CompositeDisposable();

	// �œK���p
	protected ObservableUpdateTrigger _updateTrigger = null;

	public float createIntervalTime { get; set; }
	public float afterImageLifeTime { get; set; }
	public bool isCreate { get => _isCreate.Value; set => _isCreate.Value = value; }
	public bool isInitialized { get; private set; } = false;

	/// <summary>
	/// �p�����[�^�[�̐ݒ�
	/// </summary>
	protected abstract void SetupParam();

	private void Reset()
	{
		SetupParam();
	}

	private void Awake()
	{
		// �p�����[�^�[����
		if (_param == null)
		{
			SetupParam();
		}

		// Update�̔��s�����L���b�V�����Ă���
		_updateTrigger = GetComponent<ObservableUpdateTrigger>();

		// �v�[���̏���
		_afterImageParent = AfterImageParent.Instance.transform;
		_pool = new AfterImagePool(_afterImage, _afterImageParent);
		_pool.PreloadAsync(_preLoadCount, 1)
			.TakeUntilDestroy(this)
			.Subscribe(_ => { }, exception => { Debug.LogException(exception); }, () => { isInitialized = true; });

		// �t���O�ɉ����ď����̓o�^�Ɣj�����s��
		_isCreate
			.TakeUntilDestroy(this)
			.Where(_ => isInitialized)
			.Subscribe(enable =>
			{
				if (!enable)
				{
					_disposable.Clear();
					return;
				}

				Observable.Interval(TimeSpan.FromSeconds(_createIntervalTime))
						.Subscribe(_ =>
						{
							// �v�[������c�����擾���ăI���W�i���̃|�[�Y�ƍ��킹��
							AfterImageBase image = _pool.Rent();
							image.Setup(_param);

							// ���Ԍo�ߏ����ƏI�����Ƀv�[���ɖ߂�������o�^���Ă���
							float currentTime = 0f;
							_updateTrigger.UpdateAsObservable()
								.TakeUntilDisable(image)
								.Subscribe(unit =>
								{
									currentTime += Time.deltaTime;
									image.rate = currentTime / _afterImageLifeTime;
									if (currentTime >= _afterImageLifeTime)
									{
										_pool.Return(image);
									}
								}
								//,() => { Debug.Log("����ɔj������Ă��܂�"); }
								);
						})
						.AddTo(_disposable);
			});

		isInitialized = true;
	}



	private void OnDestroy()
	{
		_disposable.Dispose();
		_disposable = null;
	}
}
