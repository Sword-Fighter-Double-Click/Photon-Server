using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FightManager : MonoBehaviour
{
    public static int fighter1CharacterNumber;
    public static int fighter2CharacterNumber = 1;

    [Header("Cashing")]
    [SerializeField] private GameObject speero;
    [SerializeField] private GameObject arksha;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI roundStateText;
    [SerializeField] private Image HPBar1, HPBar2, FPBar1, FPBar2;

    [Header("Value")]
    [SerializeField] private int roundTime = 60;

    private Fighter player1, player2;

    private int player1WinCount = 0, player2WinCount = 0;

    private float countTime;

    private bool roundStarted = false;

    private void Start()
    {
        switch (fighter1CharacterNumber)
        {
            case 0:
                player1 = Instantiate(speero).GetComponent<Fighter>();
                break;
            case 1:
                player1 = Instantiate(arksha).GetComponent<Fighter>();
                break;
        }

        switch (fighter2CharacterNumber)
        {
            case 0:
                player2 = Instantiate(speero).GetComponent<Fighter>();
                break;
            case 1:
                player2 = Instantiate(arksha).GetComponent<Fighter>();
                break;
        }

        player1.tag = "Player1";
        player1.enemyFighter = player2;
        player1.SettingUI();

        player2.tag = "Player2";
        player2.enemyFighter = player1;
        player2.SettingUI();

        StartCoroutine(NewRound());
    }

    private void Update()
    {
        if (!roundStarted) return;

        countTime = Mathf.Clamp(countTime - Time.deltaTime, 0, roundTime);
        timerText.text = countTime.ToString("F0");

        if (countTime <= 0)
        {
            StartCoroutine(HandleTimesUp());
            roundStarted = false;
        }

        bool player1Lose = player1.currentHP <= 0;
        bool player2Lose = player2.currentHP <= 0;
        if (player1Lose || player2Lose)
        {
            StartCoroutine(HandlePlayerDeath(player1Lose, player2Lose));
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
        //FPBar1.fillAmount = 1;
        HPBar2.fillAmount = 1;
        //FPBar2.fillAmount = 1;

        countTime = roundTime;
        timerText.text = countTime.ToString("F0");

        player1.transform.position = new Vector3(-6, 0, 3);
        player1.transform.eulerAngles = Vector3.zero;
        player2.transform.position = new Vector3(6, 0, 3);
        player2.transform.eulerAngles = Vector3.up * 180;

        roundStateText.gameObject.SetActive(true);

        roundStateText.text = "Ready...";

        player1.OnInput();
        player2.OnInput();
        yield return new WaitForSeconds(1);
        roundStateText.gameObject.SetActive(false);

        roundStarted = true;
    }

    private IEnumerator HandlePlayerDeath(bool player1Lose, bool player2Lose)
    {
        if (player1Lose)
        {
            player2WinCount++;
        }
        if (player2Lose)
        {
            player1WinCount++;
        }

        roundStateText.text = "Round Set";

        yield return new WaitForSeconds(2);

        roundStateText.text = "";

        if (player1WinCount == 3 || player2WinCount == 3)
        {
            LoadSceneManager.LoadScene("Title");
            yield break;
        }

        StartCoroutine(NewRound());
    }

    private IEnumerator HandleTimesUp()
    {
        roundStateText.text = "Times Up!";

        yield return new WaitForSeconds(2);

        roundStateText.text = "";

        StartCoroutine(NewRound());
    }
}