using System.Data.SqlClient;
using Fias.Update.Lib.Mapping;
using FirebirdSql.Data.FirebirdClient;
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
        public ServerType ServerType { get; set; }

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
                DbConnection connection;
                if(ServerType == ServerType.Firebird) connection = new FbConnection(ConnectionString); else connection = new SqlConnection(ConnectionString);
                try
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        var databaseMap =
                            FiasDatabaseMap.LoadFromFile(
                                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Fias.Mapping.xml");
                        foreach (var tableMap in databaseMap.Tables)
                        {
                            var pkFieldMap = tableMap.Fields.First(a => a.IsPrimaryKey);
                            using (var query = new DbQuery(connection, transaction))
                            {
                                var counter = 0;
                                var set = new HashSet<string>();
                                cache.Add(tableMap.DatabaseName, set);

                                var sql = ServerType==ServerType.Firebird ? "select trim({0}) {0} from {1};" : "select rtrim(ltrim({0})) {0} from {1};";
                                query.SqlText = string.Format(sql,
                                    pkFieldMap.DatabaseName, tableMap.DatabaseName);
                                query.ExecuteDataReader();
                                while (query.DataReader.Read())
                                {
                                    if (stopEvent.WaitOne(0))
                                        throw new StopException();
                                    set.Add(query.DataReader[pkFieldMap.DatabaseName].ToString());

                                    if (++counter%100000 != 0) continue;
                                    if (OnRebuildCache == null) continue;
                                    OnRebuildCache(counter,
                                        cacheLength.ContainsKey(tableMap.DatabaseName)
                                            ? cacheLength[tableMap.DatabaseName]
                                            : 0, tableMap.DatabaseName);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    connection.Dispose();
                }
            }
            finally
            {
                if (OnRebuildCacheComplete != null)
                    OnRebuildCacheComplete();
            }             
        }
        
        public void Add(string aTableName, string aValue)
        {
            cache[aTableName].Add(aValue);
        }
        
        private void DoCacheLength()
        {
            cacheLength.Clear();
            DbConnection connection;
            if (ServerType == ServerType.Firebird) connection = new FbConnection(ConnectionString); else connection = new SqlConnection(ConnectionString);
            try
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var databaseMap =
                        FiasDatabaseMap.LoadFromFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                                     "\\Fias.Mapping.xml");
                    foreach (var tableMap in databaseMap.Tables)
                    {
                        if (stopEvent.WaitOne(0))
                            return; //throw new StopException();
                        var pkFieldMap = tableMap.Fields.First(a => a.IsPrimaryKey);
                        var length =
                            Convert.ToInt32(DbData.GetFieldValue(connection, transaction,
                                string.Format("select count({0})from {1};", pkFieldMap.DatabaseName,
                                    tableMap.DatabaseName)));
                        cacheLength.Add(tableMap.DatabaseName, length);
                    }
                }
            }
            finally
            {
                connection.Dispose();
            }
        }
    }
}
