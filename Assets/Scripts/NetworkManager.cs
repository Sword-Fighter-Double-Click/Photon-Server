using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private readonly string gameVersion = "v1.0";
    private string userId;

    public TMP_InputField userIdText;
    public TMP_InputField roomNameText;
    private Dictionary<string, GameObject> roomDict = new Dictionary<string, GameObject>();
    public GameObject roomPrefab;
    public Transform scrollContent;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    
    private static class Names
    {
        public static string name;
    }

    private void Start()
    {
        print("Starting Photon Manager");

        PhotonNetwork.AutomaticallySyncScene = true;

        PhotonNetwork.GameVersion = gameVersion;

        PhotonNetwork.ConnectUsingSettings();
    }

    #region PHOTON_CALLBACKS
    public override void OnConnectedToMaster()
    {
        print("Connect to Server");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        print("Connect to Lobby");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        print("Failed Join Random Room");

        RoomOptions options = new RoomOptions();
        options.IsOpen = true;
        options.IsVisible = true;
        options.MaxPlayers = 2;
        CreateRandomValue();
        roomNameText.text = Names.name;
        
        PhotonNetwork.CreateRoom("room_1", options);
    }

    public override void OnCreatedRoom()
    {
        print("Room Created");
    }

    public override void OnJoinedRoom()
    {
        print("Room Joined");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        print("someone joined room");
        print(newPlayer.NickName);
        LoadSceneManager.LoadScene("InGame");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        print("room updated");
        GameObject tempRoom = null;
        foreach (var room in roomList)
        {
            if (room.RemovedFromList == true)
            {
                roomDict.TryGetValue(room.Name, out tempRoom);
                Destroy(tempRoom);
                roomDict.Remove(room.Name);
            }
            else
            {
                if (roomDict.ContainsKey(room.Name) == false)
                {
                    GameObject _room = Instantiate(roomPrefab, scrollContent);
                    _room.GetComponent<Roomdata>().RoomInfo = room;
                    roomDict.Add(room.Name, _room);
                }
                else
                {
                    roomDict.TryGetValue(room.Name, out tempRoom);
                    tempRoom.GetComponent<Roomdata>().RoomInfo = room;
                }
            }
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LeaveRoom();
            LoadSceneManager.LoadScene("Title");
        }
    }
    #endregion

    public string CreateRandomValue()
    {
        int num;
        int avg;
        string str;
        DateTime time = DateTime.Now;
        int totalTime = (int)(time.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        num = UnityEngine.Random.Range(1, 100);
        avg = totalTime * num;
        str = avg.ToString("X");
        Names.name = str.Substring(0, 5);
        return Names.name;
    }

    #region UI_BUTTON_CALLBACK
    public void OnRandomBtn()
    {
        if (string.IsNullOrEmpty(userIdText.text))
        {
            CreateRandomValue();
            userIdText.text = Names.name;
        }

        PlayerPrefs.SetString("USER_ID", userIdText.text);
        PhotonNetwork.NickName = userIdText.text;
        PhotonNetwork.JoinRandomRoom();
        LoadSceneManager.LoadScene("InGame");
    }

    public void OnMakeRoomClick()
    {
        RoomOptions options = new RoomOptions();
        options.IsOpen = true;
        options.IsVisible = true;
        options.MaxPlayers = 2;

        if (string.IsNullOrEmpty(roomNameText.text))
        {                                                                                       
            CreateRandomValue();
            roomNameText.text = Names.name;
        }

        PhotonNetwork.CreateRoom(roomNameText.text, options, null);
    }
    #endregion
}