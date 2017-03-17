using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace Microsoft.Live.Safety.Tools.VRAS
{
    public class Batch : BatchSettingsType
    {
        private bool hasRightsToDestFolders;
        private bool hasRightsToSourceFolders;
        private bool passedValidation;
        
        private CopyJobOptionsType copyJob;
        private List<SourceType> sourceFolders;
        private List<DestinationType> destFolders;
        private List<VRASThreadManager> copyThreadPools;
        private GlobalSettings globalsettings;
        private BatchSettingsType batchSettings;
        private bool copyFromFirstDestinationDir;
        private string copyRetryCount;
        private string retrySleepMs;
        private string sleepBetweenBatchesMs;

        public Batch(GlobalSettings g, BatchSettingsType b)
        {
            BatchName = b.BatchName;
            this.globalsettings = g;
            this.batchSettings = b;

            this.sourceFolders = new List<SourceType>();
            this.destFolders = new List<DestinationType>();
            this.copyThreadPools = new List<VRASThreadManager>();

            this.copyJob = new CopyJobOptionsType();

            this.hasRightsToDestFolders = false;
            this.hasRightsToSourceFolders = false;
            this.passedValidation = false;

            // if there were no batch overridable settings use the globals
            this.batchSettings.BatchSettingOverrides = this.SetSettings(g.GlobalOverridableSettings, b.BatchSettingOverrides);

            this.copyFromFirstDestinationDir = this.batchSettings.BatchSettingOverrides.CopyFromFirstDestinationDir;
            this.copyRetryCount = this.batchSettings.BatchSettingOverrides.CopyRetryCount;
            this.retrySleepMs = this.batchSettings.BatchSettingOverrides.RetrySleepMs;
            this.sleepBetweenBatchesMs = this.batchSettings.BatchSettingOverrides.SleepBetweenBatchesMs;
        }

        private SettingThatAreOverridableType SetSettings(SettingThatAreOverridableType global, SettingThatAreOverridableType current)
        {
            if (current == null)
            {
                return global;
            }
            else
            {
                return current;
            }
        }

        public void AddSource(SourceType source)
        {
            // figure out what settings to use
            source.SourceSettingOverrides = this.SetSettings(this.globalsettings.GlobalOverridableSettings, source.SourceSettingOverrides);

            // add the source
            this.sourceFolders.Add(source);              
        }

        private bool CheckModifyAccess(DirectoryInfo directory)
        {
            try
            {
                WindowsIdentity user = System.Security.Principal.WindowsIdentity.GetCurrent();
                WindowsPrincipal p = new WindowsPrincipal(new WindowsIdentity(user.Token));

                // WindowsPrincipal principal = (WindowsPrincipal)System.Threading.Thread.CurrentPrincipal;
                // Get the collection of authorization rules that apply to the current directory
                AuthorizationRuleCollection acl = directory.GetAccessControl().GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));

                // These are set to true if either the allow read or deny read access rights are set
                bool allowModify = false;
                bool denyModify = false;

                for (int x = 0; x < acl.Count; x++)
                {
                    FileSystemAccessRule currentRule = (FileSystemAccessRule)acl[x];

                    // If the current rule applies to the current user
                    if (user.User.Equals(currentRule.IdentityReference) || p.IsInRole((SecurityIdentifier)currentRule.IdentityReference))
                    {
                        if (currentRule.AccessControlType.Equals(AccessControlType.Deny))
                        {
                            if ((currentRule.FileSystemRights & FileSystemRights.FullControl) == FileSystemRights.FullControl || (currentRule.FileSystemRights & FileSystemRights.FullControl) == FileSystemRights.Modify)
                            {
                                denyModify = true;
                            }
                        }
                        else if (currentRule.AccessControlType.Equals(AccessControlType.Allow))
                        {
                            if ((currentRule.FileSystemRights & FileSystemRights.Modify) == FileSystemRights.FullControl || (currentRule.FileSystemRights & FileSystemRights.Modify) == FileSystemRights.Modify)
                            {
                                allowModify = true;
                            }
                        }
                    }
                }

                if (allowModify & !denyModify)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool MakeSureHaveWriteAccessToFolder(string folder)
        {
            try
            {
                return this.CheckModifyAccess(new DirectoryInfo(folder));
            }
            catch
            {
               return false;
            }
        }

        public bool ValidateNTFSPermissionsOnSources()
        {
            try
            {
                foreach (SourceType s in this.sourceFolders)
                {
                    this.hasRightsToSourceFolders = this.MakeSureHaveWriteAccessToFolder(s.SourceFolderPath);
                    if (!this.hasRightsToSourceFolders)
                    {
                        VRASLogEvent.LogMesage(
                            VRASLogEvent.EventLogName,
                            "Error VRAS doesn not have access to Source: " + s.SourceFolderPath,
                            System.Diagnostics.EventLogEntryType.Error,
                            Convert.ToInt32(VRAS.VRASLogEvent.EventIDs.SetupError),
                            VRASLogEvent.EventSourceDefault);
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidateNTFSPermissionsOnDestinations()
        {
            try
            {
                foreach (DestinationType d in this.destFolders)
                {
                    if (!Directory.Exists(d.DestinationLocation))
                    {
                        Directory.CreateDirectory(d.DestinationLocation);
                    }

                    this.hasRightsToDestFolders = this.MakeSureHaveWriteAccessToFolder(d.DestinationLocation);
                    if (!this.hasRightsToDestFolders)
                    {
                        VRASLogEvent.LogMesage(
                            VRASLogEvent.EventLogName,
                            "Error Accessing Destination: " + d.DestinationLocation,
                            System.Diagnostics.EventLogEntryType.Error,
                            Convert.ToInt32(VRAS.VRASLogEvent.EventIDs.SetupError),
                            VRASLogEvent.EventSourceDefault);
                        this.passedValidation = false;
                        return false;
                    }
                }

                this.passedValidation = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void AddDestination(DestinationType dest)
        {
            this.destFolders.Add(dest);
        }

        public void ValidateSettings()
        {
            // disabled.  This is optional code.  Has a bug when user uses network service to run the code.
            this.passedValidation = true;
            //// check to make sure all the setting and paths are valid and mark the batch is they failed validation
            //// if the rights on the source don't exist then the Batch will not operate
            // if (ValidateNTFSPermissionsOnSources())
            //    ValidateNTFSPermissionsOnDestinations();
            // else
            //    passedValidation = false;
        }

        public void StartCopyOperations(int timesToRun)
        {
            if (this.passedValidation)
            {
                // setup the source and destinations for the copy threads
                // each source gets a thread pool
                foreach (SourceType source in this.sourceFolders)
                {
                    VRASThreadManager t = new VRASThreadManager(new VRASCopyConfiguration(this.globalsettings, this.batchSettings, source, this.destFolders), timesToRun);
                    this.copyThreadPools.Add(t);
                }

                // start the threads
                foreach (VRASThreadManager t in this.copyThreadPools)
                {
                    t.StartThreads();
                }
            }
            else
            {
                VRASLogEvent.LogMesage(
                        VRASLogEvent.EventLogName,
                        "Cannot start batch: " + this.batchSettings.BatchName + " it did not pass vailidation",
                        System.Diagnostics.EventLogEntryType.Error,
                        Convert.ToInt32(VRAS.VRASLogEvent.EventIDs.SetupError),
                        VRASLogEvent.EventSourceDefault);
            }
        }

        public bool BatchRunning()
        {
            try
            {
                bool done = true;
                foreach (VRASThreadManager t in this.copyThreadPools)
                {
                    if (!t.TimeToQuit)
                    {
                        done = false;
                    }
                }

                return done;
            }
            catch
            {
                throw;
            }
        }

        public void StopJobs()
        {
            foreach (VRASThreadManager t in this.copyThreadPools)
            {
                t.TimeToQuit = true;
            }
        }

        public void Cleanup()
        {
            foreach (VRASThreadManager t in this.copyThreadPools)
            {
                t.Dispose();
            }
        }
    }
}
