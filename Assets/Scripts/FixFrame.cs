using UnityEngine;

public class FixFrame : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Job()
    {
        Application.targetFrameRate = 60;
    }
}