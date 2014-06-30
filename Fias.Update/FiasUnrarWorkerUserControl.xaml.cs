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
    public partial class FiasUnrarWorkerUserControl : UserControl
    {
        private FiasProgressInfo ProgressInfo = null;
        private FiasUnrarWorker _unrarWorker = new FiasUnrarWorker();
        public FiasUnrarWorker UnrarWorker { get { return _unrarWorker; } }
        public FiasUnrarWorkerUserControl()
        {
            InitializeComponent();
            this.DataContext = UnrarWorker;
            UnrarWorker.OnUnrarStart += UnrarWorker_OnUnrarStart;
            UnrarWorker.OnUnrarComplete += UnrarWorker_OnUnrarComplete;
            UnrarWorker.OnUnrarProgress += UnrarWorker_OnUnrarProgress;
            UnrarWorker.OnCheckFilesStart += UnrarWorker_OnCheckFilesStart;
            UnrarWorker.OnCheckFilesComplete += UnrarWorker_OnCheckFilesComplete;
            UnrarWorker.OnCheckProgress += UnrarWorker_OnCheckProgress;
        }

        void UnrarWorker_OnUnrarProgress(long APosition, long ALength, string AText)
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                if (ProgressInfo.CancelClicked)
                    UnrarWorker.Stop();
                ProgressInfo.TextBlock.Text = AText;
                ProgressInfo.ProgressBar.Maximum = ALength;
                ProgressInfo.ProgressBar.Value = APosition;
            }));
        }
        void UnrarWorker_OnCheckProgress(long APosition, long ALength, string AText)
        {            
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                if (ProgressInfo.CancelClicked)
                    UnrarWorker.Stop();
                ProgressInfo.TextBlock.Text = AText;
            }));
        }
        void UnrarWorker_OnCheckFilesComplete()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                ProgressInfo.Close();
            }));
        }
        void UnrarWorker_OnCheckFilesStart()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                ProgressInfo = new FiasProgressInfo();
                ProgressInfo.ProgressBar.IsIndeterminate = true;
                ProgressInfo.Show();
            }));
        }
        void UnrarWorker_OnUnrarComplete()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                ProgressInfo.Close();
                btnUnrar.IsEnabled = true;
            }));
        }
        void UnrarWorker_OnUnrarStart()
        {
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                btnUnrar.IsEnabled = false;
                ProgressInfo = new FiasProgressInfo();
                ProgressInfo.ProgressBar.IsIndeterminate = false;
                ProgressInfo.Show();
            }));
        }
        private void btnUnrar_Click(object sender, RoutedEventArgs e)
        {
            _unrarWorker.UnrarFiles();
        }  
    }
}
