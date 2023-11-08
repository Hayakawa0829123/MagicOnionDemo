
using App.Shared.MessagePack;

namespace App
{
    public class GameInstanceManager
    {
        private static GameInstanceManager gameInstanceManager = new GameInstanceManager();

        public static GameInstanceManager GetInstance()
        {
            return gameInstanceManager;
        }

        private Dictionary<string, IGameInstance> gameInstances = new Dictionary<string, IGameInstance>();

        /// <summary>
        /// ゲームのインスタンス生成
        /// </summary>
        public TGame GenerateGame<TGame>(string roomId, PlayerInfo[] players)
            where TGame : IGameInstance, new()
        {
            if (gameInstances.ContainsKey(roomId))
                return default;
            TGame game = new TGame();
            gameInstances.Add(roomId, game);
            
            Console.WriteLine($"Room {roomId} is create game!!!!!!!");
            
            game.Initialize(players, () =>
            {
                DisposeGame(roomId);
            });
            return game;
        }

        /// <summary>
        /// 指定ルームのゲームインスタンスを取得
        /// </summary>
        /// <param name="roomId"></param>
        /// <typeparam name="TGame"></typeparam>
        /// <returns></returns>
        public TGame GetGameInstance<TGame>(string roomId)
            where TGame : IGameInstance
        {
            if (!gameInstances.ContainsKey(roomId))
                return default;
            return (TGame)gameInstances[roomId];
        }

        /// <summary>
        /// ゲームのインスタンスを解放
        /// </summary>
        /// <param name="roomId"></param>
        public void DisposeGame(string roomId)
        {
            if (!gameInstances.ContainsKey(roomId))
                return;
            gameInstances[roomId].DisposeGame();
            gameInstances[roomId] = null;
            gameInstances.Remove(roomId);
        }
    }
}
