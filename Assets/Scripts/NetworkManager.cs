using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI statusText;
    public TMP_InputField nickNameInput;
    public GameObject uiPanel;
    public byte userNum = 2;

    private bool connect = false;
    public string roomName;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    // Update is called once per frame
    void Update() 
    {
        statusText.text = PhotonNetwork.NetworkClientState.ToString();
    }

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() 
    {
        print("connect server");
        string nickName = PhotonNetwork.LocalPlayer.NickName;
        nickName = nickNameInput.text;
        print("your name is" + nickName);
        connect = true;
    }

    public void DisConnect()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        print("disconnected");
    }

    public void JoinRoom()
    {
        if(connect) 
        {
            PhotonNetwork.JoinRandomRoom();
            uiPanel.SetActive(false);
        }
    }

    public void CreateRoom()
    {

        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = userNum });
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(roomName, new RoomOptions {MaxPlayers = userNum});
    }

    public string CreateRandomValue()
    {
        int num;
        int avg;

        DateTime time = DateTime.Now;
        int totalTime = (int)(time.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        num = UnityEngine.Random.Range(1, 100);
        avg = totalTime * num;
        roomName = avg.ToString("X");
        return roomName;
    }
}