using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APAudit
{
    public static class Utils
    {
        public static bool YesNoQuestion(string question)
        {
            string answer = string.Empty;
            do
            {
                Console.Write(question);
                answer = Console.ReadLine();

            } while (answer != "n" && answer != "y");

            answer = answer == "n" ? "false" : "true";
            return System.Convert.ToBoolean(answer);
        }
    }

}
