using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using App;
using App.Shared.MessagePack;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 待機メニュー
/// </summary>
public class WaitMenuView : MonoBehaviour
{
    [SerializeField] private LobbyPlayerNameView playerNamePrefab = null;
    [SerializeField] private Transform playerNameArea = null;

    [SerializeField] private Toggle readyButton = null;
    [SerializeField] private Button playButton = null;

    private Dictionary<string, LobbyPlayerNameView> playerNameViews = new Dictionary<string, LobbyPlayerNameView>();
    private RoomHubClient room = null;

    private IDisposable joinPlayerObservableDisposable = null;
    private IDisposable onReadyObservableDisposable = null;

    private bool isNeedInitButton = true;
    
    public void Initialize(RoomHubClient room)
    {
        this.room = room;

        InitView();

        InitObservable();
        
        if (isNeedInitButton)
        {
            // roomを再生成する場合があるため、2回以上初期化したくないものはこちらで処理する
            InitButtonListener();
            isNeedInitButton = false;
        }
    }

    private void InitView()
    {
        playButton.interactable = false;
        
        // ルーム内のプレイヤー名を表示
        foreach (var readyPlayer in room.JoinedRoomPlayers.Values)
        {
            SetView(readyPlayer);
        }
    }
    
    private void InitObservable()
    {
        // 入室・退出時に呼ばれる
        joinPlayerObservableDisposable = room.JoinedRoomPlayers.ObserveEveryValueChanged(x => x.Count).Subscribe(playerInfos =>
        {
            AllRemovePlayerName();
            foreach (var player in room.JoinedRoomPlayers.Values)
            {
                SetView(player);
            }
        }).AddTo(this.gameObject);

        // Ready状態を反映
        onReadyObservableDisposable = room.OnReadyObservable.Subscribe(playerInfo =>
        {
            SetReadyView(playerInfo);
            // 全員準備完了になったか
            playButton.interactable = room.JoinedRoomPlayers.Values.All(x => x.isReady);
        }).AddTo(this.gameObject);
    }
    
    private void InitButtonListener()
    {
        readyButton.onValueChanged.AddListener(isOn =>
        {
            room.SetReady(isOn).Forget();
        });
        
        playButton.onClick.AddListener(() =>
        {
            var roomHub = MagicOnionManager.Instance.GetStreamingHubClient<RoomHubClient>();
            roomHub.HostToStartGame().Forget();
        });
    }

    /// <summary>
    /// 準備状態を表示に反映
    /// </summary>
    /// <param name="playerInfo"></param>
    private void SetReadyView(PlayerInfo playerInfo)
    {
        playerNameViews[playerInfo.guid].SetReady(playerInfo.isReady);
    }
    
    /// <summary>
    /// 名前表示を追加
    /// </summary>
    /// <param name="playerInfo"></param>
    private void SetView(PlayerInfo playerInfo)
    {
        if (playerNameViews.ContainsKey(playerInfo.guid))
            return;
        var nameText = Instantiate(playerNamePrefab, parent: playerNameArea);
        nameText.Initialize(playerInfo.name, playerInfo.isReady);
        playerNameViews.Add(playerInfo.guid, nameText);
        
        if (playerInfo.guid == room.SelfPlayerInfo.guid)
        {
            playButton.gameObject.SetActive(playerInfo.isHost);
        }
    }
    
    /// <summary>
    /// 名前表示を全て破棄
    /// </summary>
    private void AllRemovePlayerName()
    {
        foreach (var text in playerNameViews.Values)
        {
            Destroy(text.gameObject);
        }
        playerNameViews.Clear();
    }

    private void OnDisable()
    {
        if (joinPlayerObservableDisposable != null)
        {
            joinPlayerObservableDisposable.Dispose();
            joinPlayerObservableDisposable = null;
        }

        if (onReadyObservableDisposable != null)
        {
            onReadyObservableDisposable.Dispose();
            onReadyObservableDisposable = null;
        }
        AllRemovePlayerName();
        room = null;
    }
}
