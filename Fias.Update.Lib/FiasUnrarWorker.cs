using Rade.Compression;
using Rade.Hash;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Fias.Update.Lib
{    
    public class FiasUnrarWorker : FiasCustomWorker
    {
        private readonly ManualResetEvent stopEvent = new ManualResetEvent(false);

        public event WorkerEventHandler OnUnrarStart;
        public event WorkerEventHandler OnUnrarComplete;
        public event WorkerProgressEventHandler OnUnrarProgress;
        public event WorkerEventHandler OnCheckFilesStart;
        public event WorkerProgressEventHandler OnCheckProgress;
        public event WorkerEventHandler OnCheckFilesComplete;

        public FiasUnrarWorker()
        {
            RescanFolder();
        }

        private void DoUnrarFiles()
        {
            stopEvent.Reset();
            OnUnrarStart();
            try
            {
                foreach (var fileInfo in FileInfoList.Where(fileInfo => fileInfo.Checked))
                {
                    UnRarFile(fileInfo);
                }
            }
            /*catch (StopException)
            {
                //ex.ToString();
                // do nothing, it's normal
            }*/
            /*catch
            {
                //if (stopEvent.WaitOne(0)) ;
                //ex.ToString();
                // do nothing, it's normal
            }*/
            finally
            {
                OnUnrarComplete();
            }
        }

        public void UnrarFiles()
        {
            var t = new Thread(DoUnrarFiles);
            t.Start();
        }

        private void ScanFolder(string aRoot)
        {
            foreach (var folder in Directory.GetDirectories(aRoot))
                ScanFolder(folder);
            foreach (var file in Directory.GetFiles(aRoot, "fias_delta_xml.rar"))
                FileInfoList.Add(new FiasFileInfo { FileName = file });
        }

        public void RescanFolder()
        {
            FileInfoList.Clear();
            ScanFolder(RootFolder + "\\Data");
        }

        private void CheckFile(FiasFileInfo aFileInfo)
        {
            var folder = Path.GetDirectoryName(aFileInfo.FileName) + "\\Extracted";
            if (!Directory.Exists(folder))
            {
                aFileInfo.Extracted = false;
                return;
            }
            var files = Directory.GetFiles(folder).Select(Path.GetFileName).ToList();

            using (var unrar = new Unrar())
            {
                unrar.ArchivePathName = aFileInfo.FileName;
                unrar.Open(Unrar.OpenMode.List);
                while (unrar.ReadHeader())
                {
                    if (stopEvent.WaitOne(0))
                        throw new StopException();

                    if (OnCheckProgress != null)
                        OnCheckProgress(0, 0, string.Format("Проверка файла {0}", folder + "\\" + unrar.CurrentFile.FileName));
                    
                    if (!files.Contains(unrar.CurrentFile.FileName))
                    {
                        aFileInfo.Extracted = false;
                        return;
                    }

                    var crc32 = new Crc32();
                    uint hash;

                    using (var fs = File.Open(folder + "\\" + unrar.CurrentFile.FileName, FileMode.Open))
                    {
                        var byteHash = crc32.ComputeHash(fs);
                        hash = BitConverter.ToUInt32(byteHash.Reverse().ToArray(), 0);
                    }
                    if (unrar.CurrentFile.FileCRC != hash)
                    {
                        aFileInfo.Extracted = false;
                        return;
                    }
                    unrar.Skip();
                }
            }
            aFileInfo.Extracted = true;
        }
        public void CheckFiles()
        {
            var t = new Thread(DoCheckFiles);
            t.Start();
        }
        private void DoCheckFiles()
        {
            stopEvent.Reset();
            if (OnCheckFilesStart != null)
                OnCheckFilesStart();
            try
            {
                foreach (var fileInfo in FileInfoList)
                {
                    CheckFile(fileInfo);
                }
            }
            catch (StopException)
            {
                //ex.ToString();
                // do nothing, it's normal
            }
            finally
            {
                if (OnCheckFilesComplete != null)
                    OnCheckFilesComplete();
            }
        }private void UnRarFile(FiasFileInfo aFileInfo)
        {
            using (var unrar = new Unrar())
            {
                unrar.Open(aFileInfo.FileName, Unrar.OpenMode.Extract);
                unrar.DestinationPath = Path.GetDirectoryName(aFileInfo.FileName) + "\\Extracted.tmp";
                unrar.ExtractionProgress += unrar_ExtractionProgress;
                while (unrar.ReadHeader())
                {
                    if (stopEvent.WaitOne(0))
                        throw new StopException();
                    unrar.Extract();
                }
            }
            Directory.Move(Path.GetDirectoryName(aFileInfo.FileName) + "\\Extracted.tmp", Path.GetDirectoryName(aFileInfo.FileName) + "\\Extracted");
            aFileInfo.Checked = false;
            aFileInfo.Extracted = true;
        }
        private void unrar_ExtractionProgress(object sender, ExtractionProgressEventArgs e)
        {
            if (stopEvent.WaitOne(0))
                e.ContinueOperation = false;
            if (OnUnrarProgress != null)
                OnUnrarProgress(e.BytesExtracted,e.FileSize,string.Format("Распаковка {0}", e.FileName));
        }
        public void Stop()
        {
            stopEvent.Set();
        }
    }
}
