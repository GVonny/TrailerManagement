using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrailerManagement.Models;

namespace TrailerManagement.Controllers
{
    public class InventoryController : Controller
    {
        public ActionResult CreateActiveInventoryLocation()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                return View();
            }
        }


        public ActionResult InventoryInput()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                this.ViewData["partNumbers"] = new SelectList(db.PalletTypes.Where(c => c.Type != "SCRAP" && c.Type != "DEDUCTION").OrderBy(c => c.PartNumber), "PartNumber", "PartNumber").ToList();

                return View();
            }
                
        }

        public ActionResult InputTest(FormCollection fc)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                string[] input = fc.AllKeys;
                var partNumber = fc["partNumbers"].Split(',');
                for(var x = 1; x < fc.Count; x++)
                {
                    var key = fc.GetKey(x);
                    var stackQuantity = Convert.ToInt32(fc[key]);
                }
                return View();
            }
        }
    }
}