using System.Collections.Generic;
using System.Threading.Tasks;
using App.Shared.MessagePack;
using App.Shared.Reversi;
using MagicOnion;

namespace App.Shared.Hubs
{
    // Server -> Client definition
    public interface IReversiReceiver
    {
        void OnReversePoints(List<Point[]> points);

        void OnSetStone(Point point);
    }
    
    // Client -> Server definition
    public interface IReversiHub : IStreamingHub<IReversiHub, IReversiReceiver>
    {
        Task<bool> Initialize(RoomSettings roomSettings, PlayerInfo playerInfo);
        
        Task SetStoneAsync(Point setPoint);
    }
}