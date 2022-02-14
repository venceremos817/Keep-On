using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Ohira
{
	public class TutorialUICondition : MonoBehaviour
	{
		[SerializeField] private GameObject checkMark;
		[SerializeField] private TextMeshProUGUI textMeshProUGUI;


		public void CheckOn()
		{
			checkMark.SetActive(true);
		}


		public void CheckOff()
		{
			checkMark.SetActive(false);
		}


		public void SetText(string text)
		{
			textMeshProUGUI.text = text;
		}
	}
}