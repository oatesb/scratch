using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace Microsoft.Live.Safety.Tools.VRAS
{
    public class WindowsService : ServiceBase
    {
        private VRASLoader loader;
        private VRASController controler;

        public WindowsService(string name)
        {
            ServiceName = name;
            EventLog.Log = "Application";

            CanHandlePowerEvent = true;
            CanHandleSessionChangeEvent = true;
            CanPauseAndContinue = true;
            CanShutdown = true;
            CanStop = true;
        }

        /// <summary>
        /// Dispose of objects that need it here.
        /// </summary>
        /// <param name="disposing">Whether
        /// or not disposing is going on.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        /// <summary>
        /// OnStart(): Put startup code here
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            try
            {
                VRASLogEvent.SetIsService(true);
                VRASLogEvent.EventSourceDefault = "VRAS Service";

                VRASLogEvent.PrimeEventLogSource(VRASLogEvent.EventSourceDefault, VRASLogEvent.EventLogName);

                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

                // true for service
                this.loader = new VRASLoader(true);

                if (!File.Exists(VRAS.VRASLogEvent.ConfigFile))
                {
                    string txt = "Could not find the configuration file: " + VRAS.VRASLogEvent.ConfigFile;

                    VRASLogEvent.LogMesage(
                        VRASLogEvent.EventLogName,
                        txt,
                        System.Diagnostics.EventLogEntryType.Error,
                        Convert.ToInt32(VRAS.VRASLogEvent.EventIDs.SetupError),
                        VRASLogEvent.EventSourceDefault);

                    Environment.Exit(1);
                }

                // if the config is valid do some work
                if (this.loader.LoadConfiguration(VRASLogEvent.ConfigFile))
                {
                    VRASLogEvent.LogMesage(
                        VRASLogEvent.EventLogName,
                        "Configuration Loaded starting Initialization.",
                        System.Diagnostics.EventLogEntryType.Information,
                        Convert.ToInt32(VRAS.VRASLogEvent.EventIDs.Starting),
                        VRASLogEvent.EventSourceDefault);

                    this.controler = this.loader.InitializeConfiguration();
                    if (this.controler == null)
                    {
                        throw new Exception("Error initializing the configuration");
                    }

                    this.controler.ValidateBatches();
                    this.controler.StartBatches();
                }
            }
            catch (Exception ex)
            {
                VRASLogEvent.LogMesage(
                        VRASLogEvent.EventLogName,
                        "Error: " + ex.Message,
                        System.Diagnostics.EventLogEntryType.Error,
                        Convert.ToInt32(VRAS.VRASLogEvent.EventIDs.ErrorID),
                        VRASLogEvent.EventSourceDefault);
            }
        }

        /// <summary>
        /// OnStop(): Put your stop code here
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                this.controler.StopAll();

                // wait for the service timeout as defined then kill if necessary
                int timeSlept = 0;

                while (!this.controler.DoneYet() || timeSlept > this.controler.GetServiceTimeOutMs())
                {
                    timeSlept += 1000;
                    Thread.Sleep(1000);
                }

                this.controler.Cleanup();
            }
            catch (Exception ex)
            {
                VRASLogEvent.LogMesage(
                        VRASLogEvent.EventLogName,
                        "Error: " + ex.Message,
                        System.Diagnostics.EventLogEntryType.Error,
                        Convert.ToInt32(VRAS.VRASLogEvent.EventIDs.ErrorID),
                        VRASLogEvent.EventSourceDefault);
            }
            finally
            {
                base.OnStop();
            }
        }

        /// <summary>
        /// OnPause: Put your pause code here
        /// </summary>
        protected override void OnPause()
        {
            base.OnPause();
        }

        /// <summary>
        /// OnContinue(): Put your continue code here
        /// </summary>
        protected override void OnContinue()
        {
            base.OnContinue();
        }

        /// <summary>
        /// OnShutdown(): Called when the System is shutting down
        /// - Put code here when you need special handling
        ///   of code that deals with a system shutdown, such
        ///   as saving special data before shutdown.
        /// </summary>
        protected override void OnShutdown()
        {
            try
            {
                this.controler.StopAll();
            }
            catch
            {
            }
            finally
            {
                base.OnShutdown();
            }
        }

        /// <summary>
        /// OnCustomCommand(): If you need to send a command to your
        ///   service without the need for Remoting or Sockets, use
        ///   this method to do custom methods.
        /// </summary>
        /// <param name="command">Arbitrary Integer between 128 - 256</param>
        protected override void OnCustomCommand(int command)
        {
            // A custom command can be sent to a service by using this method:
            // #  int command = 128; //Some Arbitrary number between 128 & 256
            // #  ServiceController sc = new ServiceController("NameOfService");
            // #  sc.ExecuteCommand(command);
            base.OnCustomCommand(command);
        }

        /// <summary>
        /// OnPowerEvent(): Useful for detecting power status changes,
        ///   such as going into Suspend mode or Low Battery for laptops.
        /// </summary>
        /// <param name="powerStatus">The Power Broadcast Status
        /// (BatteryLow, Suspend, etc.)</param>
        /// <returns>if on power event</returns>
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return base.OnPowerEvent(powerStatus);
        }

        /// <summary>
        /// OnSessionChange(): To handle a change event
        ///   from a Terminal Server session.
        ///   Useful if you need to determine
        ///   when a user logs in remotely or logs off,
        ///   or when someone logs into the console.
        /// </summary>
        /// <param name="changeDescription">The Session Change
        /// Event that occured.</param>
        protected override void OnSessionChange(
                    SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);
        }
    }
}
