using MagicOnion;

namespace App.Shared
{
    public interface ITemplateService : IService<ITemplateService>
    {
        UnaryResult<int> TemplateAsync(int i);
    }
}