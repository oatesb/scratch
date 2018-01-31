using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Text.RegularExpressions;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {


            List<string> whitelist = new List<string>();
            whitelist.Add("SmrtSvc");
            whitelist.Add("Safety");

            string[] services= { "FooSafety", "SafetyFoo", "safetyfoo", "nothing",  "SmrtSvcGo" };

            foreach (var svc in services)
            {
                if (whitelist.Any(w=> svc.ToLower().Contains(w.ToLower())))
                {
                    Console.WriteLine(svc);
                }

            }

            Console.WriteLine("-----------");
            //var pass = services.Where(svc => whitelist.Any(w => svc.ToLower().Contains(w.ToLower())));
            var pass = services.Where(svc => whitelist.Any(w => Regex.IsMatch(svc, w, RegexOptions.IgnoreCase)));

            foreach (var item in pass)
            {
                Console.WriteLine(item);

            }


            Environment.Exit(0);

            int[] arr = new int [] { 2, 7, 11, 15 };

            var t = TwoSum2(arr, 9);
            Console.WriteLine("{0} {1}",t[0], t[1]);

            t = TwoSum2(arr, 26);
            Console.WriteLine("{0} {1}", t[0], t[1]);
            t = TwoSum2(arr, 17);
            Console.WriteLine("{0} {1}", t[0], t[1]);
            t = TwoSum2(arr, 9232);
            Console.WriteLine("{0} {1}", t[0], t[1]);

            Console.Read();


        }

        public static int[] TwoSum(int[] nums, int target)
        {

            for (int i = 0; i < nums.Length; i++)
            {
                for (int j = i; j < nums.Length; j++)
                {
                    if (i != j)
                    {
                        if (nums[i] + nums[j] == target)
                        {
                            return new int[] { i, j };
                        }
                    }
                }
            }
            return new int[] { -1, -1};
        }

        public static int[] TwoSum2(int[] nums, int target)
        {
            Hashtable map = new Hashtable();
            for (int i = 0; i < nums.Length; i++)
            {
                int complement = target - nums[i];
                if (map.ContainsKey(complement))
                {
                    return new int[] { (int)map[complement], i };
                }
                map.Add(nums[i], i);
            }
            throw new ArgumentException("No two sum solution");
        }
    }
}
