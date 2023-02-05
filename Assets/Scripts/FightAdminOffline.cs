using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FightAdminOffline : MonoBehaviour
{
	private GameObject player1, player2;

	[SerializeField] private TextMeshProUGUI text;
	[SerializeField] private GameObject count;
	[SerializeField] private GameObject timesUp;

	[SerializeField] private GameObject win1, win2;

	public int wincount1 = 0, wincount2 = 0;

	public int time;
	private float currentTime;

	public bool isStart = false;

    // Start is called before the first frame update
    void Start()
    {
		player1 = GameObject.Find("SpeeroOffline");
		player2 = GameObject.Find("ArkShaOffline");
		//CallC();
		StartCoroutine(ReBuild());
	}

    // Update is called once per frame
    void Update()
    {
		if (!isStart) return;

		float a = Time.time - currentTime;
		if (a < 0) a = 0;
		text.text = a.ToString("F0");
    }
	
	private void CallC()
	{
		StartCoroutine(ReBuild());
	}

	private IEnumerator ReBuild()
	{
		//Time.timeScale = 0;
		player1.SetActive(true);
		player2.SetActive(true);
		timesUp.SetActive(false);

		player1.GetComponent<Speero>().HP = 100;
		player1.GetComponent<Speero>().FP = 0;
		player2.GetComponent<ArkSha>().HP = 100;
		player2.GetComponent<ArkSha>().FP = 0;
		GameObject.Find("1").transform.Find("HPBar").GetComponent<Image>().fillAmount = 1;
		GameObject.Find("1").transform.Find("STBar").GetComponent<Image>().fillAmount = 1;
		GameObject.Find("2").transform.Find("HPBar").GetComponent<Image>().fillAmount = 1;
		GameObject.Find("2").transform.Find("STBar").GetComponent<Image>().fillAmount = 1;

		time = 60;
		
		float countTime = 0f;
		int temp = 1;
		while (countTime <= 4f)
		{
			countTime += Time.deltaTime;
			//print(countTime);
			if (countTime > temp)
			{
				for (int i = 0; i < count.transform.childCount; i++)
				{
					count.transform.GetChild(i).gameObject.SetActive((i + 1) == temp);
				}
				temp++;
			}
			player1.transform.position = new Vector2(-6, 1.44f);
			player2.transform.position = new Vector2(6, 0);
			yield return null;
		}
		yield return new WaitForSeconds(1);
		count.transform.GetChild(3).gameObject.SetActive(false);

		currentTime = Time.time;

		isStart = true;
		//Time.timeScale = 1;

		StartC();
	}

	private void StartC()
	{
		StartCoroutine(IsPlayerDie());
	}

	IEnumerator IsPlayerDie()
	{
		yield return new WaitForSeconds(1);
		while (true)
		{
			if (!player1.active)
			{
				win2.SetActive(true);
				wincount1++;
				break;
			}
			if (!player2.active)
			{
				win1.SetActive(true);
				wincount2++;
				break;
			}
			if(int.Parse(text.text) > 60)
			{
				timesUp.SetActive(false);
				break;
			}
			yield return null;
		}
		isStart = false;
		yield return new WaitForSeconds(2);
		win1.SetActive(false);
		win2.SetActive(false);

		if (wincount1 == 3 || wincount2 == 3)
		{
			SceneManager.LoadScene("TitleOffline");
			yield break;
		}

		CallC();
	}

}
