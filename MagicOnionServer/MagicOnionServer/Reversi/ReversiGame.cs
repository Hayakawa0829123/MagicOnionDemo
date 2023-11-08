using System.Linq;
using App.Shared;
using App.Shared.MessagePack;
using App.Shared.Reversi;

namespace App.Reversi
{
    public class ReversiGame : IGameInstance
    {
        private ReversiDefine.StoneColor[,] stones = new ReversiDefine.StoneColor[ReversiDefine.Width, ReversiDefine.Height];
        
        public ReversiDefine.StoneColor[,] Stones => stones;

        private PlayerInfo currentTurnPlayer = null;
        
        private PlayerInfo blackPlayer = null;
        private PlayerInfo whitePlayer = null;

        private bool forcedEndGame = false;

        public void Initialize(PlayerInfo[] players, Action endGameAction)
        {
            // プレイヤー情報を取得出来なければ終了させる
            try
            {
                // プレイヤーの色割り当て
                blackPlayer = players[0];
                whitePlayer = players[1];
                currentTurnPlayer = blackPlayer;
            }
            catch (Exception e)
            {
                forcedEndGame = true;
                Console.WriteLine(e);
                return;
            }
            
            // Noneで初期化
            for (short h = 0; h < ReversiDefine.Height; h++)
            {
                for (short w = 0; w < ReversiDefine.Width; w++)
                {
                    SetStone(new Point(w, h), ReversiDefine.StoneColor.None);
                }
            }

            // 初期配置
            SetStone(new Point(3, 3), ReversiDefine.StoneColor.Black, true);
            SetStone(new Point(3, 4), ReversiDefine.StoneColor.White, true);
            SetStone(new Point(4, 3), ReversiDefine.StoneColor.White, true);
            SetStone(new Point(4, 4), ReversiDefine.StoneColor.Black, true);
        }

        public ReversiDefine.StoneColor GetPlayerColor(PlayerInfo playerInfo)
        {
            if (blackPlayer.guid == playerInfo.guid)
            {
                return ReversiDefine.StoneColor.Black;
            }
            if (whitePlayer.guid == playerInfo.guid)
            {
                return ReversiDefine.StoneColor.White;
            }
            return ReversiDefine.StoneColor.None;
        }

        public PlayerInfo GetCurrentTurnPlayer()
        {
            return currentTurnPlayer;
        }
        
        private void NextTurn(int loopIndex = 0)
        {
            var nextPlayer = GetPlayerColor(currentTurnPlayer) == ReversiDefine.StoneColor.Black ? whitePlayer : blackPlayer;
            currentTurnPlayer = nextPlayer;
            
            if (loopIndex > 1)
            {
                // 両者配置出来なければゲーム終了
                forcedEndGame = true;
                return;
            }
            // 次のプレイヤーが石を配置出来る場所があるか確認
            var placeableStone = PlaceableStonePointsSearch(GetPlayerColor(nextPlayer)).Any();
            // 石を配置出来なかったらパス
            if (!placeableStone)
            {
                ++loopIndex;
                NextTurn(loopIndex);
            }
        }

        /// <summary>
        /// 置ける箇所を取得
        /// </summary>
        public Point[] PlaceableStonePointsSearch(ReversiDefine.StoneColor myColor)
        {
            if (myColor == ReversiDefine.StoneColor.None)
                return null;
            List<Point> noneCellPoints = new List<Point>();
            
            // 自分の石の周りにある何も配置していないPointを探索
            for (short h = 0; h < ReversiDefine.Height; h++)
            {
                for (short w = 0; w < ReversiDefine.Width; w++)
                {
                    var searchPoint = new Point(w, h);
                    var stoneColor = GetStoneColor(searchPoint);
                    if (stoneColor == ReversiDefine.StoneColor.None)
                        continue;
                    // 相手の色を探す
                    if (stoneColor == myColor)
                        continue;

                    // 全方向探索
                    foreach (var directionPoint in ReversiDefine.AllDirectionPoints)
                    {
                        var aroundPoint = searchPoint.Add(directionPoint);
                        var color = GetStoneColor(aroundPoint);
                        // 何も配置されていなければリストに追加
                        if (color == ReversiDefine.StoneColor.None)
                        {
                            noneCellPoints.Add(aroundPoint);
                        }
                    }
                }
            }

            // 未配置のPointから配置できる箇所を探索
            List<Point> canSetPoints = new List<Point>();
            foreach (var point in noneCellPoints)
            {
                var reversePoints = GetReversePoints(point, myColor);
                if (reversePoints.Any())
                {
                    canSetPoints.Add(point);
                }
            }

            return canSetPoints.ToArray();
        }
        
