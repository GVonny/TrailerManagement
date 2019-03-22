using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrailerManagement.Models
{
    public class IndividualWorkstation
    {
        List<int?> A;
        List<int?> B;
        List<int?> SIX;
        int? TotalA = 0;
        int? TotalB = 0;
        int? TotalSix = 0;

        public IndividualWorkstation()
        {
        }

        public void SetA(List<int?> a)
        {
            A = a;
        }

        public void SetB(List<int?> b)
        {
            B = b;
        }

        public void SetSix(List<int?> six)
        {
            SIX = six;
        }

        public List<int?> GetA()
        {
            return A;
        }

        public List<int?> GetB()
        {
            return B;
        }

        public List<int?> GetSIX()
        {
            return SIX;
        }

        public void CalculateTotalA()
        {
            foreach(int? a in A)
            {
                TotalA += a;
            }
        }
        public void CalculateTotalB()
        {
            foreach (int? b in B)
            {
                TotalB += b;
            }
        }
        public void CalculateTotalSix()
        {
            foreach (int? six in SIX)
            {
                TotalSix += six;
            }
        }
    }
}