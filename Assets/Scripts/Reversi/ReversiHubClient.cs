using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Shared.Hubs;
using App.Shared.MessagePack;
using App.Shared.Reversi;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace App
{
    public class ReversiHubClient : MonoBehaviour, IReversiReceiver, IStreamingHubClientDispose
    {
        private IReversiHub client = null;
        private Subject<List<Point[]>> onReceiverReversePoints = new Subject<List<Point[]>>();
        private Subject<Point> onSetStone = new Subject<Point>();
        
        public IObservable<List<Point[]>> OnReceiverReversePoints => onReceiverReversePoints.AsObservable();
        public IObservable<Point> OnSetStone => onSetStone.AsObservable();


        /// <summary>
        /// 初期化
        /// </summary>
        public async UniTask Initialize(RoomSettings roomSetting, PlayerInfo playerInfo)
        {
            client = await ConnectAsync();
            var result = await client.Initialize(roomSetting, playerInfo);
            if (!result)
            {
                Debug.LogError($"[{nameof(ReversiHubClient)}]クラスの初期化に失敗しました。");
            }
        }
        
        /// <summary>
        /// サーバーに接続
        /// </summary>
        /// <returns></returns>
        private async UniTask<IReversiHub> ConnectAsync()
        {
            return await MagicOnionManager.Instance.ConnectStreamingHubAsync<IReversiHub, IReversiReceiver>(this);
        }
        
        /// <summary>
        /// 石をセット
        /// </summary>
        public async UniTask SetStone(Point setPoint)
        {
            await client.SetStoneAsync(setPoint);
        }
        
        public async Task DisposeHub()
        {
            if (client == null)
                return;
            await client.DisposeAsync();
        }
        
        void IReversiReceiver.OnReversePoints(List<Point[]> points)
        {
            onReceiverReversePoints.OnNext(points);
        }

        void IReversiReceiver.OnSetStone(Point point)
        {
            onSetStone.OnNext(point);
        }
    }
}