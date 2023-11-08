
using App.Shared.MessagePack;

namespace App
{
    public interface IGameInstance
    {
        public void Initialize(PlayerInfo[] players, Action endGameAction);

        public void DisposeGame();
    }
}
