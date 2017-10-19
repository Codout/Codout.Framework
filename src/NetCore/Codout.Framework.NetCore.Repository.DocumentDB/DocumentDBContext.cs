using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Documents.Client;

namespace Codout.Framework.NetCore.Repository.DocumentDB
{
    public class DocumentDBContext
    {

        private static DocumentClient client;
        private static string DatabaseId;
        private static string CollectionId;

        public DocumentDBContext()
        {
            var database = new Microsoft.Azure.Documents.Database();
        }
    }
}
