using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Microsoft.Live.Safety.Tools.VRAS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string defaultConfig = @"VRASConfiguration.xml";

            if (args.Length > 0 && args[0].ToLower().Contains("cmd"))
            {
                if (args.Length > 1)
                {
                    if (args[1] != String.Empty)
                    {
                        Work.DocommandLineWork(args[1].ToString());
                    }

                    Console.ReadLine();
                }
                else
                {
                    Work.DocommandLineWork(defaultConfig);
                }
            }
            else
            {
                string name = "VRASCopyService";
                if (args.Length > 0)
                {
                    name = args[0].ToString();
                }

                ServiceBase.Run(new WindowsService(name));
            }
        }
    }

    public class Work
    {
        public static void DocommandLineWork(string configFile)
        {
            try
            {
                VRASLogEvent.SetIsService(false);

                // to test as a service would run run this line
                // VRASLoader loader = new VRASLoader(true);
                VRASLoader loader = new VRASLoader(false);
                VRASController controler;

                // if the config is valid do some work
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                if (loader.LoadConfiguration(configFile))
                {
                    controler = loader.InitializeConfiguration();

                    controler.ValidateBatches();

                    controler.StartBatches();

                    // wait for the batch to get done
                    while (!controler.DoneYet())
                    {
                        Thread.Sleep(5000);
                    }

                    controler.Cleanup();
                }
                else
                {
                    throw new Exception("Error loading config file");
                }
            }
            catch (Exception ex)
            {
                VRASLogEvent.LogMesage(
                        VRASLogEvent.EventLogName,
                        String.Format("Error: {0}", ex.Message),
                        System.Diagnostics.EventLogEntryType.Error,
                        Convert.ToInt32(VRAS.VRASLogEvent.EventIDs.FileCopyError),
                        VRASLogEvent.EventSourceDefault);
            }
        }
    }
}
