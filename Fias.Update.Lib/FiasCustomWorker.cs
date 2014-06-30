using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Invent.Entities;

namespace Fias.Update.Lib
{
    public delegate void WorkerEventHandler();
    public delegate void WorkerProgressEventHandler(long aPosition, long aLength, string aText);
    public class StopException : Exception
    { 
        
    }
    public class FiasCustomWorker : VirtualNotifyPropertyChanged
    {
        protected string RootFolder = string.Empty;
        public FiasCustomWorker()
        {
            RootFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        private readonly ObservableCollection<FiasFileInfo> fileInfoList = new ObservableCollection<FiasFileInfo>();
        public ObservableCollection<FiasFileInfo> FileInfoList { get { return fileInfoList; } }
    }
}
