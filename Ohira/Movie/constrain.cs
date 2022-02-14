using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class constrain : MonoBehaviour
{
	private Vector3 pos;

	private void Start()
	{
		pos = transform.position;
	}

	// Update is called once per frame
	void Update()
    {
		transform.position = pos;
    }
}
