using MagicOnion.Server.Hubs;
using App.Shared.Hubs;
using App.Shared.MessagePack;
using App.Reversi;
using UniRx;

namespace App.Room
{
    /// <summary>
    /// このクラスは接続ごとにサーバー内でインスタンス化される
    /// </summary>
    public class RoomHub : StreamingHubBase<IRoomHub, IRoomReceiver> , IRoomHub
    {
        private IGroup room;
        private string roomId;
        
        private IInMemoryStorage<PlayerInfo> playerStorage;
        private PlayerInfo selfInfo;

        private Subject<Unit> leaveRoomSubject = new Subject<Unit>();
        public UniRx.IObservable<Unit> OnLeaveRoom => leaveRoomSubject.AsObservable();

        public PlayerInfo PlayerInfo => selfInfo;
        public IGroup RepositoryGroup => room;
        public string RoomId => roomId;
        
        /// <summary>
        /// ルームを作成
        /// </summary>
        public async Task<bool> CreateRoomAsync(PlayerInfo selfInfo, RoomSettings roomSettings)
        {
            var success = false;
            
            // 接続ごとのGUIDを取得
            selfInfo.guid = Context.ContextId.ToString();
            
            // ここで追加した情報（今回の場合PlayerInfo）は同じルームのユーザーにも共有される。（IInMemoryStorage<PlayerInfo>として取得）
            (success, room, playerStorage) = await Group.TryAddAsync(roomSettings.roomId, 2, true, selfInfo);
            if (success)
            {
                selfInfo.isHost = true;
                roomId = roomSettings.roomId;
                this.selfInfo = selfInfo;
                
                // Roomを登録
                RoomManager.GetInstance().RegisterRoom(this, roomSettings);
                
                // Server -> Client(自分のみ)
                BroadcastToSelf(room).OnJoinSelfCompleted(playerStorage.AllValues.ToArray(), selfInfo, roomSettings);
            }

            return success;
        }
        
        /// <summary>
        /// ルーム入室
        /// </summary>
        public async Task<bool> JoinRoomAsync(string roomId, PlayerInfo selfInfo)
        {
            selfInfo.guid = Context.ContextId.ToString();
            
            // 指定したroomId(group)の部屋が立っていたら自身の情報を追加する。
            // 返り値として現在そのルームに保存されている値（今回の場合はPlayerInfo）を取得する。
            // IInMemoryStorage<T>で受け取る必要がある。
            var success = false;
            (success, room, playerStorage) = await Group.TryAddAsync(roomId, 2, false, selfInfo);

            if (!success)
            {
                return false;
            }
            
            this.selfInfo = selfInfo;
            this.roomId = roomId;
            // Roomを登録
            RoomManager.GetInstance().AddRoomHub(this);

            if (RoomManager.GetInstance().TryGetRoomSetting(roomId, out var roomSetting))
            {
                // Server -> Client(自分のみ)
                BroadcastToSelf(room).OnJoinSelfCompleted(playerStorage.AllValues.ToArray(), selfInfo, roomSetting);
                // Server -> Client(自分以外)
                BroadcastExceptSelf(room).OnOtherPlayerJoin(selfInfo);
                return true;
            }
            return false;
        }

        /// <summary>
        /// ルーム退出
        /// 自身の情報やインスタンスをサーバーから解放
        /// </summary>
        public async Task LeaveRoomAsync()
        {
            // // ホストが退出した場合他の人にホストを渡す
            if (selfInfo.isHost && playerStorage.AllValues.Count > 1)
            {
                var newHostPlayer = playerStorage.AllValues.Where(x => !x.isHost).FirstOrDefault();
                BroadcastExceptSelf(room).OnChangeHost(newHostPlayer);
            }
            BroadcastExceptSelf(room).OnLeave(selfInfo);
            leaveRoomSubject.OnNext(Unit.Default);
            await RemoveAsync();
        }

        public async Task DisposeAsync()
        {
            await RemoveAsync();
        }
        
        /// <summary>
        /// 通信を破棄
        /// </summary>
        private async Task RemoveAsync()
        {
            await room.RemoveAsync(Context);
        }

        /// <summary>
        /// ゲームの準備が完了
        /// </summary>
        public async Task SetReadyAsync(bool isReady)
        {
            selfInfo.isReady = isReady;
            Broadcast(room).OnReadyPlayer(selfInfo);
        }

        /// <summary>
        /// ゲーム開始
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartGameAsync()
        {
            if (playerStorage.AllValues.Count == 2)
            {
                Console.WriteLine("StartGame");
                GameInstanceManager.GetInstance().GenerateGame<ReversiGame>(room.GroupName, playerStorage.AllValues.ToArray());
                Broadcast(room).OnStartGame();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 切断時に、自動的にこの接続がグループから削除された場合。
        /// </summary>
        protected override async ValueTask OnDisconnected()
        {
            await LeaveRoomAsync();
            await CompletedTask;
        }
    }
}
