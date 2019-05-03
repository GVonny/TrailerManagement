using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrailerManagement.Models
{
    public class Constants
    {
        //PERMISSIONS
        public int PERMISSION_ADMIN { get; }
        public int PERMISSION_MANAGER { get; }
        public int PERMISSION_EDIT { get; }
        public int PERMISSION_DRIVER { get; }
        public int PERMISSION_VIEW { get; }
        
        //TRAILER STATUS
        public string TRAILER_STATUS_NEED_EMPTY { get; }
        public string TRAILER_STATUS_READY_TO_ROLL { get; }
        public string TRAILER_STATUS_LOADING { get; }
        public string TRAILER_STATUS_EMPTY { get; }
        public string TRAILER_STATUS_NEED_WORK { get; }
        public string TRAILER_STATUS_IN_TRANSIT { get; }
        public string TRAILER_STATUS_DELIVERED { get; }

        public int DEPARTMENT_ASSEMBLY { get; }
        public int DEPARTMENT_AUTOMATED_ASSEMBLY { get; }
        public int DEPARTMENT_DIRECT_SUPPLY_MANAGEMENT { get; }
        public int DEPARTMENT_PALLET_REPAIR { get; }
        public int DEPARTMENT_SAW_SHOP { get; }
        public int DEPARTMENT_MAINTANANCE { get; }
        public int DEPARTMENT_TRANSPORTATION { get; }
        public int DEPARTMENT_PURCHASING { get; }
        public int DEPARTMENT_HR_SAFETY { get; }
        public int DEPARTMENT_SALES { get; }
        public int DEPARTMENT_G_AND_A { get; }
        public int DEPARTMENT_SUPER_ADMIN { get; }
        
        
        public Constants()
        {
            PERMISSION_ADMIN = 50;
            PERMISSION_MANAGER = 40;
            PERMISSION_EDIT = 30;
            PERMISSION_DRIVER = 20;
            PERMISSION_VIEW = 10;

            TRAILER_STATUS_NEED_EMPTY = "1";
            TRAILER_STATUS_READY_TO_ROLL = "2";
            TRAILER_STATUS_LOADING = "3";
            TRAILER_STATUS_EMPTY = "4";
            TRAILER_STATUS_NEED_WORK = "5";
            TRAILER_STATUS_IN_TRANSIT = "6";
            TRAILER_STATUS_DELIVERED = "7";

            DEPARTMENT_ASSEMBLY = 1100;
            DEPARTMENT_AUTOMATED_ASSEMBLY = 1200;
            DEPARTMENT_DIRECT_SUPPLY_MANAGEMENT = 1900;
            DEPARTMENT_PALLET_REPAIR = 2100;
            DEPARTMENT_SAW_SHOP = 3100;
            DEPARTMENT_MAINTANANCE = 4200;
            DEPARTMENT_TRANSPORTATION = 4300;
            DEPARTMENT_PURCHASING = 4400;
            DEPARTMENT_HR_SAFETY = 4500;
            DEPARTMENT_SALES = 8100;
            DEPARTMENT_G_AND_A = 9100;
            DEPARTMENT_SUPER_ADMIN = 10000;
        }

        public Boolean checkIfEqual(ActiveTrailerList current, ActiveTrailerList modified)
        {
            if (current.TrailerGUID == modified.TrailerGUID &&
               current.TrailerNumber == modified.TrailerNumber &&
               current.TrailerStatus == modified.TrailerStatus &&
               current.LoadStatus == modified.LoadStatus &&
               current.Customer == modified.Customer &&
               current.OrderDate == modified.OrderDate &&
               current.OrderNumber == modified.OrderNumber &&
               current.LocationStatus == modified.LoadStatus &&
               current.TrailerLocation == modified.TrailerLocation &&
               current.Notes == modified.Notes &&
               current.Tags == modified.Tags &&
               current.DateModified == modified.DateModified &&
               current.LastModifiedBy == modified.LastModifiedBy)
            {
                return true;
            }
            else
                return false;
        }
    }
}