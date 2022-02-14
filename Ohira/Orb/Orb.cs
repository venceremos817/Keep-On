using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


namespace Ohira
{
	public class Orb : MonoBehaviour
	{
		private const float life = 1f;
		public static Transform player;

		[SerializeField] private VisualEffect orbVfx;

		private float progressTime = 0f;
		private Vector3 startPos;

		private void Start()
		{
			progressTime = 0f;
		}


		private void Update()
		{
			transform.position = Vector3.Slerp(startPos, player.position, progressTime / life);

			if (progressTime > life)
			{
				orbVfx.Stop();
				gameObject.SetActive(false);
			}

			progressTime += Time.deltaTime;
		}


        public void Emit(Vector3 startPos, Style.E_Style style)
		{
			gameObject.SetActive(true);
			transform.position = startPos;
			this.startPos = startPos;
			progressTime = 0f;
            orbVfx.SetInt("Type", (int)style);
			orbVfx.Play();
		}
	}
}