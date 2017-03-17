using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Live.Safety.Tools.VRAS
{
    public class VRASCopyBlob
    {
        private VRASCopyConfiguration copyConfig;
        private FileInfo file;
        private VRASCopyOperation copyWorkItems;

        public VRASCopyBlob(VRASCopyConfiguration c, FileInfo f)
        {
            this.copyConfig = c;
            this.file = f;
        }

        public static List<VRASCopyBlob> GetFiles(VRASCopyConfiguration config)
        {
            try
            {
                List<VRASCopyBlob> blobList = new List<VRASCopyBlob>();

                DirectoryInfo dir = new DirectoryInfo(config.BaseFolderPath);
                IEnumerable<FileInfo> files;
                IEnumerable<FileInfo> qry;

                // set the query of folder enum based on the CopyType
                if (config.CopyType == SourceTypeCopyType.CombineToRoot || config.CopyType == SourceTypeCopyType.Mirror)
                {
                    files = dir.EnumerateFiles("*", SearchOption.AllDirectories);
                }
                else
                {
                    files = dir.EnumerateFiles("*", SearchOption.TopDirectoryOnly);
                }

                // choose query on file list returned based on the flag for copy 0 byte
                if (config.CopyZeroByteFiles)
                {
                    qry =
                        from file in files
                        where file.LastWriteTimeUtc < DateTime.UtcNow.AddSeconds(-1 * config.NewFileDelaySeconds) && file.Length >= 0
                        orderby file.LastWriteTimeUtc ascending
                        select file;
                }
                else
                {
                    qry =
                        from file in files
                        where file.LastWriteTimeUtc < DateTime.UtcNow.AddSeconds(-1 * config.NewFileDelaySeconds) && file.Length > 0
                        orderby file.LastWriteTimeUtc ascending
                        select file;
                }

                foreach (FileInfo f in qry)
                {
                    if (StringMatchesFileAndFolderRegEx(f, config.FileRegExFilter, config.FolderRegExFilter))
                    {
                        VRASCopyBlob blob = new VRASCopyBlob(config, f);

                        blobList.Add(blob);
                    }
                    else
                    {
                    }
                }

                return blobList;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error Enumerating Directory: {0}", ex.Message));
            }
        }

        // Once an instance of a blob is made this process will give the commands to intererate all the copy operations from the source file to N destinations.
        // It will all decode all the config settings on what the destination will look like.
        public void PopulateCopyWork()
        {
            // we have the blob now to generate all the copy tasks for this work item
            this.copyWorkItems = new VRASCopyOperation(this.file, this.copyConfig.BaseFolderPath, this.copyConfig.CopyType, this.copyConfig.DestinationCollection, this.copyConfig.CopyFromFirstDestinationDir, this.copyConfig.BatchName, this.copyConfig.RegExMatchNotFoundDefaultString, this.copyConfig.DeleteAfterCopy, this.copyConfig.CopyRetryCount, this.copyConfig.RetrySleepMs, this.copyConfig.TempFileExt);
        }

        public void DoWork(object stateInfo)
        {
            this.copyWorkItems.PerformCopy();
        }

        private static bool StringMatchesFileAndFolderRegEx(FileInfo f, string fileNameRegEx, string folderNameRegEx)
        {
            return StringMatchesFileRegEx(f, fileNameRegEx) && StringMatchesFolderRegEx(f, folderNameRegEx);
        }

        private static bool StringMatchesFileRegEx(FileInfo f, string fileNameRegEx)
        {
            try
            {
                return Regex.IsMatch(f.Name, fileNameRegEx);
            }
            catch
            {
                return false;
            }
        }

        private static bool StringMatchesFolderRegEx(FileInfo f, string folderNameRegEx)
        {
            try
            {
                return Regex.IsMatch(f.DirectoryName, folderNameRegEx);
            }
            catch
            {
                return false;
            }
        }
    }
}
