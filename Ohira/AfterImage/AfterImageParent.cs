using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImageParent : SingletonMono<AfterImageParent>
{
	public void Clear()
	{
		gameObject.SetActive(false);
	}
}
