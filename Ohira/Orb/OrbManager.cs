using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Ohira
{
	public class OrbManager : SingletonMono<OrbManager>
	{
		[SerializeField] private Transform player;
		[SerializeField] private List<Orb> orbs;


		private void Start()
		{
			Orb.player = player;

			foreach (var orb in orbs)
			{
				orb.gameObject.SetActive(false);
			}
		}


        public void EmitOrb(Vector3 startPos, Style.E_Style style)
		{
			foreach (var orb in orbs)
			{
				if (orb.gameObject.activeSelf)
				{
					continue;
				}

				orb.Emit(startPos, style);
			}
		}
	}
}