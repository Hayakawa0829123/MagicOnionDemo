using App.Shared;
using MagicOnion;
using MagicOnion.Server;

public class TemplateService : ServiceBase<ITemplateService>, ITemplateService
{
    public async UnaryResult<int> TemplateAsync(int i)
    {
        return 0;
    }
}