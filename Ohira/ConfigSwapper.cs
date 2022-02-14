using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Ohira.Auxiliary;
using UnityEngine.UI;
using UnityEngine.Events;

public class ConfigSwapper : MonoBehaviour
{
	[SerializeField] PlayerControler input;
	[SerializeField] private GameObject[] configs;
	[SerializeField] private GameObject[] labels;
	[SerializeField] private GameObject[] tabs;
	[SerializeField] private Sprite[] sprites;

	[SerializeField] private UnityEvent[] resetFunc;

	private int activeWindow = 0;
	private RectTransform[] rect = new RectTransform[2];
	private bool isBig;
	private bool isLmove;
	private bool isRmove;

	private void Start()
	{
		input = new PlayerControler();
		input.Enable();
		activeWindow = 0;

		labels[0].GetComponent<Image>().sprite = sprites[1];
		labels[1].GetComponent<Image>().sprite = sprites[2];

		for (int i = 0; i < 2; i++)
			rect[i] = tabs[i].GetComponent<RectTransform>();

		isLmove = false;
		isRmove = false;
	}

	private void OnEnable()
	{
		foreach (var config in configs)
		{
			config.SetActive(false);
		}

		activeWindow = 0;
		configs[activeWindow].SetActive(true);
	}


	private void Update()
	{
		if (input.Menu.L.triggered)
		{
			configs[activeWindow].SetActive(false);

			activeWindow -= 1;
			if (activeWindow.IsLessThan(-1))
			{
				//activeWindow = configs.Length - 1;
				activeWindow = 0;
			}

			configs[activeWindow].SetActive(true);

			// 画像切替
			labels[0].GetComponent<Image>().sprite = sprites[1];
			labels[1].GetComponent<Image>().sprite = sprites[2];

			// L押されたときの動き
			//rect[0].localScale = new Vector3(1, 1, 1);
			isLmove = true;
		}
		else if (input.Menu.R.triggered)
		{
			configs[activeWindow].SetActive(false);

			activeWindow += 1;
			if (activeWindow.IsMoreThan(configs.Length))
			{
				//activeWindow = 0;
				activeWindow = 1;
			}

			configs[activeWindow].SetActive(true);

			labels[0].GetComponent<Image>().sprite = sprites[0];
			labels[1].GetComponent<Image>().sprite = sprites[3];

			// R押された時の動き
			//rect[0].localScale = new Vector3(1, 1, 1);
			isRmove = true;
		}

		// LRの動き
		if (isLmove)
		{
			rect[0].localScale -= rect[0].localScale * (0.05f * 3);

			if (rect[0].localScale.x <= 0.5f)
			{
				for (int i = 0; i < 2; i++)
					rect[i].localScale = new Vector3(1, 1, 1);

				isLmove = false;
			}
		}
		else if (isRmove)
		{
			rect[1].localScale -= rect[1].localScale * (0.05f * 3);

			if (rect[1].localScale.x <= 0.5f)
			{
				for (int i = 0; i < 2; i++)
					rect[i].localScale = new Vector3(1, 1, 1);

				isRmove = false;
			}
		}
		else if (!isLmove && !isRmove)
		{
			for (int i = 0; i < 2; i++)
			{
				if (isBig)
					rect[i].localScale += rect[i].localScale * (0.05f * 0.2f);
				else
					rect[i].localScale -= rect[i].localScale * (0.05f * 0.2f);
			}
		}

		// チェック
		if (rect[0].localScale.x > 1.2f)
			isBig = false;
		else if (rect[0].localScale.x < 0.8f)
			isBig = true;


		if (input.Menu.Reset.triggered)
		{
			resetFunc[activeWindow]?.Invoke();
		}
	}
}
