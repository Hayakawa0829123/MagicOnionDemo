using MessagePack;

namespace App.Shared.MessagePack
{
    // for example, request object by MessagePack.
    [MessagePackObject()]
    public class PlayerInfo
    {
        [Key(0)] public string name;
        [Key(1)] public bool isHost;
        [Key(2)] public bool isReady;
        [Key(3)] public string guid;
    }
}
