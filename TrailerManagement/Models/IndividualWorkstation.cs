using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrailerManagement.Models
{
    public class IndividualWorkstation
    {
        List<int> A;
        List<int> B;
        List<int> SIX;
        List<int> OtherPartQuantities;
        List<string> OtherPartNumbers;
        int TotalA = 0;
        int TotalB = 0;
        int TotalSix = 0;
        int TotalOther = 0;
        int GrandTotal = 0;

        public IndividualWorkstation()
        {
            A = new List<int>();
            B = new List<int>();
            SIX = new List<int>();
            OtherPartNumbers = new List<string>();
            OtherPartQuantities = new List<int>();
            TotalA = 0;
            TotalB = 0;
            TotalSix = 0;
        }

        public void SetA(List<int> a)
        {
            A = a;
        }

        public List<int> GetA()
        {
            return A;
        }

        public void SetB(List<int> b)
        {
            B = b;
        }

        public List<int> GetB()
        {
            return B;
        }

        public void SetSix(List<int> six)
        {
            SIX = six;
        }

        public List<int> GetSIX()
        {
            return SIX;
        }

        public void CalculateTotalA()
        {
            foreach(int a in A)
            {
                TotalA += a;
            }
        }
        public void CalculateTotalB()
        {
            foreach (int b in B)
            {
                TotalB += b;
            }
        }
        public void CalculateTotalSix()
        {
            foreach (int six in SIX)
            {
                TotalSix += six;
            }
        }

        public void CalculateTotalOther()
        {
            foreach(int other in OtherPartQuantities)
            {
                TotalOther += other;
            }
        }

        public void CalculateGrandTotal()
        {
            GrandTotal += TotalA;
            GrandTotal += TotalB;
            GrandTotal += TotalSix;
            GrandTotal += TotalOther;
        }
        
        public void AddToOtherList(string partNumber, int quantity)
        {
            OtherPartNumbers.Add(partNumber);
            OtherPartQuantities.Add(quantity);
        }

        public List<int> GetOther()
        {
            return OtherPartQuantities;
        }

        public List<string> GetParts()
        {
            return OtherPartNumbers;
        }

        public int getGrandTotal()
        {
            return GrandTotal;
        }

        public int getTotalA()
        {
            return TotalA;
        }

        public int getTotalB()
        {
            return TotalB;
        }

        public int getTotalSix()
        {
            return TotalSix;
        }

        public int getTotalOther()
        {
            return TotalOther;
        }
    }
}