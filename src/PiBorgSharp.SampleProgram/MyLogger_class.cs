using System;
using System.Collections.Generic;
using System.Text;
using PiBorgSharp;

namespace PiBorgSharp.SampleProgram
{
    class MyLogger_class : ILogger 
    {
        private string _filename = string.Empty;
        private ILogger.Priority _default = ILogger.Priority.Information;

        public ILogger.Priority DefaultLogLevel
        {
            get
            {
                return this._default;
            }
            set
            {
                this._default = value;
            }
        }

        public void WriteLog(string message = "", ILogger.Priority messagePriority = ILogger.Priority.Medium)
        {
            // immediate check against priority for speedy return; if the message is of lower priority, straight up reject message
            if (messagePriority < this.DefaultLogLevel) return;

            if (message.Equals(string.Empty))
            {
                Console.WriteLine();
                return;
            }

            if (messagePriority >= this.DefaultLogLevel)
            {
                Console.WriteLine(DateTime.Now.ToString() + ": " + message);
            }
        }

    }
}
