using App.Room;
using MagicOnion.Server.Hubs;
using App.Shared.Hubs;
using App.Shared.MessagePack;

namespace App
{
    public class TemplateHub : StreamingHubBase<ITemplateHub, ITemplateReceiver>, ITemplateHub
    {
        private IGroup room;
        private PlayerInfo playerInfo;
        private IInMemoryStorage<TemplateData> storage;

        public async Task<bool> Initialize(RoomSettings roomSettings, PlayerInfo playerInfo)
        {
            var group = await Group.AddAsync(roomSettings.roomId);
            // この処理は基本固定
            if (group != null)
            {
                room = group;
                this.playerInfo = playerInfo;
                return true;
            }
            return false;
        }

        public async Task TemplateAsync(TemplateData data)
        {
            // Server -> Client
            Broadcast(room).OnTemplateMethod(data);
        }
    }
}