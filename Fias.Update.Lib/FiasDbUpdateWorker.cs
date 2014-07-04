using System.Data.SqlClient;
using Fias.Update.Lib.Mapping;
using FirebirdSql.Data.FirebirdClient;
using Rade.DbTools;
using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace Fias.Update.Lib
{
    public enum ServerType
    {
        MsSql = 0,
        Firebird = 1
    }

    public class FiasDbUpdateWorker : FiasCustomWorker
    {
        public event WorkerEventHandler OnUpdateStart;
        public event WorkerEventHandler OnUpdateComplete;
        public event WorkerEventHandler OnLoadDataStart;
        public event WorkerEventHandler OnLoadDataComplete;

        private readonly FiasDbCache dbCache = new FiasDbCache();        
        private readonly ManualResetEvent stopEvent = new ManualResetEvent(false);
        private DbConnection connection = null;

        public Dispatcher Dispatcher { get; set; }

        public FiasDbCache DbCache
        {
            get { return dbCache; }
        }

        public void Open(string aConnectionString, ServerType serverType)
        {
            dbCache.ConnectionString = aConnectionString;
            dbCache.ServerType = serverType;
            switch (serverType)
            {
                case ServerType.Firebird:
                    connection = new FbConnection {ConnectionString = aConnectionString};
                    break;
                case ServerType.MsSql:
                    connection = new SqlConnection { ConnectionString = aConnectionString };
                    break;
            }
            connection.Open();

            if (serverType==ServerType.Firebird)
                using (var transaction = connection.BeginTransaction())
                {
                    DbData.GetFieldValue(connection, transaction,
                        "select rdb$set_context('USER_SESSION','RPL_SERVICE','1') from rdb$database;");
                    transaction.Commit();
                }

            var t = new Thread(LoadData);
            t.Start();
        }
        private void ClearList()
        {
            if (Dispatcher == null)
                FileInfoList.Clear();
            else
            {
                Dispatcher.BeginInvoke(new ThreadStart(() => FileInfoList.Clear()));
            }
        }
        private void AddListItem(FiasFileInfo aFileInfo)
        {
            if (Dispatcher == null)
                FileInfoList.Add(aFileInfo);
            else
            {
                Dispatcher.BeginInvoke(new ThreadStart(() => FileInfoList.Add(aFileInfo)));
            }
        }
        private void LoadData()
        {
            if (OnLoadDataStart != null)
                OnLoadDataStart();

            try
            {
                ClearList();
                var databaseMap = FiasDatabaseMap.LoadFromFile(RootFolder + "\\Fias.Mapping.xml");
                using (var transaction = connection.BeginTransaction())
                {
                    var dataFolder = RootFolder + "\\Data\\";
                    foreach (var dateFolder in Directory.GetDirectories(dataFolder).OrderBy(a => a))
                    {
                        var extractedFolder = dateFolder + "\\Extracted\\";
                        if (!Directory.Exists(extractedFolder)) continue;
                        foreach (var xmlFileName in Directory.GetFiles(extractedFolder))
                        {
                            var fInfo = new FiasFileInfo
                            {
                                Description = Path.GetFileName(dateFolder),
                                FileName = xmlFileName
                            };
                            var fileName = Path.GetFileName(xmlFileName);
                            var dtImport = DbData.GetFieldValue(connection, transaction, string.Format("select DT_IMPORT from FIAS_IMPORT_HISTORY where name = '{0}';", fileName));
                            if (dtImport == null || dtImport is DBNull)
                            {
                                var c = 0;
                                var updated = DbData.GetFieldValue(connection, transaction, string.Format("select LAST_COMMIT_COUNT from FIAS_IMPORT_HISTORY where name = '{0}';", fileName));
                                if (!(updated == null || updated is DBNull))
                                    c = Convert.ToInt32(updated);
                                fInfo.StatusPos = c;
                                fInfo.Status = c == 0 ? string.Format("Пусто") : string.Format("Загружено записей {0}", c);
                                fInfo.IsExists = false;
                            }
                            else
                            {
                                fInfo.Url = dtImport.ToString();
                                fInfo.Status = "Полностью загружен";
                                fInfo.IsExists = true;
                            }

                            var tableMap = databaseMap.Tables.FirstOrDefault(a =>
                            {
                                var name = Path.GetFileName(fInfo.FileName);
                                return name != null && name.StartsWith(a.XmlDescription);
                            });
                            fInfo.Length = tableMap == null ? 0 : tableMap.OrderNum;
                            fInfo.Checked = tableMap != null;
                            AddListItem(fInfo);
                        }
                    }
                }
            }
            finally
            {
                if (OnLoadDataComplete != null)
                    OnLoadDataComplete();
            }
        }
        public void Update()
        {
            var t = new Thread(DoUpdate);
            t.Start();    
        }
        public void Stop()
        {
            stopEvent.Set();
            dbCache.Stop();
        }
       
        private void DoUpdate()
        {
            stopEvent.Reset();
            if (OnUpdateStart != null)
                OnUpdateStart();
            try
            {
                dbCache.RebuildCache();
                //var databaseMap = FiasDatabaseMap.LoadFromFile(RootFolder + "\\Fias.Mapping.xml");
                foreach (var fInfo in from date in FileInfoList.Select(a => a.Description).Distinct() from fInfo in FileInfoList.Where(a => (a.Description == date) && (a.Url == string.Empty) && (a.Checked)).OrderBy(a => a.Length) where !stopEvent.WaitOne(0) select fInfo)
                {
                    ProcessXmlFile(fInfo);
                }
            }
            catch (StopException ex)
            {
                ex.ToString();// do nothing, it's normal
            }
            finally
            {
                if (OnUpdateComplete != null)
                    OnUpdateComplete();
            }
        }
        private void ProcessXmlFile(FiasFileInfo aFileInfo)
        {
            var databaseMap = FiasDatabaseMap.LoadFromFile(RootFolder + "\\Fias.Mapping.xml");

            var itemWorker = new FiasDbUpdateWorkerItemWorker
            {
                Connection = connection,
                DbCache = DbCache,
                FileInfo = aFileInfo,
                DatabaseMap = databaseMap,
                StopEvent = stopEvent
            };
            itemWorker.Execute();
        }
    }
}
