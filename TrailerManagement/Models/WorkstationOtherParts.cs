using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrailerManagement.Models
{
    public class WorkstationOtherParts
    {
        List<string> PartNumbers;
        List<int> PartQuantities;

        public WorkstationOtherParts()
        {
            PartNumbers = new List<string>();
            PartQuantities = new List<int>();
        }

        public void AddParts(List<string> parts)
        {
            foreach(String part in parts)
            {
                if(!PartNumbers.Contains(part))
                {
                    PartNumbers.Add(part);
                    PartQuantities.Add(0);
                }
            }
        }

        public void AddQuantities(List<string> parts, List<int> quantities)
        {
            for(int x = 0; x < parts.Count; x++)
            {
                if(PartNumbers.Contains(parts[x]))
                {
                    var index = PartNumbers.IndexOf(parts[x]);
                    PartQuantities[index] += quantities[x];
                }
            }
        }

        public List<string> GetParts()
        {
            return PartNumbers;
        }

        public List<int> GetQuantities()
        {
            return PartQuantities;
        }
    }
}