using System.Collections.Generic;
using System.Linq;
using App.Shared;
using App.Shared.Reversi;
using App.Shared.MessagePack;
using App.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;

namespace App.Reversi
{
    public class ReversiGamePresenter : MonoBehaviour
    {
        [SerializeField] private BoardCellController boardCellPrefab = null;
        [SerializeField] private Transform bordParent = null;

        private RoomHubClient room = null;

        // StreamingHub(リアルタイム通信)呼び出し用
        private ReversiHubClient reversiHubClient = null;

        // Service（API）呼び出し用
        private ICallReversiGameService callReversiGameService;

        // 自分の色
        private ReversiDefine.StoneColor myColor;

        // ボード情報
        private BoardCellController[,] boardCells = new BoardCellController[ReversiDefine.Width, ReversiDefine.Height];

        // ひっくり返す石
        private List<Point[]> reversePoints = null;

        private bool myTurn = false;

        private async void Start()
        {
            // 現在入室しているルームを取得
            room = MagicOnionManager.Instance.GetStreamingHubClient<RoomHubClient>();

            // Service生成
            callReversiGameService = MagicOnionManager.Instance.CreateService<ICallReversiGameService>();
            
            // StreamingHub生成
            reversiHubClient = MagicOnionManager.Instance.CreateStreamingHubClient<ReversiHubClient>();
            await reversiHubClient.Initialize(room.RoomSetting, room.SelfPlayerInfo);

            // 石を配置した際に呼ばれる（自分・相手両方）
            reversiHubClient.OnSetStone.Subscribe(setPoint =>
            {
                boardCells[setPoint.x, setPoint.y].SetColor(GetCurrentTurnColor());
            }).AddTo(gameObject);

            // 石を配置した後に呼ばれる（自分・相手両方）
            // ひっくり返される石を取得
            reversiHubClient.OnReceiverReversePoints.Subscribe(points =>
            {
                reversePoints = points;
            }).AddTo(gameObject);

            // リバーシボードの初期化
            await InitializeBoardAsync();

            // ゲームのシーケンス開始
            GameLoopSequenceAsync().Forget();
        }

        /// <summary>
        /// ボード初期化
        /// </summary>
        private async UniTask InitializeBoardAsync()
        {
            for (short h = 0; h < ReversiDefine.Height; h++)
            {
                for (short w = 0; w < ReversiDefine.Width; w++)
                {
                    var spawnPos = new Vector3(w, 0, h);
                    var cell = Instantiate(boardCellPrefab, spawnPos, Quaternion.identity, parent: bordParent);
                    // 配置マスに位置を表すPointをセット
                    cell.Initialize(new Point(w, h),
                        point =>
                        {
                            SetClickEnable(false);
                            // 石を配置
                            SetStoneAsync(point).Forget();
                        });
                    boardCells[w, h] = cell;
                }
            }

            // サーバーで決定した自分の色を取得
            myColor = await callReversiGameService.GetMyColorAsync(room.RoomSetting.roomId, room.SelfPlayerInfo);
            LoggerController.Instance.AddGameLog($"あなたの色は「{myColor.ToString()}」です。");
        }

        /// <summary>
        /// サーバーが保持しているボードの状態をそのまま反映する
        /// </summary>
        private async UniTask RefreshBoardAsync()
        {
            // ボード上石を全て取得
            var stones = await callReversiGameService.GetAllCellStones(room.RoomSetting.roomId);

            for (short h = 0; h < ReversiDefine.Height; h++)
            {
                for (short w = 0; w < ReversiDefine.Width; w++)
                {
                    boardCells[w, h].SetColor(stones[w, h]);
                }
            }
        }

        /// <summary>
        /// 指定の位置に自分の色の石を配置
        /// </summary>
        /// <param name="point"></param>
        private async UniTask SetStoneAsync(Point point)
        {
            // 石の配置した位置をサーバーに通知
            await reversiHubClient.SetStone(point);
        }

        /// <summary>
        /// 石をひっくり返す
        /// </summary>
        private async UniTask ReverseStonesAsync(ReversiDefine.StoneColor reverseColor, List<Point[]> reverseLines)
        {
            var reversePoints = reverseLines.SelectMany(x => x).ToList();
            foreach (var points in reverseLines)
            {
                foreach (var p in points)
                {
                    boardCells[p.x, p.y].OnReverse(reverseColor, () => { reversePoints.Remove(p); });
                    await UniTask.DelayFrame(10);
                }
            }

            // 全てひっくり返し終わったら次へ
            await UniTask.WaitWhile(() => reversePoints.Any());
        }

        /// <summary>
        /// 指定のマスをハイライトする
        /// </summary>
        private void SetPlaceableCell(Point[] points)
        {
            foreach (var boardCell in boardCells)
            {
                boardCell.SetStonePlaceableCell(points.Any(x => boardCell.CellPoint.Match(x)));
            }
        }

        /// <summary>
        /// クリックできるか設定
        /// </summary>
        private void SetClickEnable(bool isEnable)
        {
            foreach (var boardCell in boardCells)
            {
                boardCell.SetClickEnable(isEnable);
            }
        }

        /// <summary>
        /// 現在のターンに配置する石の色を取得
        /// </summary>
        /// <returns></returns>
        private ReversiDefine.StoneColor GetCurrentTurnColor()
        {
            if (myTurn)
            {
                return myColor;
            }

            return myColor == ReversiDefine.StoneColor.Black
                ? ReversiDefine.StoneColor.White
                : ReversiDefine.StoneColor.Black;
        }

        /// <summary>
        /// ゲーム進行
        /// </summary>
        private async UniTask GameLoopSequenceAsync()
        {
            await RefreshBoardAsync();

            SetClickEnable(false);
            var roomId = room.RoomSetting.roomId;

            // 現在のターンが誰か取得
            var currentTurnPlayer = await callReversiGameService.GetCurrentTurnPlayer(roomId);
            myTurn = currentTurnPlayer.guid == room.SelfPlayerInfo.guid;

            if (myTurn)
            {
                LoggerController.Instance.AddGameLog("あなたのターンです。");
            }
            else
            {
                LoggerController.Instance.AddGameLog("相手のターンです。");
            }

            // 配置可能なマスを取得
            var placeableCells = await callReversiGameService.PlaceableStonePoints(roomId, currentTurnPlayer);
            SetPlaceableCell(placeableCells);

            // 自分のターンであればクリックできる
            SetClickEnable(myTurn);

            // ひっくり返すポイントがOnReceiverReversePointsから返ってくるのを待つ
            await UniTask.WaitWhile(() => reversePoints == null);

            var reverseColor = GetCurrentTurnColor();
            // 指定の石を裏返す
            await ReverseStonesAsync(reverseColor, reversePoints);
            reversePoints = null;

            // ゲームが終了したか確認
            var isGameEnd = await callReversiGameService.GetGameEnd(roomId);
            if (isGameEnd)
            {
                GameEnd().Forget();
            }
            else
            {
                GameLoopSequenceAsync().Forget();
            }
        }

        /// <summary>
        /// ゲーム終了
        /// </summary>
        private async UniTask GameEnd()
        {
            var (blackCount, whiteCount) = await callReversiGameService.GetStoneCount(room.RoomSetting.roomId);
            if (blackCount > whiteCount)
            {
                LoggerController.Instance.AddGameLog("Blackの勝ち！！！");
            }
            else
            {
                LoggerController.Instance.AddGameLog("Whiteの勝ち！！！");
            }
        }
    }
}