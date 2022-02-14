using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ohira
{
	public class ConfigManager : MonoBehaviour
	{
		[SerializeField] private GameObject configWindow;
		[SerializeField] private UnityEvent OnShow = null;
		[SerializeField] public UnityEvent OnHide = null;
		private PlayerControler input;

		private void Start()
		{
			input = new PlayerControler();
			input.Enable();
			input.Menu.Cancel.Disable();
		}


		private void OnEnable()
		{
			input?.Enable();
		}


		private void Update()
		{
			if (input.Menu.Pause.triggered || input.Menu.Cancel.triggered)
			{
				if (configWindow.activeSelf == false)
				{
					OnShow?.Invoke();
					configWindow.SetActive(true);
					input.Menu.Cancel.Enable();
				}
				else
				{
					OnHide?.Invoke();
					configWindow.SetActive(false);
					input.Menu.Cancel.Disable();
				}
			}
		}

		public void DisabelInput()
		{
			input.Disable();
			OnHide?.Invoke();
			configWindow.SetActive(false);
		}
	}
}