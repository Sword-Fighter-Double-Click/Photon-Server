using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Roomdata : MonoBehaviour
{
    private TMP_Text RoomInfoText;
    private RoomInfo _roonInfo;
    public NetworkManager network;
    public TMP_InputField userIdText;

    public RoomInfo RoomInfo
    {
        get
        {
            return _roonInfo;
        }

        set
        {
            _roonInfo = value;
            RoomInfoText.text = $"{_roonInfo.Name} ({_roonInfo.PlayerCount} / {_roonInfo.MaxPlayers})";
            GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => JoinRoom(_roonInfo.Name));
        }
    }

    public void Awake()
    {
        RoomInfoText = GetComponentInChildren<TMP_Text>();
    }

    public void JoinRoom(string roomName)
    {
       RoomOptions options = new RoomOptions();
       options.IsOpen = true;
       options.IsVisible = true;
       options.MaxPlayers = 2;

       if(string.IsNullOrEmpty(userIdText.text))
       {
            PhotonNetwork.NickName = network.CreateRandomValue();
       }
       else
       {
            PhotonNetwork.NickName = userIdText.text;
        }
       PhotonNetwork.JoinRoom(roomName);
    }
}
