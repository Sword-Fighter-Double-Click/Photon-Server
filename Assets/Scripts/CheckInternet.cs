using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckInternet : MonoBehaviour
{
	public static bool InternetConnected;

	public static bool gamePlayMode;

	private void Start()
	{
		DontDestroyOnLoad(gameObject);

		StartCoroutine(Repetition());
	}

	private IEnumerator Repetition()
	{
		while (true)
		{
			InternetConnected = !(Application.internetReachability == NetworkReachability.NotReachable);
			yield return null;
		}
	}
}
