using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Ohira.Auxiliary;

namespace Ohira
{
	public class VolumeDirecting : MonoBehaviour
	{
		[SerializeField] private Volume _Volume;
		[SerializeField] private Maeda.Player player;
		private float maxHP = 100f;


		private Vignette _Vignette;
		private Color vignetteStartColor;
		private float vignetteStartIntensity;
		private float vignetteSmoothness;

		private void Start()
		{
			_Vignette = null;
			_Volume.profile.TryGet<Vignette>(out _Vignette);
			vignetteStartColor = _Vignette.color.value;
			vignetteStartIntensity = _Vignette.intensity.value;
			vignetteSmoothness = _Vignette.smoothness.value;
		}

		private void Update()
		{
			float ratio = (maxHP - player.GetHp()) / maxHP;

			_Vignette.color.value = Color.Lerp(vignetteStartColor, Color.red, ratio * 0.5f);

			if (ratio.IsMoreThan(0.7f))
			{
				ratio *= 0.3f;
				_Vignette.intensity.value = Mathf.Lerp(vignetteStartIntensity, 1f, ratio);
				_Vignette.smoothness.value = Mathf.Lerp(vignetteSmoothness, 1f, ratio);
			}
			else
			{
				_Vignette.intensity.value = vignetteStartIntensity;
				_Vignette.smoothness.value = vignetteSmoothness;
			}
		}
	}
}