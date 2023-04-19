using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    public static string nextScene;

    private void Start()
    {
        StartCoroutine(Loading());
    }

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("Load");
    }

    private IEnumerator Loading()
    {
        yield return null;
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(nextScene);
        asyncOperation.allowSceneActivation = false;
        float timer = 0.0f;
        while (!asyncOperation.isDone)
        {
            yield return null;
            if (asyncOperation.progress < 0.9f)
            {
            }
            else
            {
                timer += Time.deltaTime;
                if (timer >= 1.0f)
                {
                    asyncOperation.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}
