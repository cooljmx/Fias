using Fias.Update.Lib.FiasServiceReference;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading;
using System.Windows.Threading;

namespace Fias.Update.Lib
{   
    public class FiasDownloadWorker : FiasCustomWorker
    {
        private readonly ManualResetEvent stopEvent = new ManualResetEvent(false);
        private string lastError;
        
        public event WorkerEventHandler OnDownloadStart;
        public event WorkerEventHandler OnDownloadComplete;
        public event WorkerProgressEventHandler OnDownloadProgress;
        public event WorkerEventHandler OnGetFileInfoListStart;
        public event WorkerEventHandler OnGetFileInfoListComplete;
        public event WorkerProgressEventHandler OnGetFileInfoProgress;
        //public event WorkerProgressEventHandler OnGetFileInfoListError;
        
        public Dispatcher Dispatcher { get; set; }
        public string LastError { get { return lastError; } private set { lastError = value; NotifyPropertyChanged("LastError"); } } 
        
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
        private void DoGetFileInfoList()
        {
            if (OnGetFileInfoListStart != null)
                OnGetFileInfoListStart();

            try
            {
                ClearList();

                var uri = new Uri("http://fias.nalog.ru/WebServices/Public/DownloadService.asmx");
                var ea = new EndpointAddress(uri);
                var binding = new BasicHttpBinding
                {
                    MaxReceivedMessageSize = 1024*1024               
                };
                using (var client = new DownloadServiceSoapClient(binding, ea))
                {
                    var position = 0;
                    var downloadFileInfoList = client.GetAllDownloadFileInfo();
                    foreach (var dfi in downloadFileInfoList)
                    {
                        //DownloadFileInfo dfi = DownloadFileInfoList[8];
                        var fi = new FiasFileInfo();
                        AddListItem(fi);

                        fi.Checked = false;
                        fi.Description = dfi.TextVersion;
                        fi.Url = dfi.FiasDeltaXmlUrl ?? dfi.FiasCompleteXmlUrl;
                        fi.Version = dfi.VersionId;

                        fi.FileName = RootFolder + "\\Data\\" + Path.GetFileName(Path.GetDirectoryName(fi.Url)) + "\\" + Path.GetFileName(fi.Url);
                        fi.IsExists = File.Exists(fi.FileName);

                        var req = (HttpWebRequest)WebRequest.Create(fi.Url);
                        req.Method = "HEAD";using (var resp = (HttpWebResponse)req.GetResponse())
                        {
                            fi.Length = resp.ContentLength;
                        }
                        if (OnGetFileInfoProgress != null)
                            OnGetFileInfoProgress(position++, downloadFileInfoList.Length, string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                /*if (OnGetFileInfoListError != null)
                    OnGetFileInfoListError(0, 0, ex.InnerException.Message);*/
            }
            finally
            {
                if (OnGetFileInfoListComplete != null)
                    OnGetFileInfoListComplete();
            }
        }
        public void GetFileInfoList()
        {
            var r = new Thread(DoGetFileInfoList);
            r.Start();      
        }      
        private void DownloadFile(FiasFileInfo aFileInfo)
        {
            using(var client = new WebClient())
            {
                using (var remoteStream = client.OpenRead(aFileInfo.Url))
                {
                    if (!Directory.Exists(Path.GetDirectoryName(aFileInfo.FileName)))
                        Directory.CreateDirectory(Path.GetDirectoryName(aFileInfo.FileName));
                    var tmpFileName = Path.GetDirectoryName(aFileInfo.FileName) + "\\data.tmp";
                    using (var localStream = new FileStream(tmpFileName, FileMode.Create, FileAccess.Write))
                    {
                        long totalBytesReaded = 0;
                        const int bufferLength = 1024 * 1024;

                        while (totalBytesReaded < aFileInfo.Length)
                        {
                            if (stopEvent.WaitOne(0))
                                throw new StopException();

                            byte[] buffer = null;
                            buffer = aFileInfo.Length - totalBytesReaded < bufferLength ? new byte[Convert.ToInt32(aFileInfo.Length - totalBytesReaded)] : new byte[bufferLength];
                            if (remoteStream != null)
                            {
                                var bytesReaded = remoteStream.Read(buffer, 0, buffer.Length);
                                localStream.Write(buffer, 0, bytesReaded);
                                totalBytesReaded += bytesReaded;
                            }

                            if (OnDownloadProgress != null)
                                OnDownloadProgress(totalBytesReaded, aFileInfo.Length, string.Format("Загрузка {0}", aFileInfo.FileName));
                        }
                    }

                    if (File.Exists(aFileInfo.FileName))
                        File.Delete(aFileInfo.FileName);
                    File.Move(tmpFileName, aFileInfo.FileName);
                    aFileInfo.IsExists = true;
                    aFileInfo.Checked = false;

                }
            }
        }
        public void DownloadFiles()
        {
            var t = new Thread(DoDownloadFiles);
            t.Start();
        }
        private void DoDownloadFiles()
        {
            if (OnDownloadStart != null)
                OnDownloadStart();
            try
            {
                stopEvent.Reset();
                foreach (var fi in FileInfoList.Where(fi => fi.Checked))
                {
                    DownloadFile(fi);
                }
            }
            catch (StopException ex)
            {
                ex.ToString();// do nothing, it's normal
            }
            finally
            {
                if (OnDownloadComplete != null)
                    OnDownloadComplete();
            }
        }
        public void Stop()
        {
            stopEvent.Set();
        }
    }
}
