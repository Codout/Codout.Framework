using System;
using System.Collections;
using System.Web;

namespace Codout.Framework.Commom.Helpers
{
    public class Local
    {
        public static object Obj = new object();

        public static ILocalData Data { get; } = new LocalData();

        #region Nested type: LocalData

        private class LocalData : ILocalData
        {
            [ThreadStatic]
            private static Hashtable _localData;

            private static readonly object LocalDataHashtableKey = new object();

            private static Hashtable LocalHashtable
            {
                get
                {
                    if (!RunningInWeb)
                    {
                        lock (Obj)
                        {
                            return _localData ?? (_localData = new Hashtable());    
                        }
                    }
                    
                    var webHashtable = HttpContext.Current.Items[LocalDataHashtableKey] as Hashtable;
                    
                    if (webHashtable == null)
                    {
                        lock (Obj)
                        {
                            webHashtable = new Hashtable();
                            HttpContext.Current.Items[LocalDataHashtableKey] = webHashtable;
                        }
                    }

                    return webHashtable;
                }
            }

            private static bool RunningInWeb
            {
                get { return HttpContext.Current != null; }
            }

            #region ILocalData Members

            public object this[object key]
            {
                get { return LocalHashtable[key]; }
                set { LocalHashtable[key] = value; }
            }

            public int Count
            {
                get { return LocalHashtable.Count; }
            }

            public void Clear()
            {
                LocalHashtable.Clear();
            }

            #endregion
        }

        #endregion
    }
}