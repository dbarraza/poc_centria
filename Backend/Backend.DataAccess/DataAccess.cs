using Backend.Common.Interfaces.DataAccess;
using Backend.Entities;
using Backend.Models;
using Microsoft.Extensions.Configuration;

namespace Backend.DataAccess
{
    /// <inheritdoc/>
    public class DataAccess : IDisposable, IDataAccess
    {
        private DatabaseContext _context;
        private bool disposed = false;

        /// <inheritdoc/>
        public IRepository<Application> Applications { get; }

        /// <inheritdoc/>
        public IRepository<ReceivedCv> ReceivedCvs { get; }

        
        public DataAccess(IConfiguration configuration)
        {
            _context = new DatabaseContext(configuration);            
            _context.Database.EnsureCreated();
            
            // Repositories
            Applications = new Repository<Application>(_context);
            ReceivedCvs = new Repository<ReceivedCv>(_context);
        }

        /// <inheritdoc/>
        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            disposed = true;
        }
    }
}