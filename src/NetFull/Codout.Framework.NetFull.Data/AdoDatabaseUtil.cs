using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codout.Framework.NetFull.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class AdoDatabaseUtil
    {

        private NetStandard.Data.DatabaseUtil databaseUtil;

        public AdoDatabaseUtil(string nomeProvider)
        {
            try
            {
                var dbCon = DbProviderFactories.GetFactory(nomeProvider);

                databaseUtil = new NetStandard.Data.DatabaseUtil(dbCon.CreateConnection());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
        
        public static DataTable FactoryDatabase(string nomeProvider)
        {
            try
            {
                var lista = DbProviderFactories.GetFactoryClasses();
                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public NetStandard.Data.DatabaseUtil DBUtil { get { return databaseUtil; } }
    }
}
