using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mikami;


namespace Ohira
{
	public class ConditionUIManager : SingletonMono<ConditionUIManager>
	{
		[SerializeField] private GameObject conditionUIPref;

		[SerializeField] private List<GameObject> UIs = new List<GameObject>();
		private List<TutorialUICondition> tutorialUIConditionCSs = new List<TutorialUICondition>();
		[SerializeField] private GameObject clearLogo;

		private void Start()
		{
			foreach (var UI in UIs)
			{
				tutorialUIConditionCSs.Add(UI.GetComponent<TutorialUICondition>());
			}
		}

		public void CheckOn(int element)
		{
			tutorialUIConditionCSs[element].CheckOn();
		}


		public void CheckOff(int element)
		{
			tutorialUIConditionCSs[element].CheckOff();
		}


		public void DestroyAll()
		{
			foreach (var UI in UIs)
			{
				UI.SetActive(false);
			}
		}


		public void CreateTutorialPartUI()
		{
			var part = TutorialManager.Instance.tutorialPart.parts[(int)TutorialManager.Instance.e_TUTORIAL_PART];
			int diff = part.conditions.Length - UIs.Count;
			for (int i = 0; i < diff; i++)
			{
				var UI = Instantiate(conditionUIPref);
				UI.transform.parent = this.transform;

				UIs.Add(UI);
				tutorialUIConditionCSs.Add(UI.GetComponent<TutorialUICondition>());
			}

			int j = 0;
			foreach (var condition in part.conditions)
			{
				UIs[j].SetActive(true);
				var controller = tutorialUIConditionCSs[j]; //UIs[j].GetComponent<TutorialUICondition>();
				controller.CheckOff();
				controller.SetText(condition.text);

				j++;
			}
		}


		public void ShowClearLogo()
		{
			clearLogo.SetActive(true);
		}



		private void OnDestroy()
		{
			UIs.Clear();
		}
	}
}