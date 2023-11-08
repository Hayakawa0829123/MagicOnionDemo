using MessagePack;

namespace App.Shared.MessagePack
{
    // for example, request object by MessagePack.
    [MessagePackObject()]
    public class RoomSettings
    {
        [Key(0)] public string roomId;
        [Key(1)] public int maxPlayer;
    }
}