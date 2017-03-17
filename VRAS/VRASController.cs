using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Live.Safety.Tools.VRAS
{
    public class VRASController
    {
        private GlobalSettings globals;
        private List<Batch> batches;
        private bool isService;

        public VRASController(GlobalSettings settings, bool runningAsService)
        {
            this.globals = settings;
            this.batches = new List<Batch>();
            this.isService = runningAsService;
        }

        public void AddBatch(Batch b)
        {
            this.batches.Add(b);
        }

        public void ValidateBatches()
        {
            foreach (Batch b in this.batches)
            {
                b.ValidateSettings();
            }
        }

        public void StartBatches()
        {
            try
            {
                foreach (Batch b in this.batches)
                {
                    if (this.isService)
                    {
                        b.StartCopyOperations(-1);
                    }
                    else
                    {
                        b.StartCopyOperations(1);
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

        public bool DoneYet()
        {
            bool done = true;
            foreach (Batch b in this.batches)
            {
                if (!b.BatchRunning())
                {
                    done = false;
                }
            }

            return done;
        }

        public void StopAll()
        {
            foreach (Batch b in this.batches)
            {
                b.StopJobs();
            }
        }

        public void Cleanup()
        {
            foreach (Batch b in this.batches)
            {
                b.Cleanup();
            }
        }

        public int GetServiceTimeOutMs()
        {
            return System.Convert.ToInt32(this.globals.ServiceStopTimeOutSec) * 1000;
        }
    }
}
