using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private RectTransform rectTransform;

    private void Start()
    {
        StartCoroutine(Job());
    }

    private IEnumerator Job()
    {
        float z = 0;

        while (true)
        {
            rectTransform.Rotate(0, 0, z);
            z = Time.deltaTime * speed;
            yield return null;
        }
    }
}
