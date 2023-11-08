using App;
using App.Reversi;
using App.Shared;
using App.Shared.MessagePack;
using App.Shared.Reversi;
using MagicOnion;
using MagicOnion.Server;

public class CallReversiGameService : ServiceBase<ICallReversiGameService>, ICallReversiGameService
{
    public async UnaryResult<ReversiDefine.StoneColor> GetMyColorAsync(string roomId, PlayerInfo playerInfo)
    {
        var reversi = GameInstanceManager.GetInstance().GetGameInstance<ReversiGame>(roomId);
        return reversi.GetPlayerColor(playerInfo);
    }
    
    public async UnaryResult<ReversiDefine.StoneColor[,]> GetAllCellStones(string roomId)
    {
        var reversi = GameInstanceManager.GetInstance().GetGameInstance<ReversiGame>(roomId);
        return reversi.Stones;
    }
    
    public async UnaryResult<Point[]> PlaceableStonePoints(string roomId, PlayerInfo playerInfo)
    {
        var reversi = GameInstanceManager.GetInstance().GetGameInstance<ReversiGame>(roomId);
        var playerColor = reversi.GetPlayerColor(playerInfo);
        return reversi.PlaceableStonePointsSearch(playerColor);
    }
    
    public async UnaryResult<PlayerInfo> GetCurrentTurnPlayer(string roomId)
    {
        var reversi = GameInstanceManager.GetInstance().GetGameInstance<ReversiGame>(roomId);
        return reversi.GetCurrentTurnPlayer();
    }

    public async UnaryResult<bool> GetGameEnd(string roomId)
    {
        var reversi = GameInstanceManager.GetInstance().GetGameInstance<ReversiGame>(roomId);
        return reversi.IsGameEnd();
    }

    public async UnaryResult<(short, short)> GetStoneCount(string roomId)
    {
        var reversi = GameInstanceManager.GetInstance().GetGameInstance<ReversiGame>(roomId);
        reversi.GetStoneCount(out var black, out var white);
        return (black, white);
    }
}