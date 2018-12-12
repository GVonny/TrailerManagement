using System;
using System.Collections.Generic;
using System.Dynamic;
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
            return View();
        }

        public ActionResult ActiveInventoryLocations()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var locations = db.ActiveInventoryLocations.ToList();
                return View(locations);
            }
        }

        public ActionResult ActiveLocationRows(int locationID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();
                var rows = db.ActiveLocationRows.Where(r => r.LocationNumber == locationID).ToList();
                
                model.Rows = rows;
                model.LocationNumber = locationID;
                
                return View(model);
            }
        }

        public ActionResult CreateLocationRow(int locationID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var maxRowNumber = db.ActiveLocationRows.Where(r => r.LocationNumber == locationID).Max(r => r.RowNumber);
                if (maxRowNumber != null)
                {
                    maxRowNumber++;
                    ViewBag.MaxRowNumber = maxRowNumber;
                }
                ViewBag.LocationNumber = locationID;
            }
            return View();
            
        }

        [HttpPost]
        public ActionResult CreateLocationRow([Bind(Include = "LocationNumber,RowNumber")] ActiveLocationRow newRow)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if(db.ActiveLocationRows.Any(a => a.LocationNumber == newRow.LocationNumber && a.RowNumber == newRow.RowNumber))
                {
                    ModelState.AddModelError("RowNumber", "Row number " + newRow.RowNumber + " already exists in this location");
                    ViewBag.MaxRowNumber = newRow.RowNumber;
                    ViewBag.LocationNumber = newRow.LocationNumber;
                    return View();
                }
                else
                {
                    var locationNumber = newRow.LocationNumber;
                    var location = db.ActiveInventoryLocations.FirstOrDefault(l => l.LocationNumber == newRow.LocationNumber);
                    location.NumberOfRows++;
                    ActiveLocationRow row = new ActiveLocationRow()
                    {
                        LocationNumber = newRow.LocationNumber,
                        RowNumber = newRow.RowNumber,
                    };
                    db.ActiveLocationRows.Add(row);
                    db.SaveChanges();
                    return RedirectToAction(actionName: "ActiveLocationRows", controllerName: "Inventory", routeValues: new { locationID = locationNumber });
                }
            }
        }

        [HttpPost]
        public ActionResult CreateActiveInventoryLocation([Bind(Include = "LocationNumber")] ActiveInventoryLocation location)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                ActiveInventoryLocation newLocation = new ActiveInventoryLocation()
                {
                    LocationNumber = location.LocationNumber,
                };
                db.ActiveInventoryLocations.Add(newLocation);
                db.SaveChanges();
                return RedirectToAction(actionName: "ActiveInventoryLocations", controllerName: "Inventory");
            }
        }


        public ActionResult AddToRow(int locationID, int rowNumber)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                ViewBag.LocationNumber = locationID;
                ViewBag.RowNumber = rowNumber;

                this.ViewData["partNumbers"] = new SelectList(db.PalletTypes.Where(c => c.Type != "SCRAP" && c.Type != "DEDUCTION").OrderBy(c => c.PartNumber), "PartNumber", "PartNumber").ToList();

                return View();
            }
                
        }

        [HttpPost]
        public ActionResult AddToRow(FormCollection fc)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                string[] input = fc.AllKeys;
                var partNumbers = fc["partNumbers"].Split(',');
                foreach(string part in partNumbers)
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