using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Ohira
{
	public class UpCamera : MonoBehaviour
	{
		public Transform target;
		public float upSpeed;
		public float rotateSpeed;
		public float radius;
		public float diff;

		private float time = 0f;

		private void Start()
		{
			time = 0f;
		}

		private void Update()
		{
			time += Time.deltaTime * rotateSpeed;

			Vector3 pos;

			pos.x = Mathf.Cos(time + diff) * radius;
			pos.z = Mathf.Sin(time + diff) * radius;
			pos.y = transform.position.y + Time.deltaTime * upSpeed;

			transform.position = pos;

			Vector3 targetPos;
			targetPos = target.position;
			targetPos.y = pos.y;

			transform.LookAt(targetPos);

		}
	}
}