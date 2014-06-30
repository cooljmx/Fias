using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    public partial class MainWindow : DXWindow
    {
        public MainWindow()
        {
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Data"))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+"\\Data");
            InitializeComponent();
            FiasDownloadWorkerUC.DownloadWorker.OnDownloadStart += DownloadWorker_OnDownloadStart;
            FiasDownloadWorkerUC.DownloadWorker.OnDownloadComplete += DownloadWorker_OnDownloadComplete;
            FiasDownloadWorkerUC.DownloadWorker.OnGetFileInfoListStart += DownloadWorker_OnGetFileInfoListStart;
            FiasDownloadWorkerUC.DownloadWorker.OnGetFileInfoListComplete += DownloadWorker_OnGetFileInfoListComplete;

            FiasUnrarWorkerUC.UnrarWorker.OnCheckFilesStart += UnrarWorker_OnCheckFilesStart;
            FiasUnrarWorkerUC.UnrarWorker.OnCheckFilesComplete += UnrarWorker_OnCheckFilesComplete;
            FiasUnrarWorkerUC.UnrarWorker.OnUnrarStart += UnrarWorker_OnUnrarStart;
            FiasUnrarWorkerUC.UnrarWorker.OnUnrarComplete += UnrarWorker_OnUnrarComplete;

            FiasDbUpdateWorkerUC.DbUpdateWorker.OnUpdateStart += DBUpdateWorker_OnUpdateStart;
            FiasDbUpdateWorkerUC.DbUpdateWorker.OnUpdateComplete += DBUpdateWorker_OnUpdateEnd;
        }

        void DBUpdateWorker_OnUpdateEnd()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                btnBack.IsEnabled = true;
                btnNext.IsEnabled = true;
                btnClose.IsEnabled = true;
                FiasDownloadWorkerUC.IsEnabled = true;
            }));
        }
        void DBUpdateWorker_OnUpdateStart()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                btnBack.IsEnabled = false;
                btnNext.IsEnabled = false;
                btnClose.IsEnabled = false;
                FiasDownloadWorkerUC.IsEnabled = false;
            }));  
        }
        void DownloadWorker_OnGetFileInfoListComplete()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                this.IsEnabled = true;
                btnBack.IsEnabled = true;
                btnNext.IsEnabled = true;
                btnClose.IsEnabled = true;
                //FiasDownloadWorkerUC.IsEnabled = true;
                //splash.Close(TimeSpan.FromSeconds(1));
            }));            
        }
        void DownloadWorker_OnGetFileInfoListStart()
        {            
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                //splash.Show(false);
                this.IsEnabled = false;
                btnBack.IsEnabled = false;
                btnNext.IsEnabled = false;
                btnClose.IsEnabled = false;
                //FiasDownloadWorkerUC.IsEnabled = false;
            }));  
        }
        void DownloadWorker_OnDownloadComplete()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                btnBack.IsEnabled = true;
                btnNext.IsEnabled = true;
                btnClose.IsEnabled = true;
                FiasDownloadWorkerUC.IsEnabled = true;
            }));
        }
        void DownloadWorker_OnDownloadStart()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                btnBack.IsEnabled = false;
                btnNext.IsEnabled = false;
                btnClose.IsEnabled = false;
                FiasDownloadWorkerUC.IsEnabled = false;
            }));  
        }
        void UnrarWorker_OnUnrarStart()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                btnBack.IsEnabled = false;
                btnNext.IsEnabled = false;
                btnClose.IsEnabled = false;
                FiasUnrarWorkerUC.IsEnabled = false;
            }));  
        }
        void UnrarWorker_OnCheckFilesComplete()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                btnBack.IsEnabled = true;
                btnNext.IsEnabled = true;
                btnClose.IsEnabled = true;
                FiasUnrarWorkerUC.IsEnabled = true;
            }));
        }
        void UnrarWorker_OnUnrarComplete()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                btnBack.IsEnabled = true;
                btnNext.IsEnabled = true;
                btnClose.IsEnabled = true;
                FiasUnrarWorkerUC.IsEnabled = true;
            }));
        }
        void UnrarWorker_OnCheckFilesStart()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                btnBack.IsEnabled = false;
                btnNext.IsEnabled = false;
                btnClose.IsEnabled = false;
                FiasUnrarWorkerUC.IsEnabled = false;
            }));           
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            switch (tabControl.SelectedIndex)
            { 
                case 0:
                    break;
                case 1:
                    tabControl.SelectedIndex = 0;
                    try
                    {
                        FiasDownloadWorkerUC.DownloadWorker.GetFileInfoList();
                    }
                    catch
                    {
                        MessageBox.Show("Сервис http://fias.nalog.ru не доступен");
                    }
                    break;
                case 2:
                    tabControl.SelectedIndex = 1;
                    break;
                case 3:
                    tabControl.SelectedIndex = 2;
                    break;
            }
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            switch (tabControl.SelectedIndex)
            {
                case 0:
                    tabControl.SelectedIndex = 1;
                    FiasUnrarWorkerUC.UnrarWorker.RescanFolder();
                    FiasUnrarWorkerUC.UnrarWorker.CheckFiles();
                    break;
                case 1:
                    tabControl.SelectedIndex = 2;
                    break;
                case 2:
                    FiasConnectionUC.DoNext();
                    tabControl.SelectedIndex = 3;
                    FiasDbUpdateWorkerUC.DbUpdateWorker.Open(FiasConnectionUC.Config.ConnectionString);
                    break;
                case 3:
                    break;
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FiasDownloadWorkerUC.DownloadWorker.GetFileInfoList();            
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Закрыть программу?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                FiasDbUpdateWorkerUC.DbUpdateWorker.Stop();
            else
                e.Cancel = true;
        }
    }
}
