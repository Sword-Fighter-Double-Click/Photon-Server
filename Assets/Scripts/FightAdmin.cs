using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FightAdmin : MonoBehaviour
{
	[SerializeField] private GameObject errorScreen;

	[SerializeField] private GameObject spiro, arkSha;

	private void Start()
    {
		if (CheckInternet.gamePlayMode)
		{
			GameObject gameObject = PhotonNetwork.Instantiate("PrototypeHero", PhotonNetwork.IsMasterClient ? new Vector3(-6, 0) : new Vector3(6, 0), Quaternion.identity);
			gameObject.tag = PhotonNetwork.IsMasterClient ? "Player1" : "Player2";
			StartCoroutine(SetFlipOnce());
			StartCoroutine(RoomIsFull());
		}
		else
		{
			GameObject gameObject1 = Instantiate(spiro, new Vector3(-6, 0), Quaternion.identity);
			GameObject gameObject2 = Instantiate(arkSha, new Vector3(6, 0), Quaternion.identity);
		}
    }
	
	// flip 인지 아닌지 확인하기!!
	private IEnumerator SetFlipOnce()
	{
		Prototype[] players;

		while ((players = FindObjectsOfType<Prototype>()).Length != 2)
		{
			yield return null;
		}

		for (int count = 0; count < players.Length; count++)
		{
			if (players[count].CompareTag("Untagged"))
			{
				players[count].tag = PhotonNetwork.IsMasterClient ? "Player2" : "Player1";
				players[count].GetComponent<SpriteRenderer>().flipX = PhotonNetwork.IsMasterClient;
			}
			else
			{
				players[count].GetComponent<SpriteRenderer>().flipX = !PhotonNetwork.IsMasterClient;
			}
		}
	}

    private IEnumerator RoomIsFull()
	{
		while (PhotonNetwork.CurrentRoom.PlayerCount.Equals(PhotonNetwork.CurrentRoom.MaxPlayers))
		{
			yield return null;
		}
		errorScreen.SetActive(true);
		PhotonNetwork.LeaveRoom();
		float countTime = 0f;
		while (PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.ConnectedToMasterServer)
		{
			yield return null;
		}
		while (countTime <= 2f)
		{
			countTime += Time.deltaTime;
			yield return null;
		}
		errorScreen.SetActive(false);
		SceneManager.LoadScene("Title");
	}
}
