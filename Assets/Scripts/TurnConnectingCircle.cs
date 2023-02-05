using System.Collections;
using UnityEngine;

public class TurnConnectingCircle : MonoBehaviour
{
	[System.Obsolete]
	private void OnEnable()
	{
		StartCoroutine(Turn());
	}

	[System.Obsolete]
	private IEnumerator Turn()
	{
		GameObject parent = transform.parent.gameObject;
		float standardTime = Time.time;

		while (true)
		{
			if (parent.active)
			{
				transform.rotation = Quaternion.Euler(0, 0, (standardTime - Time.time) * 150);
			}
			yield return null;
		}
	}
}
