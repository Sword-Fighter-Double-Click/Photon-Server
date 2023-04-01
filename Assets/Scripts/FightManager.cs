using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FightManager : MonoBehaviour
{
	public static int player1CharacterNumber;
	public static int player2CharacterNumber;

	[Header("Caching")]
	[SerializeField] private GameObject speero;
	[SerializeField] private GameObject arksha;


	[SerializeField] private TextMeshProUGUI roundCount;
	[SerializeField] private Image countDown;
	[SerializeField] private Sprite[] countDownImages;
	[SerializeField] private GameObject timesUp;
	[SerializeField] private Image HPBar1, HPBar2, FPBar1, FPBar2;
	[SerializeField] private GameObject win1, win2;

	[Header("Value")]
	[SerializeField] private int roundTime = 60;
	
	private Fighter player1, player2;
	
	private int player1WinCount = 0, player2WinCount = 0;
	
	private float countRoundTime;

	private bool roundStart = false;

	private Coroutine handlePlayerDeath;

	private Coroutine handleTimesUp;

    void Start()
	{
		switch (player1CharacterNumber)
		{
			case 0:
				player1 = Instantiate(speero).GetComponent<Fighter>();
				break;
			case 1:
				player1 = Instantiate(arksha).GetComponent<Fighter>();
				break;
		}
		player1.fighterNumber = 0;

        switch (player2CharacterNumber)
        {
            case 0:
                player2 = Instantiate(speero).GetComponent<Fighter>();
                break;
            case 1:
                player2 = Instantiate(arksha).GetComponent<Fighter>();
                break;
        }
		player2.fighterNumber = 1;

		player1.enemyFighter = player2;
		player2.enemyFighter = player1;

        StartCoroutine(NewRound());
	}

	void Update()
	{
		if (!roundStart) return;

		countRoundTime = Mathf.Clamp(countRoundTime - Time.deltaTime, 0, float.MaxValue);
		roundCount.text = countRoundTime.ToString("F0");

		if (countRoundTime <= 0)
		{
			countRoundTime = 0;
			handleTimesUp ??= StartCoroutine(HandleTimesUp());
		}

		if (player1.HP < 0 || player2.HP < 0)
		{
			handlePlayerDeath ??= StartCoroutine(HandlePlayerDeath());
		}
	}

	private IEnumerator NewRound()
	{
		roundStart = false;

		timesUp.SetActive(false);
		player1.ResetState();
		player2.ResetState();
		player1.OffInput();
		player2.OffInput();

		HPBar1.fillAmount = 1;
		FPBar1.fillAmount = 1;
		HPBar2.fillAmount = 1;
		FPBar2.fillAmount = 1;

		countRoundTime = roundTime;

		player1.transform.position = new Vector2(-6, 0);
		player1.transform.eulerAngles = Vector3.zero;
		player2.transform.position = new Vector2(6, 0);
		player2.transform.eulerAngles = Vector3.up * 180;

		countDown.gameObject.SetActive(true);

		int count;
		for (count = countDownImages.Length - 1; count > 0; count--)
		{
			countDown.sprite = countDownImages[count];
			yield return new WaitForSeconds(1);
		}
		countDown.sprite = countDownImages[count];
		player1.OnInput();
		player2.OnInput();
		yield return new WaitForSeconds(1);
		countDown.gameObject.SetActive(false);

		handlePlayerDeath = null;

		roundStart = true;
	}

	private IEnumerator HandlePlayerDeath()
	{
		roundStart = false;

		bool player1Lose = player1.HP < 0;
		bool player2Lose = player2.HP < 0;

		if (player1Lose)
		{
			win2.SetActive(true);
			player2WinCount++;
		}
		else if (player2Lose)
		{
			win1.SetActive(true);
			player1WinCount++;
		}

		yield return new WaitForSeconds(2);
		win1.SetActive(false);
		win2.SetActive(false);

		if (player1WinCount == 3 || player2WinCount == 3)
		{
			SceneManager.LoadScene("Title");
			yield break;
		}

		StartCoroutine(NewRound());
	}

	private IEnumerator HandleTimesUp()
	{
		roundStart = false;

		timesUp.SetActive(true);

        yield return new WaitForSeconds(2);

		timesUp.SetActive(false);

        StartCoroutine(NewRound());
    }
}