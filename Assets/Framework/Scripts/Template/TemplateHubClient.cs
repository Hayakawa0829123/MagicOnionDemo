using System.Threading.Tasks;
using App.Shared.Hubs;
using App.Shared.MessagePack;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace App
{
    public class TemplateHubClient : MonoBehaviour, ITemplateReceiver, IStreamingHubClientDispose
    {
        private ITemplateHub client = null;
        
        /// <summary>
        /// 初期化
        /// </summary>
        public async UniTask Initialize(RoomSettings roomSetting, PlayerInfo playerInfo)
        {
            client = await ConnectAsync();
            var result = await client.Initialize(roomSetting, playerInfo);
            if (!result)
            {
                Debug.Log($"[{nameof(TemplateHubClient)}]クラスの初期化に失敗しました。");
            }
        }
        
        /// <summary>
        /// サーバーに接続
        /// </summary>
        /// <returns></returns>
        private async UniTask<ITemplateHub> ConnectAsync()
        {
            return await MagicOnionManager.Instance.ConnectStreamingHubAsync<ITemplateHub, ITemplateReceiver>(this);
        }
        
        public async Task DisposeHub()
        {
            if (client == null)
                return;
            await client.DisposeAsync();
        }

        void ITemplateReceiver.OnTemplateMethod(TemplateData data)
        {
        }
    }
}