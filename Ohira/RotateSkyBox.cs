using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSkyBox : MonoBehaviour
{
	[SerializeField] private float speed = 0.5f;
	private Material skyBoxMat;


	private void Start()
	{
		skyBoxMat = RenderSettings.skybox;
	}


	private void Update()
	{
		skyBoxMat.SetFloat("_Rotation", Mathf.Repeat(skyBoxMat.GetFloat("_Rotation") + speed * Time.deltaTime, 360f));
	}
}
