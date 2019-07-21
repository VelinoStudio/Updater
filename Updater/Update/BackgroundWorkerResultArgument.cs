using System;

namespace VelinoStudio.Updater
{
    public class BackgroundWorkerResultArgument
    {
        public bool Result { get; set; }
        public Exception Error { get; set; }
        public object Object { get; set; }
    }
}
