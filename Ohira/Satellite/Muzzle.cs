using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Ohira
{
	public class Muzzle : MonoBehaviour
	{

		[SerializeField] private Transform _muzzle;

		public Transform muzzle
		{
			get => _muzzle;
			private set
			{
				_muzzle = value;
			}
		}
	}
}