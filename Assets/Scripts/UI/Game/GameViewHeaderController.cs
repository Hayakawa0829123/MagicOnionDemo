using System;
using System.Collections;
using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameViewHeaderController : MonoBehaviour
{
    [SerializeField] private Button leaveButton = null;

    private void Start()
    {
        leaveButton.onClick.AddListener(() =>
        {
            RoomHubClient.ForcedLeave( () =>
            {
                SceneManager.Instance.ChangeScene(SceneManager.SceneState.Lobby).Forget();
            }).Forget();
        });
    }
}
