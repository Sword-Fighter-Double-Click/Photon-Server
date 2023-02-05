using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleAdminOffline : MonoBehaviour
{
    public void btnClick()
	{
		SceneManager.LoadScene("FightOffline");
	}
}
