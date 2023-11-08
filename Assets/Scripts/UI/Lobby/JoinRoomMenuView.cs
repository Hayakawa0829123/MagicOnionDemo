using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace App.UI
{
    public class JoinRoomMenuView : MonoBehaviour
    {
        [SerializeField] private Button joinButton = null;
        [SerializeField] private InputField nameField = null;
        [SerializeField] private InputField roomNameField = null;

        private Action<RoomHubClient> onJoinRoomAction = null;
        private RoomHubClient roomHub = null;

        private bool enteringRoom = false;

        public void Initialize(Action<RoomHubClient> onJoinRoomAction)
        {
            this.onJoinRoomAction = onJoinRoomAction;

            joinButton.onClick.AddListener(() =>
            {
                if (enteringRoom)
                    return;
                JoinRoom().Forget();
            });
        }

        private async UniTaskVoid JoinRoom()
        {
            enteringRoom = true;
            if (string.IsNullOrEmpty(nameField.text) || string.IsNullOrEmpty(roomNameField.text))
            {
                LoggerController.Instance.AddSystemLog("作成失敗");
                enteringRoom = false;
                return;
            }
            // MEMO : ルームのHubはMagicOnionManagerに直接持たせてもいいかも
            roomHub = MagicOnionManager.Instance.CreateStreamingHubClient<RoomHubClient>();
            roomHub.SetSelfPlayerName(nameField.text);

            var result = await roomHub.JoinRoom(roomNameField.text);
            if (result)
            {
                onJoinRoomAction?.Invoke(roomHub);
            }
            else
            {
                // 失敗したインスタンスは破棄する
                // MEMO : 無理に使いまわすとフリーズする
                await MagicOnionManager.Instance.StreamingHubClientDispose(roomHub);
            }
            enteringRoom = false;
        }
    }
}