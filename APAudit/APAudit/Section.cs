﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APAudit
{
    public class Section
    {
        public string Name { get; set; }
        public List<SectionItem> Items { get; set; }

        public Section(string name)
        {
            Name = name;
            Items = new List<SectionItem>();
        }

        /// <summary>
        /// Adds the item to the List of SectionItems.  If the item is there it will update it on the value
        /// </summary>
        /// <param name="item"></param>
        public void UpsertSectionItem(SectionItem item)
        {
            var itemFound = GetSectionItem(item.Name);

            if (itemFound != null)
            {
                itemFound.Value = item.Value;
            }
            else
            {
                Items.Add(item);
            }
        }

        /// <summary>
        /// Will return a SectionItem if the passed in name matches an existing item.  Else return null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SectionItem GetSectionItem(string name)
        {
            return (from i in this.Items
                             where i.Name == name
                             select i).FirstOrDefault();
        }

        public Dictionary<string, string> ReturnItemsAsDictionary()
        {
            Dictionary<string, string> temp = new Dictionary<string, string>();
            foreach (var item in Items)
            {
                temp.Add(item.Name, item.Value);
            }
            return temp;
        }

        /// <summary>
        /// Takes in another section and compairs its values for each key agaist the calling Section.
        /// If the section names don't match then throws an exception
        /// </summary>
        /// <param name="other"></param>
        /// <returns> A new Section with all the results for all comparisons</returns>
        public Section CompareThisAgainst(Section other)
        {
            if (this.Name != other.Name)
            {
                throw new Exception(string.Format("Can't compare two sections with different names.  You passed in {1} and {0}", other.Name, this.Name));
            }
            Section result = new Section(this.Name);

            var sourceDict = this.ReturnItemsAsDictionary();
            var compDict = other.ReturnItemsAsDictionary();
            foreach (var key in sourceDict.Keys)
            {
                // same key
                if (compDict.ContainsKey(key))
                {
                    if (sourceDict[key] == compDict[key])
                    {
                        result.Items.Add(new SectionItem(key, sourceDict[key], SectionItemStatus.ok));
                    }
                    else // source and compare keys don't match
                    {
                        result.Items.Add(new SectionItem(key, sourceDict[key], SectionItemStatus.invalid));
                    }
                }
                else //extra key in source
                {
                    result.Items.Add(new SectionItem(key, sourceDict[key], SectionItemStatus.extra));
                }
            }

            // now just look at the extra keys in the other that are not in the source
            foreach (var key in compDict.Keys)
            {
                // missing key in the source
                if (!sourceDict.ContainsKey(key))
                {
                    result.Items.Add(new SectionItem(key, compDict[key], SectionItemStatus.missing));
                }
            }
            return result;
        }
    }
}
