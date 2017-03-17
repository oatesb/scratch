using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Live.Safety.Tools.VRAS
{
    // class will define one unit of the xsd input types to run copy jobs.  It will contain one batch settings, source folder, N destination folders, and a thread pool to run the copy operations.
    public class VRASCopyConfiguration
    {
        public int CopyThreadPerSource { get; set; }

        public bool CopyZeroByteFiles { get; set; }
        
        public bool DeleteAfterCopy { get; set; }
        
        public int NewFileDelaySeconds { get; set; }
        
        public string BatchName { get; set; }
        
        public bool CopyFromFirstDestinationDir { get; set; }
        
        public int CopyRetryCount { get; set; }
        
        public bool RemoveEmptyFoldersAfterCopy { get; set; }
        
        public int RetrySleepMs { get; set; }
        
        public int SleepBetweenBatchesMs { get; set; }
        
        public string ErrorLogPath { get; set; }
        
        public string EventSource { get; set; }
        
        public string RegExMatchNotFoundDefaultString { get; set; }
        
        public int ServiceStopTimeOutSec { get; set; }
        
        public string BaseFolderPath { get; set; }
        
        public SourceTypeCopyType CopyType { get; set; }
        
        public string FileRegExFilter { get; set; }
        
        public string FolderRegExFilter { get; set; }

        public string TempFileExt { get; set; }

        public List<DestinationType> DestinationCollection { get; set; }

        public VRASCopyConfiguration(GlobalSettings globals, BatchSettingsType settingsForBatch, SourceType source, List<DestinationType> destinations)
        {
            // source settings
            this.BaseFolderPath = source.SourceFolderPath;
            this.CopyType = source.CopyType;
            this.FileRegExFilter = source.FileRegExFilter;
            this.FolderRegExFilter = source.FolderRegExFilter;
                        
            // source folder overrides
            this.CopyThreadPerSource = System.Convert.ToInt32(source.SourceSettingOverrides.CopyThreadPerSource);
            this.CopyZeroByteFiles = source.SourceSettingOverrides.CopyZeroByteFiles;
            this.DeleteAfterCopy = source.SourceSettingOverrides.DeleteAfterCopy;
            this.NewFileDelaySeconds = System.Convert.ToInt32(source.SourceSettingOverrides.NewFileDelaySeconds);

            // figure out what settings overrides to use
            SettingThatAreOverridableType mySettings = new SettingThatAreOverridableType();
            if (source.SourceSettingOverrides != null)
            {
                mySettings = source.SourceSettingOverrides;
            }
            else
            {
                mySettings = settingsForBatch.BatchSettingOverrides;
            }

            // batch settings
            this.BatchName = settingsForBatch.BatchName;
            this.CopyFromFirstDestinationDir = mySettings.CopyFromFirstDestinationDir;
            this.CopyRetryCount = System.Convert.ToInt32(mySettings.CopyRetryCount);
            this.RetrySleepMs = System.Convert.ToInt32(mySettings.RetrySleepMs);
            this.SleepBetweenBatchesMs = System.Convert.ToInt32(settingsForBatch.BatchSettingOverrides.SleepBetweenBatchesMs);

            // golbal settings
            this.ErrorLogPath = globals.ErrorLogPath;
            this.EventSource = globals.EventSource;
            this.RegExMatchNotFoundDefaultString = globals.RegExMatchNotFoundDefaultString;
            this.ServiceStopTimeOutSec = System.Convert.ToInt32(globals.ServiceStopTimeOutSec);
            this.TempFileExt = "." + globals.TempFileExt.TrimStart('.');

            // destinations
            this.DestinationCollection = destinations;

            // prime the event log source.
            VRASLogEvent.EventSourceDefault = this.EventSource;
        }
    }
}
