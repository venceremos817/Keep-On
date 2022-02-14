using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ohira
{
	public class TutorialPause : MonoBehaviour
	{
		[SerializeField] private RectTransform moveRect;
		[SerializeField] private float speed = 1.0f;
		private Vector3 startPos;
		private float time;


		private void Update()
		{
			moveRect.localPosition = Vector3.Lerp(startPos, Vector3.zero, time);

			time += Time.unscaledDeltaTime * speed;
		}


		private void OnEnable()
		{
			TutorialEvents.Instance.DisablePlayerMove();
			CameraController.Instance.input.Camera.Disable();

			startPos = moveRect.localPosition;
			time = 0f;
		//	TimeManager.Instance.Stop();
		}


		private void OnDisable()
		{
			TutorialEvents.Instance?.EnablePlayerMove();
			CameraController.Instance?.input.Camera.Enable();

			moveRect.localPosition = startPos;

		//	TimeManager.Instance?.NormalTime();
		}
	}
}