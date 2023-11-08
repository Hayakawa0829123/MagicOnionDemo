using App.Shared.MessagePack;
using App.Shared.Reversi;
using MagicOnion;

namespace App.Shared
{
    public interface ICallReversiGameService : IService<ICallReversiGameService>
    {
        /// <summary>
        /// プレイヤーの石色を取得
        /// </summary>
        UnaryResult<ReversiDefine.StoneColor> GetMyColorAsync(string roomId, PlayerInfo playerInfo);

        /// <summary>
        /// サーバー内の石配置状態を全て取得
        /// </summary>
        UnaryResult<ReversiDefine.StoneColor[,]> GetAllCellStones(string roomId);

        /// <summary>
        /// 石を配置出来るマスを取得
        /// </summary>
        UnaryResult<Point[]> PlaceableStonePoints(string roomId, PlayerInfo playerInfo);

        /// <summary>
        /// 現在のターンのプレイヤーを取得
        /// </summary>
        UnaryResult<PlayerInfo> GetCurrentTurnPlayer(string roomId);

        /// <summary>
        /// ゲームが終了したか取得
        /// </summary>
        UnaryResult<bool> GetGameEnd(string roomId);

        /// <summary>
        /// 色ごとの石の数を取得
        /// </summary>
        UnaryResult<(short, short)> GetStoneCount(string roomId);
    }
}