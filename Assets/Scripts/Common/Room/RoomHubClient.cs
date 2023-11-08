using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Shared.Hubs;
using App.Shared.MessagePack;
using App.UI;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace App
{
    public class RoomHubClient : MonoBehaviour, IRoomReceiver, IStreamingHubClientDispose
    {
        /// <summary>
        /// サーバーに実装したロジックを呼び出すためのインスタンス
        /// </summary>
        private IRoomHub client = null;
        
        /// <summary>
        /// 入室しているプレイヤーリスト
        /// </summary>
        private Dictionary<string, PlayerInfo> joinedRoomPlayers = new Dictionary<string, PlayerInfo>();
        public IReadOnlyDictionary<string, PlayerInfo> JoinedRoomPlayers => joinedRoomPlayers;

        /// <summary>
        /// プレイヤー情報
        /// </summary>
        private PlayerInfo selfPlayerInfo;
        public PlayerInfo SelfPlayerInfo => selfPlayerInfo;
        
        /// <summary>
        /// ルーム設定
        /// </summary>
        private RoomSettings roomSetting;
        public RoomSettings RoomSetting => roomSetting;

        /// <summary>
        /// 準備完了をしているか
        /// </summary>
        private Subject<PlayerInfo> onReady = new Subject<PlayerInfo>();
        public IObservable<PlayerInfo> OnReadyObservable => onReady.AsObservable();
        
        /// <summary>
        /// サーバーに接続
        /// </summary>
        /// <returns></returns>
        private async UniTask<IRoomHub> ConnectAsync()
        {
            return await MagicOnionManager.Instance.ConnectStreamingHubAsync<IRoomHub, IRoomReceiver>(this);
        }
        
        /// <summary>
        /// 新しくルームを作成
        /// </summary>
        public async UniTask<bool> CreateRoom(RoomSettings roomSetting)
        {
            try
            {
                if (client == null)
                {
                    client = await ConnectAsync();
                    if (client == default)
                        return false;
                }
                this.roomSetting = roomSetting;
                var result = await client.CreateRoomAsync(selfPlayerInfo, roomSetting);
                if (result)
                {
                    // 作成成功
                    LoggerController.Instance.AddSystemLog("ルームを作成しました");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            client = null;
            LoggerController.Instance.AddSystemLog("参加に失敗しました。");
            return false;
        }
        
        /// <summary>
        /// 既存の部屋に入室
        /// </summary>
        public async UniTask<bool> JoinRoom(string roomId)
        {
            try
            {
                if (client == null)
                {
                    client = await ConnectAsync();
                    if (client == default)
                        return false;
                }
                var result = await client.JoinRoomAsync(roomId, selfPlayerInfo);
                if (result)
                {
                    // 入室完了
                    LoggerController.Instance.AddSystemLog("入室成功");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            client = null;
            LoggerController.Instance.AddSystemLog("入室失敗");
            return false;
        }

        /// <summary>
        /// ゲーム開始（ホストが呼ぶ）
        /// </summary>
        public async UniTask HostToStartGame()
        {
            await client.StartGameAsync();
        }

        /// <summary>
        /// 準備完了
        /// </summary>
        public async UniTask SetReady(bool isReady)
        {
            await client.SetReadyAsync(isReady);
        }
        
        /// <summary>
        /// ルームから退出
        /// </summary>
        private async UniTask LeaveRoom()
        {
            if (client == null)
                return;
            await client.LeaveRoomAsync();
            await client.DisposeAsync();
            await client.WaitForDisconnect();
        }
        
        /// <summary>
        /// プレイヤー名を設定
        /// </summary>
        public void SetSelfPlayerName(string name)
        {
            selfPlayerInfo = new PlayerInfo();
            selfPlayerInfo.name = name;
        }

        /// <summary>
        /// 退出処理（手動で呼ばなければMagicOnionManagerが自動で呼ぶ）
        /// </summary>
        public async Task DisposeHub()
        {
            await LeaveRoom();
        }
        
        /// <summary>
        /// 全ての通信を切断してルームから退出する
        /// </summary>
        public static async UniTask ForcedLeave(Action onCompleted)
        {
            LoggerController.Instance.AddSystemLog("退出開始");
            await MagicOnionManager.Instance.AllStreamingHubClientDispose();
            LoggerController.Instance.AddSystemLog("退出完了");
            onCompleted?.Invoke();
        }
        
        void IRoomReceiver.OnJoinSelfCompleted(PlayerInfo[] otherPlayerInfos, PlayerInfo updatedSelfInfo, RoomSettings roomSetting)
        {
            joinedRoomPlayers = otherPlayerInfos.ToDictionary(x => x.guid);
            // 入室した際にGUIDが付与されるのでここで更新
            selfPlayerInfo = updatedSelfInfo;
            this.roomSetting = roomSetting;
            
            LoggerController.Instance.AddSystemLog("入室しました。");
        }
        
        void IRoomReceiver.OnOtherPlayerJoin(PlayerInfo otherRoomPlayerInfo)
        {
            if (joinedRoomPlayers.ContainsKey(otherRoomPlayerInfo.guid))
                return;
            LoggerController.Instance.AddSystemLog($"{otherRoomPlayerInfo.name}さんが入室しました。");
            joinedRoomPlayers.Add(otherRoomPlayerInfo.guid, otherRoomPlayerInfo);
        }
        
        void IRoomReceiver.OnLeave(PlayerInfo leaveRoomPlayerInfo)
        {
            if (!joinedRoomPlayers.ContainsKey(leaveRoomPlayerInfo.guid))
                return;
            LoggerController.Instance.AddSystemLog($"{leaveRoomPlayerInfo.name}さんが退出しました。");
            joinedRoomPlayers.Remove(leaveRoomPlayerInfo.guid);
        }

        void IRoomReceiver.OnReadyPlayer(PlayerInfo playerInfo)
        {
            if (!joinedRoomPlayers.ContainsKey(playerInfo.guid))
                return;
            joinedRoomPlayers[playerInfo.guid].isReady = playerInfo.isReady;
            onReady?.OnNext(playerInfo);
        }
        
        void IRoomReceiver.OnChangeHost(PlayerInfo newHostPlayer)
        {
            if (!joinedRoomPlayers.ContainsKey(newHostPlayer.guid))
                return;
            joinedRoomPlayers[newHostPlayer.guid].isHost = true;
            LoggerController.Instance.AddSystemLog($"ホストが{newHostPlayer.name}さんに変わりました。");
        }

        void IRoomReceiver.OnStartGame()
        {
            LoggerController.Instance.AddSystemLog($"ホストがゲームを開始しました");
            SceneManager.Instance.ChangeScene(SceneManager.SceneState.Game).Forget();
        }
    }
}
