using Fias.Update.Lib.Mapping;
using Rade.DbTools;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace Fias.Update.Lib
{
    public class FiasDbUpdateWorkerItemWorker
    {
        private FiasTableMap tableMap = null;
        private FiasFieldMap pkFieldMap = null;
        private int counter = 0;
        private int length = 0;
        private string sqlStatement = string.Empty;
        private DbTransaction transaction = null;
        private string shortFileName = string.Empty;

        public FiasFileInfo FileInfo { get; set; }
        public DbConnection Connection { get; set; }
        public FiasDbCache DbCache { get; set; }
        public FiasDatabaseMap DatabaseMap { get; set; }
        public ManualResetEvent StopEvent { get; set; }

        private void DoPrepare()
        {
            shortFileName = Path.GetFileName(FileInfo.FileName);
            tableMap = DatabaseMap.Tables.FirstOrDefault(a => shortFileName.StartsWith(a.XmlDescription));
            if (tableMap != null) pkFieldMap = tableMap.Fields.First(a => a.IsPrimaryKey);

            var lengthThread = new Thread(DoDetermineLength);
            lengthThread.Start();
        }
        private void DoDetermineLength()
        { 
            var c = 0;
            using (var reader = new XmlTextReader(FileInfo.FileName))
            {
                while (reader.Read())
                {
                    if (tableMap.XmlName == reader.Name) c++;
                }
            }
            length = c;
        }
        private void DoUpdateExecute()
        {
            using (var reader = new XmlTextReader(FileInfo.FileName))
            {
                transaction = Connection.BeginTransaction();
                if (
                    !DbData.IsRecordExists(Connection, transaction,
                        string.Format("select name from fias_import_history where name = '{0}';", shortFileName)))
                    DbData.ExecuteQuery(Connection, transaction,
                        string.Format("insert into fias_import_history (file_date,name) values('{0}','{1}');",
                            FileInfo.Description, shortFileName));
                var dtVersion =
                    DateTime.Parse(FileInfo.Description.Substring(0, 4) + '.' + FileInfo.Description.Substring(4, 2) +
                                   '.' + FileInfo.Description.Substring(6, 2));
                while (reader.Read())
                {
                    if (tableMap.XmlName == reader.Name)
                        counter++;

                    if (counter <= FileInfo.StatusPos) continue; // skiping commited

                    if (tableMap.XmlName != reader.Name) continue;

                    try
                    {
                        var recordExists = DbCache.IsExists(tableMap.DatabaseName, reader[pkFieldMap.XmlName]);
                        var data = new Dictionary<string, string>();
                        while (reader.MoveToNextAttribute())
                        {
                            if (tableMap.Fields.Exists(a => a.XmlName == reader.Name))
                                data.Add(tableMap.GetDatabaseFieldName(reader.Name), reader.Value.Replace("'", "''"));
                        }
                        data.Add("DT_VERSION", dtVersion.ToString());

                        if (recordExists)
                        {
                            if (pkFieldMap != null)
                            {
                                sqlStatement = string.Format("update {0} set {1} where {2} ='{3}';",
                                    tableMap.DatabaseName,
                                    data.Select(a => a.Key + "='" + a.Value + "'").Aggregate((a, b) => a + "," + b),
                                    pkFieldMap.DatabaseName, data[pkFieldMap.DatabaseName]);
                            }
                        }
                        else
                        {
                            sqlStatement = string.Format("insert into {0} ({1}) values ({2});", tableMap.DatabaseName,
                                data.Select(a => a.Key).Aggregate((a, b) => a + "," + b),
                                data.Select(a => "'" + a.Value + "'").Aggregate((a, b) => a + "," + b));
                        }

                        using (var query = new DbQuery(Connection, transaction))
                        {
                            query.SqlText = sqlStatement;
                            query.Execute();
                            if (!recordExists)
                                DbCache.Add(tableMap.DatabaseName, reader[pkFieldMap.XmlName]);
                        }

                        if (shortFileName.ToUpper().Contains("_DEL_"))
                        {
                            sqlStatement = string.Format("delete from {0} where {1}='{2}';", tableMap.RefTable,
                                pkFieldMap.DatabaseName, data[pkFieldMap.DatabaseName]);
                            using (var query = new DbQuery(Connection, transaction))
                            {
                                query.SqlText = sqlStatement;
                                query.Execute();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        if (!ex.Message.Contains("E_EXISTS_RECORD_IS_NEWER"))
                            throw;}
                    if (counter%100 != 0) continue;
                    DbData.ExecuteQuery(Connection, transaction,
                        string.Format("update fias_import_history set LAST_COMMIT_COUNT={0} where name = '{1}';",
                            counter, shortFileName));
                    transaction.Commit();
                    FileInfo.Status = length == 0
                        ? string.Format("Загружено {0}", counter)
                        : string.Format("Загружено {0}/{1}", counter, length);
                    FileInfo.StatusPos = counter;
                    FileInfo.StatusMax = length;

                    if (StopEvent.WaitOne(0))
                        throw new StopException();
                    transaction = Connection.BeginTransaction();
                }
                DbData.ExecuteQuery(Connection, transaction,
                    string.Format("update fias_import_history set dt_import='{0}' where name = '{1}';",
                        dtVersion.ToString(), shortFileName));
                transaction.Commit();
                FileInfo.Status = "Полностью загружен";
                FileInfo.IsExists = true;
            }
        }        
        public void Execute()
        {
            DoPrepare();
            DoUpdateExecute();
        }
    }
}
