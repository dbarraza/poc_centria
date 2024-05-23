using Backend.Common.Interfaces.DataAccess;
using Backend.Entities;

namespace Backend.DataAccess
{
    public class ApplicationRepository : Repository<Application>, IApplicationRepository
    {
        public ApplicationRepository(DatabaseContext context) : base(context)
        {
        }
    }
}
