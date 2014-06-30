using System.Data.SqlClient;
using Fias.Update.Lib.Mapping;
using Rade.DbTools;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Fias.Update.Lib
{
    public class FiasDbCache
    {
        private readonly Dictionary<string, HashSet<string>> cache = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, int> cacheLength = new Dictionary<string,int>();
        private string connectionString = string.Empty;
        private readonly ManualResetEvent stopEvent = new ManualResetEvent(false);
        public string ConnectionString { get { return connectionString; } set { connectionString = value; } }

        public event WorkerEventHandler OnRebuildCacheStart;
        public event WorkerEventHandler OnRebuildCacheComplete;
        public event WorkerProgressEventHandler OnRebuildCache;
                
        public void RebuildCache()
        {
            stopEvent.Reset();
            var cacheLengthThread = new Thread(DoCacheLength);
            cacheLengthThread.Start();
            DoCache();
        }

        public bool IsExists(string aTableName, string aValue)
        {
            return cache[aTableName].Contains(aValue);
        }

        public void Stop()
        {
            stopEvent.Set();
        }private void DoCache()
        {
            if (OnRebuildCacheStart != null)
                OnRebuildCacheStart();
            try
            {
                cache.Clear();
                using (DbConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        var databaseMap = FiasDatabaseMap.LoadFromFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Fias.Mapping.xml");
                        foreach (var tableMap in databaseMap.Tables)
                        {
                            var pkFieldMap = tableMap.Fields.First(a => a.IsPrimaryKey);
                            using (var query = new DbQuery(connection, transaction))
                            {
                                var counter = 0;
                                var set = new HashSet<string>();
                                cache.Add(tableMap.DatabaseName, set);
                                query.SqlText = string.Format("select rtrim(ltrim({0})) {0} from {1};", pkFieldMap.DatabaseName, tableMap.DatabaseName);
                                query.ExecuteDataReader();
                                while (query.DataReader.Read())
                                {
                                    if (stopEvent.WaitOne(0))
                                        throw new StopException();
                                    set.Add(query.DataReader[pkFieldMap.DatabaseName].ToString());

                                    if (++counter%100000 != 0) continue;
                                    if (OnRebuildCache == null) continue;
                                    OnRebuildCache(counter,cacheLength.ContainsKey(tableMap.DatabaseName) ? cacheLength[tableMap.DatabaseName] : 0, tableMap.DatabaseName);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (OnRebuildCacheComplete != null)
                    OnRebuildCacheComplete();
            }             
        }public void Add(string aTableName, string aValue)
        {
            cache[aTableName].Add(aValue);
        }
        private void DoCacheLength()
        {
            cacheLength.Clear();
            using (DbConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var databaseMap = FiasDatabaseMap.LoadFromFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Fias.Mapping.xml");
                    foreach (var tableMap in databaseMap.Tables)
                    {
                        if (stopEvent.WaitOne(0))
                            return;//throw new StopException();
                        var pkFieldMap = tableMap.Fields.First(a => a.IsPrimaryKey);
                        var length = Convert.ToInt32(DbData.GetFieldValue(connection, transaction, string.Format("select count({0})from {1};", pkFieldMap.DatabaseName, tableMap.DatabaseName)));
                        cacheLength.Add(tableMap.DatabaseName, length);
                    }
                }
            }
        }
    }
}
