using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SliderController : MonoBehaviour
{
	[SerializeField] private EventSystem eventSystem;
	[SerializeField] private Slider firstSelect;


	private void OnEnable()
	{
		eventSystem.SetSelectedGameObject(firstSelect.gameObject);
	}
}
