using System.Collections.Generic;
using Codout.Framework.DAL;
using NHibernate;

namespace Codout.Framework.NH
{
    public interface ISessionStorage
    {
        IEnumerable<ISession> GetAllSessions();

        ISession GetSessionForTenant(ITenant tenant);

        void SetSessionForTenant(ITenant tenant, ISession session);
    }
}
