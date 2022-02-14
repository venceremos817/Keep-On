using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Ohira
{
	public class AroundCamera : MonoBehaviour
	{
		public Transform center;
		public float height = 10f;
		public float radius = 20f;
		public float rotateSpeed = 1f;

		private float progressTime;

		// Start is called before the first frame update
		void Start()
		{
			progressTime = 0f;
		}

		// Update is called once per frame
		void Update()
		{
			progressTime += Time.deltaTime * rotateSpeed;

			Vector3 pos;
			pos.x = Mathf.Cos(progressTime) * radius;
			pos.z = Mathf.Sin(progressTime) * radius;
			pos.y = height;
			transform.position = pos;

			transform.LookAt(center);
		}
	}
}