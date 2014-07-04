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
    public partial class FiasDownloadWorkerUserControl : UserControl
    {
        private FiasProgressInfo progressInfo = null;

        private readonly FiasDownloadWorker downloadWorker = new FiasDownloadWorker();
        public FiasDownloadWorker DownloadWorker { get { return downloadWorker; } }
        public FiasDownloadWorkerUserControl()
        {
            InitializeComponent();
            this.DataContext = DownloadWorker;
            DownloadWorker.Dispatcher = Dispatcher;
            DownloadWorker.OnDownloadStart += Downloader_OnDownloadStart;
            DownloadWorker.OnDownloadComplete += Downloader_OnDownloadComplete;
            DownloadWorker.OnDownloadProgress += DownloadWorker_OnDownloadProgress;
            DownloadWorker.OnGetFileInfoListStart += DownloadWorker_OnGetFileInfoListStart;
            DownloadWorker.OnGetFileInfoListComplete += DownloadWorker_OnGetFileInfoListComplete;
            DownloadWorker.OnGetFileInfoProgress += DownloadWorker_OnGetFileInfoProgress;
            //DownloadWorker.OnGetFileInfoListError += DownloadWorker_OnGetFileInfoListError;
        }

        void DownloadWorker_OnGetFileInfoListError(long aPosition, long aLength, string aText)
        {
            DownloadWorker_OnGetFileInfoListComplete();
            MessageBox.Show(aText);
        }

        void DownloadWorker_OnDownloadProgress(long aPosition, long aLength, string aText)
        {
            if (progressInfo.CancelClicked)
            {
                DownloadWorker.Stop();
            }
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                progressInfo.ProgressBar.IsIndeterminate = false;
                progressInfo.ProgressBar.Maximum = aLength;
                progressInfo.ProgressBar.Value = aPosition;
                progressInfo.TextBlock.Text = aText;
            }));
        }

        void DownloadWorker_OnGetFileInfoProgress(long aPosition, long aLength, string aName)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                progressInfo.ProgressBar.IsIndeterminate = false;
                progressInfo.ProgressBar.Maximum = aLength;
                progressInfo.ProgressBar.Value = aPosition;
            }));
        }

        void DownloadWorker_OnGetFileInfoListComplete()
        {
            Dispatcher.BeginInvoke(new ThreadStart(() => progressInfo.Close()));
        }

        void DownloadWorker_OnGetFileInfoListStart()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                progressInfo = new FiasProgressInfo
                {
                    TextBlock = {Text = "Загрузка данных"},
                    ProgressBar = {IsIndeterminate = true},
                    btnCancel = {Visibility = Visibility.Hidden}
                };
                progressInfo.Show();
            }));
        }

        void Downloader_OnDownloadComplete()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                btnDownload.IsEnabled = true;
                progressInfo.Close();                
            }));           
        }

        void Downloader_OnDownloadStart()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                btnDownload.IsEnabled = false;
                progressInfo = new FiasProgressInfo();
                progressInfo.Show();
            }));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var f = false;
            try
            {
                //_downloader.GetFileInfoList();
            }
            catch
            {
                f = true;
            }            
            if (f)
                MessageBox.Show("Сервис http://fias.nalog.ru не доступен");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            downloadWorker.DownloadFiles();
        }
    }
}
