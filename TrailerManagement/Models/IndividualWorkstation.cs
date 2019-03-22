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
        int TotalA;
        int TotalB;
        int TotalSix;

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
    }
}