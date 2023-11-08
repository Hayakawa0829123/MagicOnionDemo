using App;
using App.Reversi;
using App.Room;
using MagicOnion.Server.Hubs;
using App.Shared.Hubs;
using App.Shared.MessagePack;
using App.Shared.Reversi;

namespace App.Reversi
{
    public class ReversiHub : StreamingHubBase<IReversiHub, IReversiReceiver> , IReversiHub
    {
        private IGroup room;
        private string roomId;
        private PlayerInfo playerInfo;
        
        public async Task<bool> Initialize(RoomSettings roomSettings, PlayerInfo playerInfo)
        {
            var group = await Group.AddAsync(roomSettings.roomId);
            // この処理は基本固定
            if (group != null)
            {
                room = group;
                roomId = roomSettings.roomId;
                this.playerInfo = playerInfo;
                return true;
            }
            return false;
        }
        
        public async Task SetStoneAsync(Point setPoint)
        {
            Broadcast(room).OnSetStone(setPoint);
            var game = GameInstanceManager.GetInstance().GetGameInstance<ReversiGame>(roomId);
            var myColor = game.GetPlayerColor(playerInfo);
            var points = game.SetStone(setPoint, myColor);
            // Server -> Client
            Broadcast(room).OnReversePoints(points);
        }
    }
}
