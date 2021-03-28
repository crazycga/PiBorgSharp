using System;

namespace PiBorgSharp.ThunderBorg
{
    /// <summary>
    /// The intention here is to allow for a variety of logging methods, so you're not tied to the Console.Writeline output; requires only a "WriteLog (string message)" function
    /// </summary>
    public class Logger_class : ILogger
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
