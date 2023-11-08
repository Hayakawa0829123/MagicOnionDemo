using System.Threading.Tasks;
using MagicOnion;
using App.Shared.MessagePack;

namespace App.Shared.Hubs
{
    // Server -> Client definition
    public interface IRoomReceiver
    {
        /// <summary>
        /// 自身の入室が完了した
        /// </summary>
        public void OnJoinSelfCompleted(PlayerInfo[] joinedPlayers, PlayerInfo updatedSelfInfo, RoomSettings roomSettings);
        
        /// <summary>
        /// 他のユーザーが入室してきた
        /// </summary>
        public void OnOtherPlayerJoin(PlayerInfo otherPlayer);

        /// <summary>
        /// ユーザーが退出した
        /// </summary>
        public void OnLeave(PlayerInfo otherPlayer);

        /// <summary>
        /// ホストが変更された
        /// </summary>
        public void OnChangeHost(PlayerInfo newHostPlayer);

        /// <summary>
        /// ゲーム開始
        /// </summary>
        public void OnStartGame();
        
        /// <summary>
        /// ゲームの準備が完了
        /// </summary>
        public void OnReadyPlayer(PlayerInfo playerInfo);
    }
    
    // Client -> Server definition
    public interface IRoomHub : IStreamingHub<IRoomHub, IRoomReceiver>
    {
        public Task<bool> CreateRoomAsync(PlayerInfo selfInfo, RoomSettings roomSettings);
        
        public Task<bool> JoinRoomAsync(string roomId, PlayerInfo selfInfo);
        
        public Task LeaveRoomAsync();

        public Task SetReadyAsync(bool isReady);

        public Task<bool> StartGameAsync();
    }
}