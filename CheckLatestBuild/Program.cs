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
            string configPath;
            // get the default config if there are no params for config file name passed in.
            if(args.Length == 0)
            {
                configPath = ConfigurationManager.AppSettings["defaultConfigFile"];
            }
            else
            {
                configPath = args[0];
            }

            if (!TestPath(configPath, "file"))
            {
                Console.WriteLine("You passed in an invalid path to config file: {0}", configPath);
                Environment.Exit(5);
            }

            // prime the configs
            LatestBuildSettings settings = JsonConvert.DeserializeObject<LatestBuildSettings>(File.ReadAllText(configPath));

            

            Console.Read();

        }

        /// <summary>
        /// Will find the latest build number then write the contents to disk of the build number
        /// </summary>
        /// <returns></returns>
        public static string WriteLastBuildNumber()
        {

        }

        /// <summary>
        /// Reads the string contents from the file passed in.  If the file is empty or doesn't exist then empty string is returned.
        /// </summary>
        /// <param name="outputPath"></param>
        /// <returns>
        /// string of the txt file contents
        /// </returns>
        public static string ReadLastBuildNumber(string outputPath)
        {
            try
            {
                var f = File.ReadAllText(outputPath);
                if (string.IsNullOrEmpty(f))
                {
                    return string.Empty;
                }
                else
                {
                    return f;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Last output file not found returning empty string: {0}", outputPath);
                return string.Empty;
            }
        }

        public static void CheckLastestBuildNumber()
        {
            //var dirs = Directory.EnumerateDirectories(rootPath,"*",SearchOption.TopDirectoryOnly);
            var dirInfo = new DirectoryInfo(settings.rootPath);
            var dirs = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);

            var dirsByDate = dirs.OrderByDescending(d => d.CreationTime).Take(10);

            var dirsByStringOrder = dirs.OrderByDescending(d => d.Name).Take(10);

            Console.WriteLine("++++++++ Name Order +++++++++++");
            foreach (var item in dirsByStringOrder)
            {
                Console.WriteLine(item);
            }

            Console.WriteLine("++++++++ Date Order +++++++++++");
            foreach (var item in dirsByDate)
            {
                Console.WriteLine(item);
            }

            var selectedBuild = dirs.OrderByDescending(d => d.Name).FirstOrDefault();


            Console.WriteLine("Last Build: {0}", selectedBuild.Name);

            if (File.Exists(settings.outputPath))
            {
                var lastBuild = File.ReadAllText(settings.outputPath);

                var result = BuildFirstGreaterThanSecondByStringName(selectedBuild.Name, lastBuild);

                if (result)
                {
                    File.WriteAllText(settings.outputPath, selectedBuild.Name);
                }
            }
            else
            {
                File.WriteAllText(settings.outputPath, selectedBuild.Name);
            }
        }

        /// <summary>
        /// Returns bool if the string alphabetically name if the first string passed in is larger and the second string.
        /// Equal is true result as well.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool BuildFirstGreaterThanSecondByStringName(string first, string second)
        {
            var result = first.CompareTo(second);
            if (result < 0)
            {
                return false;
            }
            else if (result > 1)
            {
                return true;
            }
            else // then they are =
            {
                return true;
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
