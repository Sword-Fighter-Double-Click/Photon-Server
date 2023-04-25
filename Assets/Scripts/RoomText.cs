using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoomText : NetworkManager
{
    public TextMeshProUGUI roomCodeText;

    private void Awake()
    {
        SetRoomIDText();
    }

    public void SetRoomIDText()
    {
        CreateRandomValue();
        roomCodeText.text = roomName;
    }
}
