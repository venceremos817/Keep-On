using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ohira
{
	public class CameraSensitivityOption : MonoBehaviour
	{
		[SerializeField] private Vector2Variable cameraSensitive;
		[SerializeField] private Slider sliderX;
		[SerializeField] private Slider sliderY;

		private void OnEnable()
		{
			sliderX.value = cameraSensitive.value.x;
			sliderY.value = cameraSensitive.value.y;
		}

		private void Start()
		{
			sliderX.value = cameraSensitive.value.x;
			sliderY.value = cameraSensitive.value.y;
		}


		private void OnDisable()
		{
			cameraSensitive.value.x = sliderX.value;
			cameraSensitive.value.y = sliderY.value;
		}


		public void ResetValue()
		{
			sliderX.value = cameraSensitive.value.x = cameraSensitive.InitialValue.x;
			sliderY.value = cameraSensitive.value.y = cameraSensitive.InitialValue.y;
		}
	}
}