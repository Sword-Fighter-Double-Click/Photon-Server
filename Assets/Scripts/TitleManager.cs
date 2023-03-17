using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void ConnectSetting()
	{
		Application.targetFrameRate = 60;
	}

    public void LocalGamePlay()
    {
        SceneManager.LoadScene("Fight");
    }
}