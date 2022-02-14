using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearLogo : MonoBehaviour
{
	[SerializeField] private Material selfMat;
	float t;

	private void OnEnable()
	{
		//	selfMat = GetComponent<Material>();
		t = 0f;
	}

	private void Start()
	{
	}


	private void Update()
	{
		t += Time.deltaTime;
		//selfMat.Lerp(materials[0], materials[1], t);
		//	Debug.Log("oohira" + selfMat.GetFloat("_LightAngle"));
		var value = Mathf.Sin(t) * Mathf.PI + Mathf.PI;
		selfMat.SetFloat("_LightAngle", value);
	}

}