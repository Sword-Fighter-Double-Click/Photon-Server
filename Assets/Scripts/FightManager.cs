using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor;

public class FightManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static int fighter1CharacterNumber = 0;
    public static int fighter2CharacterNumber = 1;

    [Header("Cashing")]
    [SerializeField] private List<GameObject> fighters = new List<GameObject>();

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI roundStateText;
    [SerializeField] private Image HPBar1, HPBar2, ultimateGageBar1, ultimateGageBar2;
	[SerializeField] private GameObject[] roundCounters1, roundCounters2;

    [Header("Value")]
    [SerializeField] private int roundTime = 60;

    public Fighter player1, player2;
    public Fighter clone1, clone2;
   
    private int player1WinCount = 0, player2WinCount = 0;

    private float countTime;

    private bool roundStarted = false;

    private void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            player1 = PhotonNetwork.Instantiate(fighters[fighter1CharacterNumber].name, new Vector3(-6, 0, 3), Quaternion.identity).GetComponent<Fighter>();
            player2 = PhotonNetwork.Instantiate(fighters[fighter2CharacterNumber].name, new Vector3(6, 0, 3), Quaternion.identity).GetComponent<Fighter>();
        }
    }

    private void Start()
    {
        player1.SetEnemyFighter(player2);
        player1.SettingUI();

        player2.SetEnemyFighter(player1);
        player2.SettingUI();

        StartCoroutine(NewRound());
    }

    private void Update()
    {
        if (!roundStarted) return;

        if (countTime <= 0)
        {
            StartCoroutine(HandleTimesUp());
            roundStarted = false;
        }

        if (player1.isDead || player2.isDead)
        {
            StartCoroutine(HandlePlayerDeath(player1.isDead, player2.isDead));
            roundStarted = false;
        }
    }

    private IEnumerator NewRound()
    {
        player1.ResetState();
        player2.ResetState();
        player1.OffInput();
        player2.OffInput();

        HPBar1.fillAmount = 1;
        HPBar2.fillAmount = 1;

        countTime = roundTime;
        timerText.text = countTime.ToString("F0");

        player1.transform.position = new Vector3(-6, 0, 3);
        player1.transform.eulerAngles = Vector3.zero;
        player2.transform.position = new Vector3(6, 0, 3);
        player2.transform.eulerAngles = Vector3.up * 180;

        roundStateText.gameObject.SetActive(true);

        roundStateText.text = "Ready...";

        yield return new WaitForSeconds(1);
        player1.OnInput();
        player2.OnInput();

        roundStateText.text = "Start!";

        yield return new WaitForSeconds(0.75f);

        roundStateText.text = "";

        roundStarted = true;
    }

    private IEnumerator HandlePlayerDeath(bool player1Lose, bool player2Lose)
    {
        if (player1Lose)
        {
			roundCounters2[player2WinCount].SetActive(true);
            player2WinCount++;
        }
        if (player2Lose)
        {
			roundCounters1[player1WinCount].SetActive(true);
            player1WinCount++;
        }

        roundStateText.text = "Round Set";

		player1.OffInput();
		player2.OffInput();

        yield return new WaitForSeconds(2);

        roundStateText.text = "";

        if (player1WinCount == 3 || player2WinCount == 3)
        {
			StartCoroutine(GameEnd());
			yield break;
        }

        StartCoroutine(NewRound());
    }

	private IEnumerator GameEnd()
	{
		if (player1WinCount == player2WinCount)
		{
			roundStateText.text = "Draw";
		}
		else if (player1WinCount == 3)
		{
			roundStateText.text = "Player1 Win";
		}
		else if (player2WinCount == 3)
		{
			roundStateText.text = "Player2 Win";
		}

		yield return new WaitForSeconds(3);

		LoadSceneManager.LoadScene("Title");
		yield break;
	}

    private IEnumerator HandleTimesUp()
    {
        roundStateText.text = "Times Up!";

        yield return new WaitForSeconds(2);

        roundStateText.text = "";

        StartCoroutine(NewRound());
    }

    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            countTime = Mathf.Clamp(countTime - Time.deltaTime, 0, roundTime);
            timerText.text = countTime.ToString("F0");
            stream.SendNext(countTime);
        }
        else
        {
            countTime = (float)stream.ReceiveNext();
            print(countTime);
            timerText.text = countTime.ToString("F0");
            print(countTime);
        }
    }

    [PunRPC]
    void SendClonesValue(Fighter clone1, Fighter clone2)
    {
        player1 = clone1;
        player2 = clone2;
    }
}