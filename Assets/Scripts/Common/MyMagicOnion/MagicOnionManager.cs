using System;
using System.Collections.Generic;
using System.Linq;
using MagicOnion;
using Grpc.Core;
using MagicOnion.Client;
using System.Threading.Tasks;
using App.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace App
{
    public class MagicOnionManager : SingletonMonoBehaviour<MagicOnionManager>
    {
        private const string Address = "localhost";
        
        private Grpc.Core.Channel channel = null;

        public Grpc.Core.Channel CurrentChannel => channel;
        
        public bool IsConnected => channel != null && channel.State != ChannelState.Shutdown;
        public ChannelState State => channel != null ? channel.State : ChannelState.Shutdown;
        

        private List<GameObject> streamingHubObjects = new List<GameObject>();
        
        protected override void doAwake()
        {
            // TODO : サーバーに配置した際はちゃんと設定する
            channel = new Grpc.Core.Channel(Address, 5000, ChannelCredentials.Insecure);
        }

        /// <summary>
        /// Service(API)を呼び出すインスタンスを生成
        /// </summary>
        public T CreateService<T>() where T : IService<T>
        {
            if (!IsConnected)
            {
                Debug.Log("サーバー未接続");
                return default;
            }
            return MagicOnionClient.Create<T>(channel);
        }

        /// <summary>
        /// StreamingHubのGameObjectを生成
        /// </summary>
        public TStreamingHubClient CreateStreamingHubClient<TStreamingHubClient>()
            where TStreamingHubClient : MonoBehaviour, IStreamingHubClientDispose
        {
            if (!IsConnected)
            {
                Debug.Log("サーバー未接続");
                return null;
            }
            // MEMO : StreamingHubは破棄に失敗するとフリーズしたりするので、GameObject単位で管理して簡単に破棄できるようにしている
            var hubObject = new GameObject(typeof(TStreamingHubClient).Name);
            hubObject.transform.parent = transform;
            streamingHubObjects.Add(hubObject);
            return hubObject.AddComponent<TStreamingHubClient>();
        }

        /// <summary>
        /// 指定のStreamingHubを取得
        /// </summary>
        public TStreamingHubClient GetStreamingHubClient<TStreamingHubClient>()
        {
            // TODO : クラス名以外でいい方法がないか考え中
            var clientObject = streamingHubObjects.FirstOrDefault(x => x.name == typeof(TStreamingHubClient).Name);
            if (clientObject == null)
                return default;
            return clientObject.GetComponent<TStreamingHubClient>();
        }
        
        /// <summary>
        /// StreamingHub(リアルタイム通信)を呼び出すインスタンスを生成
        /// </summary>
        public async Task<TStreamingHub> ConnectStreamingHubAsync<TStreamingHub, TReceiver>(TReceiver receiver)
            where TStreamingHub : IStreamingHub<TStreamingHub, TReceiver>
        {
            try
            {
                return await StreamingHubClient.ConnectAsync<TStreamingHub, TReceiver>(channel, receiver);
            }
            catch (Exception e)
            {
                LoggerController.Instance.AddSystemLog("サーバーに接続失敗。");
                throw;
            }
        }

        /// <summary>
        /// チャネルを破棄する
        /// </summary>
        private async UniTask Shutdown()
        {
            if (!IsConnected)
                return;
            await channel.ShutdownAsync();
            Debug.Log("切断完了");
        }

        /// <summary>
        /// 指定のStreamingHubを破棄する
        /// </summary>
        public async Task StreamingHubClientDispose(MonoBehaviour disposeClient)
        {
            streamingHubObjects.Remove(disposeClient.gameObject);
            var dispose = disposeClient.GetComponent<IStreamingHubClientDispose>();
            await dispose.DisposeHub();
            Destroy(disposeClient.gameObject);
        }

        /// <summary>
        /// StreamingHubClientを全て破棄
        /// </summary>
        public async Task AllStreamingHubClientDispose()
        {
            foreach (var hubObject in streamingHubObjects)
            {
                if (hubObject == null)
                    continue;
                var disposeClient = hubObject.GetComponent<IStreamingHubClientDispose>();
                await disposeClient.DisposeHub();
                DestroyImmediate(hubObject);
            }
            
            streamingHubObjects.Clear();
        }

        /// <summary>
        /// アプリを閉じた時に通信を破棄する
        /// </summary>
        private void OnApplicationQuit()
        {
            // MEMO : 一度止めないとフリーズして破棄しないと応答なしになる
            // 通信が破棄されるまで一旦キャンセルする
            Application.CancelQuit();
            DisposeHubAndShutdownChannel().Forget();
        }

        /// <summary>
        /// 通信関係を全て破棄
        /// </summary>
        private async UniTask DisposeHubAndShutdownChannel()
        {
            try
            {
                await AllStreamingHubClientDispose();
                await Shutdown();
            }
            catch (Exception e)
            {
                // MEMO : エラーが起きた際にもQuitを呼ばないと非同期で処理が止まってしまいタスクバーからでしか閉じれなくなる場合がある
                Application.Quit();
            }
            // 破棄されたら強制敵に終了
            Application.Quit();
        }
    }
}
