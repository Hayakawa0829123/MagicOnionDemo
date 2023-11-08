using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerNameView : MonoBehaviour
{
    [SerializeField] private Toggle readyToggle = null;
    [SerializeField] private Text playerNameText = null;

    private void Start()
    {
        readyToggle.isOn = false;
    }

    public void Initialize(string name, bool isReady)
    {
        playerNameText.text = name;
        readyToggle.isOn = isReady;
    }
    
    public void SetReady(bool isReady)
    {
        readyToggle.isOn = isReady;
    }
}
