using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.Live.Safety.Tools.VRAS
{
    public sealed class VRASThreadManager : IDisposable
    {
        private bool timeToQuit;
        private Semaphore workWaiting;
        private Queue<WaitQueueItem> queue;
        private List<Thread> threads;
        private Thread workFinder;
        private VRASCopyConfiguration copyConfig;
        private int timesToRunTheWorkFinder;
        private int workingCopyThreads;

        public VRASThreadManager(VRASCopyConfiguration config, int timesToRun)
        {
            if (config.CopyThreadPerSource <= 0)
            {
                throw new ArgumentOutOfRangeException("copyThreadPerSource");
            }

            this.copyConfig = config;
            this.timeToQuit = false;
            this.threads = new List<Thread>(this.copyConfig.CopyThreadPerSource);
            this.queue = new Queue<WaitQueueItem>();
            this.workWaiting = new Semaphore(0, int.MaxValue);
            this.timesToRunTheWorkFinder = timesToRun;
            this.workingCopyThreads = 0;
        }

        // sets or gets the flag on if the application is ready to dispose the threads
        public bool TimeToQuit { get { return this.timeToQuit; } set { this.timeToQuit = value; } }

        // cleanup go through the copy threads and file finder thread and make them stop.
        public void Dispose()
        {
            this.timeToQuit = true;
            if (this.threads != null)
            {
                this.threads.ForEach(delegate(Thread t)
                {
                    t.Interrupt();
                });
                this.threads = null;
            }

            if (this.workFinder != null)
            {
                this.workFinder.Interrupt();
            }
        }

        // start the threads to look at the queue for files to copy and start the thread to find files to copy
        public void StartThreads()
        {
            // start the work finder thread
            this.workFinder = new Thread(this.RunJobFinder);
            this.workFinder.IsBackground = true;
            this.workFinder.Start();

            // start the worker threads
            for (int i = 0; i < this.copyConfig.CopyThreadPerSource; i++)
            {
                Thread t = new Thread(this.RunWorker);
                t.IsBackground = true;
                this.threads.Add(t);
                t.Start();
            }
        }

        // interface to add a new item to the copy queue
        private void QueueUserWorkItem(WaitCallback callback, object state)
        {
            try
            {
                if (this.threads == null)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                if (callback == null)
                {
                    throw new ArgumentNullException("callback");
                }

                WaitQueueItem item = new WaitQueueItem();
                item.Callback = callback;
                item.State = state;
                item.Context = ExecutionContext.Capture();

                lock (this.queue)
                {
                    this.queue.Enqueue(item);
                }
            }
            finally
            {
                this.workWaiting.Release();
            }
        }

        // method the thread uses to find new files to copy
        private void RunJobFinder()
        {
            int timesRun = 0;
            while (!this.timeToQuit)
            {
                lock (this.queue)
                {
                    if (this.queue.Count <= 0 && this.workingCopyThreads == 0)
                    {
                        try
                        {
                            List<VRASCopyBlob> blobList = VRASCopyBlob.GetFiles(this.copyConfig);
                            foreach (VRASCopyBlob copyJob in blobList)
                            {
                                // need to make the workitems for the blob to know what to copy
                                copyJob.PopulateCopyWork();
                                this.QueueUserWorkItem(new WaitCallback(copyJob.DoWork), new object());
                            }
                        }
                        catch (Exception ex)
                        {
                            VRASLogEvent.LogMesage(
                            VRASLogEvent.EventLogName,
                            "Error while getting files: " + ex.Message + "...Worker Thread to find files is going to sleep for its scheduled duration.",
                            System.Diagnostics.EventLogEntryType.Error,
                            Convert.ToInt32(VRAS.VRASLogEvent.EventIDs.FileCopyError),
                            VRASLogEvent.EventSourceDefault);
                        }
                    }
                }

                // if it is a service increase the run count
                if (this.timesToRunTheWorkFinder != -1)
                {
                    timesRun++;
                }
                else 
                {
                    // sleep if it is a service (-1) times to run
                    this.workWaiting.WaitOne(this.copyConfig.SleepBetweenBatchesMs);
                }

                // don't sleep if there are no more runs to do
                if ((this.timesToRunTheWorkFinder - timesRun) > 0)
                {
                    this.workWaiting.WaitOne(this.copyConfig.SleepBetweenBatchesMs);
                }
                    
                // see if it ran enough times -1 means forever.  Then we can set the flag to quit to true.  If we don't wait then the copy threads will all want to quit now.
                if (this.timesToRunTheWorkFinder != -1 && timesRun >= this.timesToRunTheWorkFinder)
                {
                    // wait for the queue to drain
                    bool stillWaiting = true;
                    int count;
                    while (stillWaiting)
                    {
                        lock (this.queue)
                        {
                            count = this.queue.Count;
                        }

                        if (count > 0)
                        {
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            stillWaiting = false;
                        }
                    }

                    this.timeToQuit = true;
                }
            }
        }
     
        // method the copy threads use to copy each file
        private void RunWorker()
        {
            try
            {
                while (!this.timeToQuit)
                {
                    WaitQueueItem item = null;
                    lock (this.queue)
                    {
                        if (this.queue.Count > 0)
                        {
                            item = this.queue.Dequeue();
                        }
                    }

                    // only run if valid and else if means running multple times from a command line >1 or it is set to service (-1)
                    if (item != null)
                    {
                        this.workingCopyThreads++;
                        ExecutionContext.Run(item.Context, new ContextCallback(item.Callback), item.State);
                        this.workingCopyThreads--;
                    }
                    else if (this.timesToRunTheWorkFinder > 1 || this.timesToRunTheWorkFinder == -1)
                    {
                        this.workWaiting.WaitOne(this.copyConfig.SleepBetweenBatchesMs);
                    }
                }
            }
            catch (Exception ex) 
            {
                VRASLogEvent.LogMesage(
                        VRASLogEvent.EventLogName,
                        String.Format("Error with copy thread: {0}", ex.Message),
                        System.Diagnostics.EventLogEntryType.Error,
                        Convert.ToInt32(VRAS.VRASLogEvent.EventIDs.FileCopyError),
                        VRASLogEvent.EventSourceDefault);
            }
        }

        // helper class
        private class WaitQueueItem
        {
            public WaitCallback Callback { get; set; }

            public object State { get; set; }
            
            public ExecutionContext Context { get; set; }
        }
    }
}
