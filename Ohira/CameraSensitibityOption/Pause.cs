using UnityEngine;

namespace Ohira
{
	public class Pause : MonoBehaviour
	{
		private void OnEnable()
		{
			TimeManager.Instance?.Stop();
		}



		private void OnDisable()
		{
			TimeManager.Instance?.NormalTime();
		}
	}
}