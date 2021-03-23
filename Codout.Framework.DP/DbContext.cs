using System.Data;

namespace Codout.Framework.DP
{
    public class DbContext
    {
        private IDbStrategy _dbStrategy;

        public DbContext SetStrategy(IDbStrategy dbStrategy) 
        {
            _dbStrategy = dbStrategy;
            return this;
        }

        public IDbConnection GetDbContext(string connectionString) 
        {
            return _dbStrategy.GetConnection(connectionString);
        }
    }
}
