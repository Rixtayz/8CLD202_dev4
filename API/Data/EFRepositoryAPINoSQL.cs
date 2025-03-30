using MVC.Business;

namespace MVC.Data
{
    public class EFRepositoryAPINoSQL : EFRepositoryAPI<ApplicationDbContextNoSQL>
    {
        public EFRepositoryAPINoSQL(ApplicationDbContextNoSQL context, EventHubController eventHubController) : base(context, eventHubController) { }

    }
}
