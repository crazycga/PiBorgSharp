using System;
using System.Collections.Generic;
using System.Text;

namespace PiBorgSharp.ThunderBorg
{
    public interface ILogger
    {
        public enum Priority
        {
            Critical = 5,
            High = 4,
            Medium = 3,
            Low = 2,
            Information = 1
        }

        ILogger.Priority DefaultLogLevel { get; set; }

        //TODO: introduce log diagnostic output routine

        public void WriteLog(string message = "", Priority messagePriority = Priority.Critical);
    }
}
