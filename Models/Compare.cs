using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ColecticaSdkMvc.Models
{
    public class Compare
    {
        public string String1 { get; set; }
        public string String2 { get; set; }

        public bool OneWordMatches(string string1, string string2)
        {
            if (String1 != null && String2 != null)
            {
                return string1.Split().Intersect(string2.Split()).Any();
            }
            else return false;
        }

       

        public string StringCompare1(string string1, string string2)
        {
            if (string1 != null && string2 != null)
            {
                string[] list1 = string1.ToLower().Split();
                string[] list2 = string2.ToLower().Split();

                int matches = 0;
                for (int i = 0; i < list1.Count(); i++)
                {
                    bool exists = list2.Any(s => s.Contains(list1[i]));
                    if (exists)
                    {
                        matches++;
                    }
                }
                double num3 = (((double)matches / (double)list1.Count())*100);
                return (num3.ToString(("#.##")) + "%");
            }
            else return null;
        }
    }
}