using Fias.Update.Lib;
using System.Threading;
using System.Windows;

namespace Fias.Update
{
    public partial class FiasDbUpdateWorkerUserControl
    {
        private FiasProgressInfo progressInfo;
        private readonly FiasDbUpdateWorker dbUpdateWorker = new FiasDbUpdateWorker();
        public FiasDbUpdateWorker DbUpdateWorker { get { return dbUpdateWorker; } }
        public FiasDbUpdateWorkerUserControl()
        {
            InitializeComponent();
            DataContext = DbUpdateWorker;
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
            Dispatcher.BeginInvoke(new ThreadStart(() => progressInfo.Close()));
        }

        void DBUpdateWorker_OnLoadDataStart()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                progressInfo = new FiasProgressInfo
                {
                    TextBlock = {Text = "Загрузка данных"},
                    ProgressBar = {IsIndeterminate = true}
                };
                progressInfo.Show();
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

        void DbCache_OnRebuildCache(long aPosition, long aLength, string aText)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                if (progressInfo.CancelClicked)
                {
                    DbUpdateWorker.Stop();
                }
                if (aLength == 0)
                    progressInfo.TextBlock.Text = string.Format("Выполняется кэширование таблицы {0} ({1})", aText, aPosition);
                else
                    progressInfo.TextBlock.Text = string.Format("Выполняется кэширование таблицы {0} ({1}/{2})", aText, aPosition, aLength);
                progressInfo.ProgressBar.Value = aPosition;
                progressInfo.ProgressBar.Maximum = aLength;
                progressInfo.ProgressBar.IsIndeterminate = aLength == 0;
            }));
        }

        void DbCache_OnEndRebuildCache()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                progressInfo.Close();
                btnStop.IsEnabled = true;
            }));
        }

        void DbCache_OnStartRebuildCache()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                progressInfo = new FiasProgressInfo {TextBlock = {Text = "Подготовка к кэшированию"}};
                progressInfo.Show();
            }));
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            DbUpdateWorker.Stop();
        }
    }
}
