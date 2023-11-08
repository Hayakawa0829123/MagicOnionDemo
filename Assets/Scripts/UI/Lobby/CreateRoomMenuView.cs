using System;
using App.Shared.MessagePack;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace App.UI
{
    public class CreateRoomMenuView : MonoBehaviour
    {
        [SerializeField] private Button createButton = null;
        [SerializeField] private InputField nameField = null;
        [SerializeField] private InputField roomNameField = null;

        private RoomHubClient roomHub = null;
        private Action<RoomHubClient> onCreateRoomAction = null;
        
        private bool creatingRoom = false;
        
        public void Initialize(Action<RoomHubClient> onCreateRoomAction)
        {
            this.onCreateRoomAction = onCreateRoomAction;
            
            createButton.onClick.AddListener(() =>
            {
                if (creatingRoom)
                    return;
                CreateRoom().Forget();
            });
        }
        
        private async UniTaskVoid CreateRoom()
        {
            creatingRoom = true;
            roomHub = MagicOnionManager.Instance.CreateStreamingHubClient<RoomHubClient>();
            if (string.IsNullOrEmpty(nameField.text) || string.IsNullOrEmpty(roomNameField.text))
            {
                LoggerController.Instance.AddSystemLog("作成失敗");
                creatingRoom = false;
                return;
            }
            roomHub.SetSelfPlayerName(nameField.text);
            var roomSetting = new RoomSettings(){ roomId = roomNameField.text };
            
            var result = await roomHub.CreateRoom(roomSetting);
            if (result)
            {
                onCreateRoomAction?.Invoke(roomHub);
            }
            else
            {
                // 失敗したインスタンスは破棄する
                // MEMO : 無理に使いまわすとフリーズする
                await MagicOnionManager.Instance.StreamingHubClientDispose(roomHub);
            }
            creatingRoom = false;
        }
    }
}
