using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.Live.Safety.Tools.VRAS
{
    public class VRASLoader
    {
        // the xsd version of the configuration
        private VRASConfigurationType vrasXsdConfig;
        private bool isService;

        // actual working version of the configuration
        public VRASLoader(bool thisIsAService)
        {
            this.vrasXsdConfig = new VRASConfigurationType();
            this.isService = thisIsAService;
        }

        public bool LoadConfiguration(string pathToConfig)
        {
            try
            {
                StreamReader str = new StreamReader(pathToConfig);

                XmlSerializer serializer = new XmlSerializer(typeof(VRASConfigurationType));

                return (this.vrasXsdConfig = (VRASConfigurationType)serializer.Deserialize(str)) != null;
            }
            catch (Exception ex)
            {
                VRASLogEvent.LogMesage(
                        VRASLogEvent.EventLogName,
                        "Error: " + ex.Message,
                        System.Diagnostics.EventLogEntryType.Error,
                        Convert.ToInt32(VRAS.VRASLogEvent.EventIDs.ErrorID),
                        VRASLogEvent.EventSourceDefault);
            
                return false;
            }
        }

        public void DisplayConfigurationToConsole()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(VRASConfigurationType));
            serializer.Serialize(Console.Out, this.vrasXsdConfig);
        }

        public VRASController InitializeConfiguration()
        {
            try
            {
                // init the controller
                VRASController vrasController = new VRASController(this.vrasXsdConfig.Settings, this.isService);

                // fill in each batch
                foreach (BatchType b in this.vrasXsdConfig.Batch)
                {
                    // make a new batch
                    Batch newBatch = new Batch(this.vrasXsdConfig.Settings, b.BatchSettings);

                    // start filling it up
                    // add the sources
                    foreach (SourceType s in b.CopyCollection.CopyCollection.SourceFolder)
                    {
                        newBatch.AddSource(s);
                    }

                    // sort the Tiers in the archive section
                    foreach (DestinationType d in b.CopyCollection.CopyCollection.DestinationFolder)
                    {
                        d.ArchiveSettings.SortLines();
                        newBatch.AddDestination(d);
                    }

                    // now we can add the batch
                    vrasController.AddBatch(newBatch);
                }

                return vrasController;
            }
            catch
            {
                return null;
            }
        }
    }
}
