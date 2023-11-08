using System.Threading.Tasks;
using App.Shared.MessagePack;
using MagicOnion;
using MessagePack;

namespace App.Shared.Hubs
{
    // Server -> Client definition
    public interface ITemplateReceiver
    {
        void OnTemplateMethod(TemplateData data);
    }
    
    // Client -> Server definition
    public interface ITemplateHub : IStreamingHub<ITemplateHub, ITemplateReceiver>
    {
        Task<bool> Initialize(RoomSettings roomSettings, PlayerInfo playerInfo);
        
        Task TemplateAsync(TemplateData data);
    }

    // for example, request object by MessagePack.
    [MessagePackObject]
    public class TemplateData
    {
    }
}