using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Microsoft.Live.Safety.Tools.VRAS
{ 
    public struct VRASCopyOperationHelper
    {
        public string SourceFullString;
        public VRASCopyDestinationOperationHelper DestInfo;
        public bool HashCompare;
    }

    public struct VRASCopyDestinationOperationHelper
    {
        public string DestFullString;
        public string DestPathOnly;
    }

    public class VRASCopyOperation
    {
        private FileInfo sourceFile;
        private List<DestinationType> destinationInfo;
        private List<VRASCopyOperationHelper> filesToCopy;
        private string sourceRootFolder;
        private bool copyFromFirstDestinationFolder;
        private SourceTypeCopyType copyTypeOperation;
        private string batchSetName;
        private string defaultRegExString;
        private bool moveFiles;
        private int copyRetries;
        private int copyRetrySleepMs;
        private bool copyErrorOccured;
        private string errorString;
        private string tempFileExt;

        public VRASCopyOperation(FileInfo source, string sourceRootPath, SourceTypeCopyType copyType, List<DestinationType> dest, bool copyFromFirstDest, string copySetName, string defaultRegexNotFound, bool deleteFileAfterMove, int copyRetryAttempts, int retrySleepMs, string tempFileExt)
        {
            this.sourceFile = source;
            this.sourceRootFolder = sourceRootPath;
            this.destinationInfo = dest;
            this.copyFromFirstDestinationFolder = copyFromFirstDest;
            this.filesToCopy = new List<VRASCopyOperationHelper>();
            this.copyTypeOperation = copyType;
            this.batchSetName = copySetName;
            this.defaultRegExString = defaultRegexNotFound;

            this.moveFiles = deleteFileAfterMove;
            this.copyRetries = copyRetryAttempts;
            this.copyRetrySleepMs = retrySleepMs;

            this.copyErrorOccured = false;
            this.errorString = string.Empty;
            this.tempFileExt = tempFileExt;

            this.Init();
        }

        private void Init()
        {
            try
            {
                int cur = 0;
                string firstDestFileFullPath = String.Empty;

                foreach (DestinationType d in this.destinationInfo)
                {
                    // need to keep track on the instance count
                    cur++;

                    // now populate the copy operation
                    VRASCopyOperationHelper helper = new VRASCopyOperationHelper();

                    // figure out the source
                    if (this.copyFromFirstDestinationFolder & cur > 1)
                    {
                        // was made the first run
                        helper.SourceFullString = firstDestFileFullPath;
                    }
                    else
                    {
                        helper.SourceFullString = this.sourceFile.FullName;
                    }

                    // figure out the destination
                    helper.DestInfo = this.DestinationFileGenerator(d.DestinationLocation, this.sourceFile.DirectoryName, this.sourceFile.FullName, this.sourceRootFolder, this.sourceFile.Name, d.ArchiveSettings, this.batchSetName, this.defaultRegExString);

                    // assign this incase this is to be the new source for the copy.  For the next run through the destination lists
                    if (cur == 1)
                    {
                        firstDestFileFullPath = helper.DestInfo.DestFullString;
                    }

                    // need to hash compare?
                    helper.HashCompare = d.HashCompareFlag;

                    this.filesToCopy.Add(helper);

                    // Delete all in process file in destination folder
                    foreach (string file in Directory.GetFiles(d.DestinationLocation, "*" + this.tempFileExt))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                            // Swallow exceptions if we cannot delete it.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                VRASLogEvent.LogMesage(
                        VRASLogEvent.EventLogName,
                        "Error: " + ex.Message,
                        System.Diagnostics.EventLogEntryType.Error,
                        Convert.ToInt32(VRAS.VRASLogEvent.EventIDs.FileCopyError),
                        VRASLogEvent.EventSourceDefault);
            }
        }

        // use the class variable to help determine the generated path file name
        // SourceRootPath is the root path the source file was gather from.  Note the actual path of source file may be longer because it was nested in child folder from the root source
        private VRASCopyDestinationOperationHelper DestinationFileGenerator(string baseDestPath, string fullFileSourePath, string fullFileSourePathAndFile, string sourceRootPath, string fileName, ArchiveFeatureType archiveOptions, string copySetName, string regExNotFoundDefault)
        {
            VRASCopyDestinationOperationHelper destInfo = new VRASCopyDestinationOperationHelper();
            string archivePath = String.Empty;

            if (archiveOptions.ArchiveFeatureEnabled && archiveOptions.ArchiveLines != null)
            {
                // make a string array as large as the archinve lines
                string[] arr = new string[archiveOptions.ArchiveLines.Length];
                int cur = -1;

                // archive settings enabled
                // Tiers
                // archive option: RegExOnFileName FileTimeLastModifyTime FileTimeDateCreatedTime FileTimeLastModifyTimeUtc FileTimeDateCreatedTimeUtc StaticName CopySetName RegExOnFilePath
                foreach (ArchiveLineType line in archiveOptions.ArchiveLines)
                {
                    cur++;
                    switch (line.Option.ArchiveOption)
                    {
                        case ArchiveTypeArchiveOption.CopySetName:
                            {
                                arr[cur] = this.FixPath(copySetName);
                                break;
                            }

                        case ArchiveTypeArchiveOption.FileTimeDateCreatedTime:
                            {
                                DateTime d = new DateTime();
                                d = new FileInfo(fullFileSourePathAndFile).CreationTime;
                                arr[cur] = this.FixPath(d.ToString(line.Option.ArchiveOptionValue));
                                break;
                            }

                        case ArchiveTypeArchiveOption.FileTimeLastModifyTime:
                            {
                                DateTime d = new DateTime();
                                d = new FileInfo(fullFileSourePathAndFile).LastWriteTime;
                                arr[cur] = this.FixPath(d.ToString(line.Option.ArchiveOptionValue));
                                break;
                            }

                        case ArchiveTypeArchiveOption.FileTimeDateCreatedTimeUtc:
                            {
                                DateTime d = new DateTime();
                                d = new FileInfo(fullFileSourePathAndFile).CreationTimeUtc;
                                arr[cur] = this.FixPath(d.ToString(line.Option.ArchiveOptionValue));
                                break;
                            }

                        case ArchiveTypeArchiveOption.FileTimeLastModifyTimeUtc:
                            {
                                DateTime d = new DateTime();
                                d = new FileInfo(fullFileSourePathAndFile).LastWriteTimeUtc;
                                arr[cur] = this.FixPath(d.ToString(line.Option.ArchiveOptionValue));
                                break;
                            }

                        case ArchiveTypeArchiveOption.RegExOnFileName:
                            {
                                // add a folder that matches part of the source file name
                                Match match = Regex.Match(fileName, line.Option.ArchiveOptionValue, RegexOptions.IgnoreCase);
                                if (match.Success)
                                {
                                    arr[cur] = this.FixPath(match.Groups[0].Value);
                                }
                                else
                                {
                                    arr[cur] = this.FixPath(regExNotFoundDefault);
                                }

                                break;
                            }

                        case ArchiveTypeArchiveOption.RegExOnFilePath:
                            {
                                // add a folder that matches part of the source file path
                                Match match = Regex.Match(fullFileSourePath, line.Option.ArchiveOptionValue, RegexOptions.IgnoreCase);
                                if (match.Success)
                                {
                                    arr[cur] = this.FixPath(match.Groups[0].Value);
                                }
                                else
                                {
                                    arr[cur] = this.FixPath(regExNotFoundDefault);
                                }

                                break;
                            }

                        case ArchiveTypeArchiveOption.StaticName:
                            {
                                arr[cur] = this.FixPath(line.Option.ArchiveOptionValue);
                                break;
                            }
                    }
                }

                // now loop through the array to make the string for the archive path folder
                StringBuilder str = new StringBuilder();
                foreach (string archive in arr)
                {
                    // make sure not adding empty strings
                    if (archive == String.Empty || archive == @"/")
                    {
                        str.Append(this.FixPath(regExNotFoundDefault));
                    }
                    else
                    {
                        str.Append(this.FixPath(archive));
                    }
                }

                archivePath = str.ToString();
            }

            // see if the destination is at a root level or in a mirrored configuration
            if (this.copyTypeOperation == SourceTypeCopyType.CombineToRoot || this.copyTypeOperation == SourceTypeCopyType.Root)
            {
                // just copy the source file name to dest folder as is
                destInfo.DestFullString = this.FixPath(baseDestPath) + this.FixPath(archivePath) + fileName;
                destInfo.DestPathOnly = this.FixPath(baseDestPath) + this.FixPath(archivePath);
            }
            else
            {
                // need to come up with the delta for the source path - it base path to insert between dest base and file name
                string deltaSourcePath = this.FixPath(fullFileSourePath).Replace(this.FixPath(sourceRootPath), String.Empty);

                destInfo.DestFullString = this.FixPath(baseDestPath) + this.FixPath(deltaSourcePath) + this.FixPath(archivePath) + fileName;
                destInfo.DestPathOnly = this.FixPath(baseDestPath) + this.FixPath(deltaSourcePath) + this.FixPath(archivePath);
            }
                
            return destInfo;
        }

        // make sure the path ends in '\' unless it comes in empty then return empty
        private string FixPath(string folder)
        {
            if (folder == null || folder == String.Empty)
            {
                return String.Empty;
            }
            else
            {
                if (folder.EndsWith(@"\"))
                {
                    return folder;
                }
                else
                {
                    return folder += @"\";
                }
            }
        }

        // each source file might go to N destinations
        public void PerformCopy()
        {
            if (this.filesToCopy.Count > 0)
            {
                foreach (VRASCopyOperationHelper helper in this.filesToCopy)
                {
                    if (!VRASLogEvent.IsService)
                    {
                        VRASLogEvent.LogMesage(
                            VRASLogEvent.EventLogName,
                            String.Format("Processing File in Batch: {0}\n\tSource: {1}\n\tDest: {2}", this.batchSetName, helper.SourceFullString, helper.DestInfo.DestFullString),
                            System.Diagnostics.EventLogEntryType.Error,
                            Convert.ToInt32(VRAS.VRASLogEvent.EventIDs.ErrorID),
                            VRASLogEvent.EventSourceDefault);
                    }

                    try
                    {
                        if (!this.CopyFile(helper))
                        {
                            this.copyErrorOccured = true;
                            throw new Exception(String.Format("Error occured in the copy file method.\n\tSource:{0}\n\tDestination:{1}", helper.SourceFullString, helper.DestInfo.DestFullString));
                        }
                    }
                    catch (Exception ex)
                    {
                        // Failed to copy the file to all destinations
                        this.copyErrorOccured = true;

                        VRASLogEvent.LogMesage(
                            VRASLogEvent.EventLogName,
                            String.Format("Error Copying File: Batch:{0}\n\tError:{1}\n\tSource:{2}\n\tDest:{3}", ex.Message, this.batchSetName, helper.SourceFullString, helper.DestInfo.DestFullString),
                            System.Diagnostics.EventLogEntryType.Error,
                            Convert.ToInt32(VRAS.VRASLogEvent.EventIDs.FileCopyError),
                            VRASLogEvent.EventSourceDefault);
                    }
                }
                
                try
                {
                    // See if we need to delete this file
                    if (this.moveFiles && !this.copyErrorOccured)
                    {
                        // attempt to remove the first source
                        File.Delete(this.filesToCopy[0].SourceFullString);
                    }
                }
                catch (Exception ex)
                {
                    VRASLogEvent.LogMesage(
                        VRASLogEvent.EventLogName,
                        String.Format("Error Deleting Source File After the File was copied to each Destination.  This file will be copied again the next batch run.\n\tBatch: {0}\n\tSource: {1}.\n\tError: {2}", this.batchSetName, this.filesToCopy[0].SourceFullString, ex.Message),
                        System.Diagnostics.EventLogEntryType.Error,
                        Convert.ToInt32(VRAS.VRASLogEvent.EventIDs.ErrorID),
                        VRASLogEvent.EventSourceDefault);
                }
            }
        }
        
        private bool CopyFile(VRASCopyOperationHelper helper)
        {
            bool success = false;
            int retries = 0;

            string tempFile = helper.DestInfo.DestFullString + this.tempFileExt;

            // Try to copy the file N times acording to the config
            try
            {
                // make sure the file is still there
                if (!File.Exists(helper.SourceFullString))
                {
                    this.copyErrorOccured = true;
                    throw new Exception(String.Format("Discovered File: {0} is missing before copy operations", helper.SourceFullString));
                }

                FileInfo source = new FileInfo(helper.SourceFullString);

                // attempt to copy the file N times
                while (retries <= this.copyRetries && !success)
                {
                    try
                    {
                        // make sure the dir is there
                        if (!Directory.Exists(helper.DestInfo.DestPathOnly))
                        {
                            Directory.CreateDirectory(helper.DestInfo.DestPathOnly);
                        }

                        if (File.Exists(helper.DestInfo.DestFullString))
                        {
                            if (CryptoHashCompare(helper.SourceFullString, helper.DestInfo.DestFullString))
                            {
                                // Same file
                                success = true;
                                break;
                            }
                            else
                            {
                                // Overwrite the file so delete it first
                                File.Delete(helper.DestInfo.DestFullString);
                            }
                        }

                        // copy the file if the file is still there
                        if (File.Exists(helper.SourceFullString))
                        {
                            source.CopyTo(tempFile, true);
                        }

                        // Move the temp file to final destination
                        if (File.Exists(tempFile))
                        {
                            File.Move(tempFile, helper.DestInfo.DestFullString);
                        }
                        else
                        {
                            throw new Exception("Move file Failed for " + tempFile + " and " + helper.DestInfo.DestFullString + " in CopySet: " + this.batchSetName);
                        }

                        // does a hash compare need to be done?
                        if (helper.HashCompare)
                        {
                            if (!CryptoHashCompare(helper.SourceFullString, helper.DestInfo.DestFullString))
                            {
                                throw new Exception("Backup HASH Validation Failed for " + helper.SourceFullString + " and " + helper.DestInfo.DestFullString + " in CopySet: " + this.batchSetName);
                            }
                        }

                        success = true;
                    }
                    catch (Exception ex)
                    {
                        if (retries >= this.copyRetries)
                        {
                            this.copyErrorOccured = true;
                            throw new Exception(String.Format("Error with copy operation: {0}. Source: {1}. Destination: {2}.", ex.Message, source.FullName, helper.DestInfo.DestFullString));
                        }
                        else
                        {
                            // sleep before retry
                            retries++;
                            Thread.Sleep(this.copyRetrySleepMs);
                        }
                    }
                }
            }
            catch
            {
                try
                {
                    // attempt to clean the file off if we failed in the copy or has validation.
                    if (File.Exists(helper.DestInfo.DestFullString))
                    {
                        File.Delete(helper.DestInfo.DestFullString);
                    }

                    this.copyErrorOccured = true;
                    this.errorString = String.Format("In Error condition.  Needed to remove the destintion file {0}", helper.DestInfo.DestFullString);
                }
                catch
                {
                }

                // still want to throw the exception
                throw;
            }
            finally
            {
                try
                {
                    // Cleanup temp file on best efforts
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch
                {
                }
            }

            return success;
        }

        private static string GetChecksum(string file)
        {
            object lockObj = new object();
            lock (lockObj)
            {
                using (FileStream stream = File.OpenRead(file))
                {
                    SHA256Managed sha = new SHA256Managed();
                    byte[] checksum = sha.ComputeHash(stream);
                    return BitConverter.ToString(checksum).Replace("-", String.Empty);
                }
            }
        }

        public static bool CryptoHashCompare(string file1, string file2)
        {
            object lockObj = new object();
            lock (lockObj)
            {
                try
                {
                    string hash1 = GetChecksum(file1);
                    string hash2 = GetChecksum(file2);
                    return hash1 == hash2;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error trying to make the Hashes: " + ex.Message);
                }
            }
        }
    }
}
