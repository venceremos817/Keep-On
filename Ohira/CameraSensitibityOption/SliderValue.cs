using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Ohira
{
	public class SliderValue : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI text;
		[SerializeField] private Slider slider;


		private void Start()
		{
			ReadSliderValue();
		}


		public void ReadSliderValue()
		{
			text.text = "" + slider.value;
		}
	}
}