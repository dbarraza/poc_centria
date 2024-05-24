using Backend.Entities;

namespace Backend.Common.Interfaces.Services
{
    public interface IApplicationService
    {
        public Task<Application> CreateApplicationAsync(string applicationName, string fileName, Stream data, string contentType);
    }
}
