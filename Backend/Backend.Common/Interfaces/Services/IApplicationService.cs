using Backend.Entities;

namespace Backend.Common.Interfaces.Services
{
    public interface IApplicationService
    {
        public Task<Application> CreateApplicationAsync(string name);
    }
}
