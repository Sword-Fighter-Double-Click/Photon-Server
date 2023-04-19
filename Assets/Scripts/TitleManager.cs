using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public void LocalGamePlay()
    {
        LoadSceneManager.LoadScene("InGame");
    }
}