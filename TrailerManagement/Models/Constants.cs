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
        
        public Constants()
        {
            PERMISSION_ADMIN = 40;
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
        }
    }
}