using System.Diagnostics;
using App.Shared.MessagePack;
using MagicOnion.Server.Hubs;
using UniRx;

namespace App.Room
{
    public class Room
    {
        private RoomSettings roomSettings;
        private IGroup group;
        
        private Dictionary<string, RoomHub> roomInstanceDictionary = new Dictionary<string, RoomHub>();
        private Subject<string> onAllRemoveInstance = new Subject<string>();
        public UniRx.IObservable<string> OnAllRemoveInstance => onAllRemoveInstance.AsObservable();
        public RoomSettings RoomSettings => roomSettings;
        public IGroup RoomGroup => group;

        public Room(RoomHub roomHub, RoomSettings roomSettings)
        {
            this.roomSettings = roomSettings;
            group = roomHub.RepositoryGroup;
            AddRoomInstance(roomHub);
        }

        /// <summary>
        /// RoomHubを保管
        /// </summary>
        public void AddRoomInstance(RoomHub roomHub)
        {
            var guid = roomHub.PlayerInfo.guid;
            if (roomInstanceDictionary.ContainsKey(guid))
                return;
            roomHub.OnLeaveRoom.Subscribe(_ =>
            {
                RoomDispose(roomHub);
                Console.WriteLine($"{roomHub.PlayerInfo.name}が退出！！！！！");
            });
            roomInstanceDictionary.Add(roomHub.PlayerInfo.guid, roomHub);
        }

        /// <summary>
        /// ルームインスタンスが破棄された
        /// （退出したか何かしらの条件で自動で抜けた）
        /// </summary>
        /// <param name="roomHub"></param>
        private void RoomDispose(RoomHub roomHub)
        {
            var guid = roomHub.PlayerInfo.guid;
            if (!roomInstanceDictionary.ContainsKey(guid))
                return;
            roomInstanceDictionary[guid] = null;
            roomInstanceDictionary.Remove(guid);
            // 全てのルームインスタンスが破棄された
            if (!roomInstanceDictionary.Any())
            {
                OnAllRemove();
            }
        }

        /// <summary>
        /// 全てのインスタンスを強制的に破棄
        /// （全員強制退出させる）
        /// </summary>
        public async Task AllRoomDispose()
        {
            foreach (var roomHub in roomInstanceDictionary.Values)
            {
                await roomHub.DisposeAsync();
            }
            roomInstanceDictionary.Clear();
            // 全てのルームインスタンスが破棄された
            OnAllRemove();
        }

        /// <summary>
        /// 全てのインスタンスが破棄された
        /// </summary>
        private void OnAllRemove()
        {
            // その部屋が立てたゲームがあれば破棄する
            GameInstanceManager.GetInstance().DisposeGame(roomSettings.roomId);
            // 全てのルームインスタンスが破棄された(ルームにプレイヤーが不在)
            onAllRemoveInstance.OnNext(roomSettings.roomId);
        }
    }
    
    public class RoomManager
    {
        private static RoomManager roomManager = new RoomManager();

        public static RoomManager GetInstance()
        {
            return roomManager;
        }

        private Dictionary<string, Room> roomDictionary = new Dictionary<string, Room>();

        /// <summary>
        /// ルームを登録
        /// </summary>
        public void RegisterRoom(RoomHub roomHub, RoomSettings roomSettings)
        {
            var room = new Room(roomHub, roomSettings);
            room.OnAllRemoveInstance.Subscribe(roomId =>
            {
                roomDictionary.Remove(roomId);
            });
            roomDictionary.Add(roomSettings.roomId, room);
        }

        /// <summary>
        /// RoomHubのインスタンスを追加
        /// </summary>
        public void AddRoomHub(RoomHub roomHub)
        {
            roomDictionary[roomHub.RoomId].AddRoomInstance(roomHub);
        }
        
        public bool TryGetRoomSetting(string roomId, out RoomSettings roomSettings)
        {
            if (roomDictionary.ContainsKey(roomId))
            {
                roomSettings = roomDictionary[roomId].RoomSettings;
                return true;
            }
            roomSettings = null;
            return false;
        }
        
        public bool TryGetRoomGroup(string roomId, out IGroup group)
        {
            if (roomDictionary.ContainsKey(roomId))
            {
                group = roomDictionary[roomId].RoomGroup;
                return true;
            }
            group = null;
            return false;
        }
        
        public async Task RemoveRoom(string roomId)
        {
            await roomDictionary[roomId].AllRoomDispose();
        }
    }
}