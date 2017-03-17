using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Microsoft.Live.Safety.Tools.VRAS
{
    public static class VRASLogEvent
    {
        public static bool IsService { get; set; }

        public enum EventIDs
        {
            ErrorID = 5000,
            SetupError = 5001,
            FileCopyError = 5004,
            Success = 5100,
            Starting = 5102
        }

        public const string ConfigFile = "VRASConfiguration.xml";
        public const string EventLogName = "Application";
        
        public static string EventSourceDefault { get; set; }

        // make sure the event source is registered into the system.
        public static void PrimeEventLogSource(string source, string logName)
        {
            EventLog log = new EventLog(logName);
            log.Close();

            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, logName);
            }
        }

        public static void SetIsService(bool isThisAService)
        {
            IsService = isThisAService;
        }

       // log the message into the event log
        public static void LogMesage(
            string logName,
            string txt,
            EventLogEntryType type,
            int eventid,
            string source)
        {
            if (IsService)
            {
                object lockObj = new object();

                // multiple threads may be using class at the same time
                lock (lockObj)
                {
                    EventLog log = new EventLog(logName);
                    log.Source = source;
                    log.WriteEntry(txt, type, eventid);
                    log.Close();
                }
            }
            else
            {
                Console.WriteLine(txt);
            }
        }
    }
}
