using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SyncronizeHPFP : MonoBehaviour
{
	[SerializeField] private string targetFighterName;
	private GameObject targetFighter = null;
	private Image HPBar, FPBar;
    // Start is called before the first frame update
    void Start()
    {
		if (!string.IsNullOrEmpty(targetFighterName))
		{
			targetFighter = GameObject.Find(targetFighterName);
		}
    }

    // Update is called once per frame
    void Update()
    {
		if (CheckInternet.gamePlayMode)
		{
			if (targetFighter.name.Equals("spiro"))
			{
				
			}
			else
			{
				
			}
		}
		else
		{
			if (targetFighter.name.Equals("spiro"))
			{

			}
			else
			{
				float hp = targetFighter.GetComponent<ArkSha>().HP;
				float fp = targetFighter.GetComponent<ArkSha>().FP;
				HPBar.fillAmount = hp;
				FPBar.fillAmount = fp;
			}
		}
    }
}
