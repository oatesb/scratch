using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json;

namespace CheckLatestBuild
{
    class Program
    {
        static void Main(string[] args)
        {

            //string action = "WriteCurrentBuild";
            //string action = "WriteLastCompletedBuild";
            string action;
            string configPath;
            // get the default config if there are no params for config file name passed in.
            if(args.Length == 1)
            {
                action = args[0];
                configPath = ConfigurationManager.AppSettings["defaultConfigFile"];
            }
            else if (args.Length == 2)
            {
                action = args[0];
                configPath = args[1];
            }
            else
            {
                action = "";
                configPath = "";
                Console.WriteLine("Invalid Arguments: must be 1 or 2 items <action> <(optional) configpath>");
                Environment.Exit(5);
            }

            if (!action.Equals("WriteCurrentBuild", StringComparison.CurrentCultureIgnoreCase) && !action.Equals("WriteLastCompletedBuild", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine("Action must be {0} or {1}", "WriteCurrentBuild", "WriteLastCompletedBuild");
                Environment.Exit(5);
            }

            if (!TestPath(configPath, "file"))
            {
                Console.WriteLine("You passed in an invalid path to config file: {0}", configPath);
                Environment.Exit(5);
            }

            // prime the configs
            LatestBuildSettings settings = JsonConvert.DeserializeObject<LatestBuildSettings>(File.ReadAllText(configPath));

            if (action.ToLower() == "writecurrentbuild")
            {
                SetupCurrentBuild(settings);
            }
            else if (action.ToLower() == "writelastcompletedbuild")
            {
                WriteLastCompletedBuild(settings.outputPath);
            }
            else
            {

            }

            Console.Read();

        }

        /// <summary>
        /// Will find the latest build number then write the contents to disk of the build number. It will store
        /// the CurrentRunningBuild to equal empty and the LastComplete to be the CurrentRunningBuild value
        /// </summary>
        /// <returns></returns>
        public static void WriteLastCompletedBuild(string outputPath)
        {
            try
            {
                BuildStatus status;
                if (File.Exists(outputPath))
                {
                    
                    status = JsonConvert.DeserializeObject<BuildStatus>(File.ReadAllText(outputPath));
                    if (!string.IsNullOrEmpty(status.CurrentRunningBuild))
                    {
                        status.LastCompleteBuild = status.CurrentRunningBuild;
                        status.CurrentRunningBuild = string.Empty;

                        Console.WriteLine("Updating the LastCompleteBuild : {0} and setting CurrentRunningBuild to be empty", status.LastCompleteBuild);
                        File.WriteAllText(outputPath, JsonConvert.SerializeObject(status, Formatting.Indented));
                    }
                    else
                    {
                        Console.WriteLine("Could not update the LastCompleteBuild as the CurrentRunningBuild was Empty");
                    }
                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Will find the current build on the network.  Then it will determine if that build is newer than the last run.
        /// if newer then it will update the network file and exit normally.  If the build is the same then it will just exit
        /// with an error code from the entire application.
        /// </summary>
        /// <returns></returns>
        public static void SetupCurrentBuild(LatestBuildSettings settings)
        {
            try
            {
                // get the current build
                var lastBuild = GetLastestBuildNumber(settings);

                // update the files if needed.
                BuildStatus status;
                if (File.Exists(settings.outputPath))
                {
                    status = JsonConvert.DeserializeObject<BuildStatus>(File.ReadAllText(settings.outputPath));
                    if (BuildFirstGreaterThanSecondByStringName(lastBuild, status.LastCompleteBuild))
                    {
                        status.CurrentRunningBuild = lastBuild;
                        File.WriteAllText(settings.outputPath, JsonConvert.SerializeObject(status, Formatting.Indented));
                        Console.WriteLine("output file found setting the CurrentRunningBuild to {0} which is newer than {1}", lastBuild, status.LastCompleteBuild);
                    }
                    else
                    {
                        // exit with error code if the version didn't increase.
                        Console.WriteLine("The build {0} is the same or less than {1}", lastBuild, status.LastCompleteBuild);
                        Environment.Exit(1);
                    }
                    
                }
                else // the network file wasn't there.  So fil it out and let the build number be assumed to be newer
                {
                    status = new BuildStatus();
                    status.CurrentRunningBuild = lastBuild;
                    File.WriteAllText(settings.outputPath, JsonConvert.SerializeObject(status, Formatting.Indented));
                    Console.WriteLine("output file not found creating {0} and setting the CurrentRunningBuild to {1}", settings.outputPath, lastBuild);
                }

                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Reads the string contents from the file passed in.  If the file is empty or doesn't exist then BuildStatus object is returned.
        /// </summary>
        /// <param name="outputPath"></param>
        /// <returns>
        /// string of the txt file contents
        /// </returns>
        public static BuildStatus ReadLastBuildStatus(string outputPath)
        {
            try
            {
                var status = JsonConvert.DeserializeObject<BuildStatus>(File.ReadAllText(outputPath));
                return status;
            }
            catch (Exception)
            {
                Console.WriteLine("Last output file not found returning empty status object: {0}", outputPath);
                return new BuildStatus();
            }
        }

        public static string GetLastestBuildNumber(LatestBuildSettings settings)
        {
            //var dirs = Directory.EnumerateDirectories(rootPath,"*",SearchOption.TopDirectoryOnly);
            var dirInfo = new DirectoryInfo(settings.rootPath);
            var dirs = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);

            IEnumerable<DirectoryInfo> dirsInOrder;
            if (settings.orderType.ToLower() == "date")
            {
                dirsInOrder = dirs.OrderByDescending(d => d.CreationTime).Take(10);
            }
            else
            {
                dirsInOrder = dirs.OrderByDescending(d => d.Name).Take(10);
            }

            Console.WriteLine("++++++++ {0} Order +++++++++++", settings.orderType);

            Console.WriteLine("Testing each build until we find the latest good build");

            bool done = false;
            DirectoryInfo theCurrentBuild = null;
            foreach (var item in dirsInOrder)
            {
                if (!done)
                {
                    // check each of the items in the collection
                    if (CheckAllValidations(settings.validations, item.FullName))
                    {
                        done = true;
                        theCurrentBuild = item;
                    }
                }

            }

            if (theCurrentBuild == null)
            {
                throw new Exception(string.Format("There are no good builds under root path: {0}", settings.rootPath));
            }
            Console.WriteLine("Last Good Build: {0}", theCurrentBuild.Name);

            return theCurrentBuild.Name;

            //if (File.Exists(outputPath))
            //{
            //    var lastBuild = File.ReadAllText(outputPath);

            //    var result = BuildFirstGreaterThanSecondByStringName(selectedBuild.Name, lastBuild);

            //    if (result)
            //    {
            //        File.WriteAllText(outputPath, selectedBuild.Name);
            //    }
            //}
            //else
            //{
            //    File.WriteAllText(outputPath, selectedBuild.Name);
            //}
        }

        public static string CombinePath(string root, string tail)
        {
            if (!root.EndsWith(@"\"))
            {
                root += @"\";
            }
            if (tail.StartsWith(@"\"))
            {
                tail = tail.Substring(1, tail.Length -1);
            }
            return root + tail;
        }
        public static bool CheckAllValidations(List<LatestBuildValidation> validations, string rootPath)
        {
            bool success = true;
            foreach (var item in validations)
            {
                if (item.type.ToLower() == "file")
                {
                    if (!File.Exists(CombinePath(rootPath, item.pathFromRoot)))
                    {
                        success = false;
                        Console.WriteLine("\tFile: {0} is not found is not a valid build", CombinePath(rootPath, item.pathFromRoot));
                    }

                }
                if (item.type.ToLower() == "directory")
                {
                    if (!Directory.Exists(CombinePath(rootPath, item.pathFromRoot)))
                    {
                        success = false;
                        Console.WriteLine("\tDirectory: {0} is not found is not a valid build", CombinePath(rootPath, item.pathFromRoot));
                    }

                }
            }
            return success;
        }


        /// <summary>
        /// Returns bool if the string alphabetically name if the first string passed in is larger and the second string.
        /// Equal is false result as well.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool BuildFirstGreaterThanSecondByStringName(string first, string second)
        {
            if (string.IsNullOrEmpty(first))
            {
                return false;
            }
            if (string.IsNullOrEmpty(second))
            {
                return true;
            }
            var result = first.CompareTo(second);
            if (result < 0)
            {
                return false;
            }
            else if (result >= 1)
            {
                return true;
            }
            else // then they are =
            {
                return false;
            }
        }


        /// test the path of a file or directory
        /// type must be file or directory or it throws an exception
        /// returns bool on existance
        public static bool TestPath(string path, string type)
        {
            switch (type.ToLower())
            {
                case "directory":
                    return Directory.Exists(path);
                case "file":
                    return File.Exists(path);
                default:
                    throw new InvalidOperationException(string.Format("Must pass in \"file\" or \"directory\" as param to TestPath.  You passed in {0}.", type));
            }
        }


    }
}
