using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public NetworkManager network;
    public GameObject createPanel;

    // Start is called before the first frame update
    void Start()
    {
        network.Connect();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickCreateRoom()
    {
        createPanel.SetActive(true);
    }

    public void OnClickOffCreateRoom()
    {
        createPanel.SetActive(false);
    }

    public void OnClickCreate()
    {
        network.CreateRoom();
        SceneManager.LoadScene("Game");
    }
}