        /// <summary>
        /// 石をセットする。
        /// ひっくり返す石を返す
        /// </summary>
        public List<Point[]> SetStone(Point setPoint, ReversiDefine.StoneColor myColor, bool isForced = false)
        {
            if (IsOutsideCell(setPoint))
                return null;
            stones[setPoint.x, setPoint.y] = myColor;
            if (myColor == ReversiDefine.StoneColor.None || isForced)
                return null;

            var reverseLines = GetReversePoints(setPoint, myColor);
            foreach (var points in reverseLines)
            {
                foreach (var point in points)
                {
                    if (IsOutsideCell(point))
                        continue;
                    stones[point.x, point.y] = myColor;
                }
            }

            // 次のターンへ
            NextTurn();

            return reverseLines;
        }

        public bool IsGameEnd()
        {
            bool allPlaced = true;
            
            foreach (var stoneColor in stones)
            {
                // 未配置のマスがある
                if (stoneColor == ReversiDefine.StoneColor.None)
                {
                    allPlaced = false;
                    break;
                }
            }

            return allPlaced || forcedEndGame;
        }

        /// <summary>
        /// 指定のPointからひっくり返せるPointを取得
        /// </summary>
        private List<Point[]> GetReversePoints(Point setPoint, ReversiDefine.StoneColor myColor)
        {
            List<Point[]> reversePoint = new List<Point[]>();
            foreach (var directionPoint in ReversiDefine.AllDirectionPoints)
            {
                List<Point> searchPoints = new List<Point>();
                LineSearchStone(setPoint, directionPoint, myColor, ref searchPoints);
                // この方向には何も配置されていない
                if (!searchPoints.Any())
                    continue;
                var firstPoint = searchPoints.FirstOrDefault();
                // 一番近い石が自分と同じ色
                if (myColor == GetStoneColor(firstPoint))
                    continue;

                reversePoint.Add(searchPoints.ToArray());
            }
            return reversePoint;
        }
        
        /// <summary>
        /// 指定方向に探索する
        /// </summary>
        /// <param name="point"></param>
        /// <param name="searchDirection"></param>
        /// <param name="pointList"></param>
        private void LineSearchStone(Point point, Point searchDirection, ReversiDefine.StoneColor myColor, ref List<Point> pointList)
        {
            var nextPoint = point.Add(searchDirection);
            var nextColor = GetStoneColor(nextPoint);
            // 次の色がNoneだったら
            if (nextColor == ReversiDefine.StoneColor.None)
            {
                pointList.Clear();
                return;
            }
            // 次の色が自分の色だったら終了
            if (nextColor == myColor)
            {
                return;
            }
            pointList.Add(nextPoint);
            LineSearchStone(nextPoint, searchDirection, myColor, ref pointList);
        }

        private ReversiDefine.StoneColor GetStoneColor(Point point)
        {
            if (IsOutsideCell(point))
                return ReversiDefine.StoneColor.None;
            return stones[point.x, point.y];
        }

        private bool IsOutsideCell(Point point)
        {
            return !(point.x >= 0 && point.x < ReversiDefine.Width) ||
                   !(point.y >= 0 && point.y < ReversiDefine.Height);
        }

        public void GetStoneCount(out short _blackCount, out short _whiteCount)
        {
            short blackCount = 0;
            short whiteCount = 0;
            foreach (var stoneColor in stones)
            {
                switch (stoneColor)
                {
                    case ReversiDefine.StoneColor.Black:
                        blackCount++;
                        break;
                    case ReversiDefine.StoneColor.White:
                        whiteCount++;
                        break;
                }
            }
            _blackCount = blackCount;
            _whiteCount = whiteCount;
        }
        
        public void DisposeGame()
        {
            
        }
    }
}
