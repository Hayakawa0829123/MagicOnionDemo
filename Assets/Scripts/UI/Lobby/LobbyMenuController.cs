using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace App.UI
{
    public class LobbyMenuController : MonoBehaviour
    {
        [SerializeField, Header("Button")] private Button createMenuButton = null;
        [SerializeField] private Button joinMenuButton = null;
        [SerializeField] private Button backButton = null;
        [SerializeField] private Button leaveButton = null;

        [SerializeField, Header("View")] private CreateRoomMenuView createRoomMenuView = null;
        [SerializeField] private JoinRoomMenuView joinRoomMenuView = null;
        [SerializeField] private WaitMenuView waitMenuView = null;
        [SerializeField] private GameObject mainView = null;

        private enum ViewState
        {
            Main = 0,
            Join,
            Create,
            Ready
        }

        private void Start()
        {
            // Viewクラス初期化
            createRoomMenuView.Initialize(room =>
            {
                ChangeView(ViewState.Ready);
                waitMenuView.Initialize(room);
            });

            joinRoomMenuView.Initialize(room =>
            {
                ChangeView(ViewState.Ready);
                waitMenuView.Initialize(room);
            });

            // ボタンを初期化
            createMenuButton.onClick.AddListener(() => { ChangeView(ViewState.Create); });

            joinMenuButton.onClick.AddListener(() => { ChangeView(ViewState.Join); });

            backButton.onClick.AddListener(() => { ChangeView(ViewState.Main); });

            leaveButton.onClick.AddListener(() =>
            {
                RoomHubClient.ForcedLeave(() => { ChangeView(ViewState.Main); }).Forget();

                ChangeView(ViewState.Main);
            });

            ChangeView(ViewState.Main);
        }

        private void ChangeView(ViewState changeState)
        {
            foreach (var state in Enum.GetValues(typeof(ViewState)))
            {
                switch (state)
                {
                    case ViewState.Main:
                        mainView.SetActive((ViewState)state == changeState);
                        break;
                    case ViewState.Create:
                        createRoomMenuView.gameObject.SetActive((ViewState)state == changeState);
                        break;
                    case ViewState.Join:
                        joinRoomMenuView.gameObject.SetActive((ViewState)state == changeState);
                        break;
                    case ViewState.Ready:
                        waitMenuView.gameObject.SetActive((ViewState)state == changeState);
                        break;
                }
            }

            backButton.gameObject.SetActive(changeState != ViewState.Main && changeState != ViewState.Ready);
            leaveButton.gameObject.SetActive(changeState == ViewState.Ready);
        }
    }
}