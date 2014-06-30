using Fias.Update.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fias.Update
{
    public partial class FiasDbUpdateWorkerUserControl : UserControl
    {
        private FiasProgressInfo ProgressInfo = null;
        private FiasDbUpdateWorker _dbUpdateWorker = new FiasDbUpdateWorker();
        public FiasDbUpdateWorker DbUpdateWorker { get { return _dbUpdateWorker; } }
        public FiasDbUpdateWorkerUserControl()
        {
            InitializeComponent();
            this.DataContext = DbUpdateWorker;
            DbUpdateWorker.Dispatcher = Dispatcher;
            DbUpdateWorker.OnUpdateStart += DBUpdateWorker_OnUpdateStart;
            DbUpdateWorker.OnUpdateComplete += DBUpdateWorker_OnUpdateEnd;
            DbUpdateWorker.DbCache.OnRebuildCacheStart += DbCache_OnStartRebuildCache;
            DbUpdateWorker.DbCache.OnRebuildCacheComplete += DbCache_OnEndRebuildCache;
            DbUpdateWorker.DbCache.OnRebuildCache += DbCache_OnRebuildCache;
            DbUpdateWorker.OnLoadDataStart += DBUpdateWorker_OnLoadDataStart;
            DbUpdateWorker.OnLoadDataComplete += DbUpdateWorker_OnLoadDataComplete;
        }

        void DbUpdateWorker_OnLoadDataComplete()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                ProgressInfo.Close();
            }));
        }

        void DBUpdateWorker_OnLoadDataStart()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                ProgressInfo = new FiasProgressInfo();
                ProgressInfo.TextBlock.Text = "Загрузка данных";
                ProgressInfo.ProgressBar.IsIndeterminate = true;
                ProgressInfo.Show();
            }));
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            //DbUpdateWorker.Open(@"data source=localhost;initial catalog=D:\DB\fias.fdb;user id=sysdba;password=masterkey;client library=C:\Program Files (x86)\Red Soft Corporation\Red Database\bin\fbclient.dll;character set=WIN1251");
            DbUpdateWorker.Update();
        }

        void DBUpdateWorker_OnUpdateEnd()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                btnUpdate.IsEnabled = true;
                btnStop.IsEnabled = false;
            }));
        }

        void DBUpdateWorker_OnUpdateStart()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                btnUpdate.IsEnabled = false;
                btnStop.IsEnabled = false;
            }));
        }

        void DbCache_OnRebuildCache(long APosition, long ALength, string AText)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                if (ProgressInfo.CancelClicked)
                {
                    DbUpdateWorker.Stop();
                }
                if (ALength == 0)
                    ProgressInfo.TextBlock.Text = string.Format("Выполняется кэширование таблицы {0} ({1})", AText, APosition);
                else
                    ProgressInfo.TextBlock.Text = string.Format("Выполняется кэширование таблицы {0} ({1}/{2})", AText, APosition, ALength);
                ProgressInfo.ProgressBar.Value = APosition;
                ProgressInfo.ProgressBar.Maximum = ALength;
                ProgressInfo.ProgressBar.IsIndeterminate = ALength == 0;
            }));
        }

        void DbCache_OnEndRebuildCache()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                ProgressInfo.Close();
                btnStop.IsEnabled = true;
            }));
        }

        void DbCache_OnStartRebuildCache()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                ProgressInfo = new FiasProgressInfo();
                ProgressInfo.TextBlock.Text = "Подготовка к кэшированию";
                ProgressInfo.Show();
            }));
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            DbUpdateWorker.Stop();
        }
    }
}
