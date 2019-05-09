using Microsoft.Ajax.Utilities;
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

        public ActionResult ActiveLocationRows(string locationID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();
                var rows = db.ActiveLocationRows.Where(r => r.LocationNumber == locationID).OrderBy(r => r.RowNumber).ToList();
                
                model.Rows = rows;
                model.LocationNumber = locationID;
                
                return View(model);
            }
        }

        public ActionResult CreateLocationRow(string locationID)
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
                        NumberOfPartNumbers = 0,
                        NumberOfStacks = 0,
                        PalletCount = 0,
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
                if(db.ActiveInventoryLocations.Any(l => l.LocationNumber == location.LocationNumber))
                {
                    ModelState.AddModelError("LocationNumber", "Location already exists");
                    return View();
                }
                else
                {
                    ActiveInventoryLocation newLocation = new ActiveInventoryLocation()
                    {
                        LocationNumber = location.LocationNumber,
                        NumberOfRows = 0,
                    };
                    db.ActiveInventoryLocations.Add(newLocation);
                    db.SaveChanges();
                }
                
                return RedirectToAction(actionName: "ActiveInventoryLocations", controllerName: "Inventory");
            }
        }
        
        public ActionResult AddToRow(string locationID, int rowNumber)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                ViewBag.LocationNumber = locationID;
                ViewBag.RowNumber = rowNumber;

                var parts = db.PalletTypes.Where(c => c.Type != "SCRAP" && c.Type != "DEDUCTION").OrderBy(c => c.PartNumber);
                
                

                this.ViewData["partNumbers"] = new SelectList(db.PalletTypes.Where(c => c.Type != "SCRAP" && c.Type != "DEDUCTION").OrderBy(c => c.PartNumber), "PartNumber", "PartNumberDescription").ToList();

                return View();
            } 
        }

        [HttpPost]
        public ActionResult AddToRow(FormCollection fc)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                string[] input = fc.AllKeys;
                var locationID = fc["locationNumber"];
                var rowNumber = Convert.ToInt32(fc["rowNumber"]);
                var partNumbers = fc["partNumbers"].Split(',');
                
                List<string> uniquePartNumbers = new List<string>();
                foreach(var part in partNumbers)
                {
                    if(!uniquePartNumbers.Contains(part))
                    {
                        uniquePartNumbers.Add(part);
                    }
                }
                
                var location = db.ActiveLocationRows.FirstOrDefault(r => r.LocationNumber == locationID && r.RowNumber == rowNumber);
                location.NumberOfPartNumbers = uniquePartNumbers.Count;
                location.NumberOfStacks = 0;
                location.PalletCount = 0;

                var stacks = db.InventoryRowStacks.Where(s => s.InventoryRowGUID == location.InventoryRowGUID);
                foreach(InventoryRowStack stack in stacks)
                {
                    db.InventoryRowStacks.Remove(stack);
                }
                
                for (var x = 3; x < fc.Count; x++)
                {
                    var key = fc.GetKey(x);
                    var dashIndex = Convert.ToInt32(key.IndexOf('-'));
                    var partIndex = Convert.ToInt32(key.Substring(0, dashIndex)) - 1;
                    var stackNumber = Convert.ToInt32(key.Substring((dashIndex + 1), (key.Length - 1 - dashIndex)));

                    var quantity = Convert.ToInt32(fc[key]);
                    location.PalletCount += quantity;

                    InventoryRowStack newStack = new InventoryRowStack()
                    {
                        InventoryRowGUID = location.InventoryRowGUID,
                        PartNumber = partNumbers[partIndex],
                        StackQuantity = quantity,
                        StackNumber = stackNumber,
                    };

                    db.InventoryRowStacks.Add(newStack);
                }
                var lastStack = fc.GetKey(fc.Count - 1);
                var lastDashIndex = lastStack.IndexOf('-');
                var totalStacks = Convert.ToInt32(fc.GetKey(fc.Count - 1).Substring(lastDashIndex + 1, (lastStack.Length - 1 - lastDashIndex)));
                location.NumberOfStacks = totalStacks;

                db.SaveChanges();
                return RedirectToAction(actionName: "ActiveLocationRows", controllerName: "Inventory", routeValues: new { locationID });
            }
        }

        public ActionResult ViewRowStacks(int rowID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();

                var location = db.ActiveLocationRows.FirstOrDefault(l => l.InventoryRowGUID == rowID);
                ViewBag.LocationNumber = location.LocationNumber;
                ViewBag.RowNumber = location.RowNumber;

                var stacks = db.InventoryRowStacks.Where(p => p.InventoryRowGUID == rowID).ToList();
                var partNumbers = db.InventoryRowStacks.Where(p => p.InventoryRowGUID == rowID).Select(p => p.PartNumber).Distinct().ToList();

                this.ViewData["partNumbers"] = new SelectList(db.PalletTypes.Where(c => c.Type != "SCRAP" && c.Type != "DEDUCTION").OrderBy(c => c.PartNumber), "PartNumber", "PartNumberDescription").ToList();

                model.Stacks = stacks;
                model.PartNumbers = partNumbers;


                var parts = "";
                foreach (var stack in partNumbers)
                {
                    parts += stack + ",";
                }
                parts = parts.Substring(0, parts.Length - 1);
                ViewBag.Parts = parts;

                return View(model);
            }
        }

        public ActionResult LocationSummary(string locationID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var rows = db.ActiveLocationRows.Where(l => l.LocationNumber == locationID);
                List<Object> stacks = new List<Object>();
                foreach(var row in rows)
                {
                    var stack = db.InventoryRowStacks.Where(s => s.InventoryRowGUID == row.InventoryRowGUID).OrderBy(s => s.StackNumber).ToList();
                    if(stack.Count > 0)
                    {
                        stacks.Add(stack);
                    }
                    
                }
                dynamic model = new ExpandoObject();

                model.Stacks = stacks;
                model.LocationID = locationID;

                return View(model);
            }
        }

        public ActionResult RemoveLocationRow(string locationID, int rowID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var row = db.ActiveLocationRows.FirstOrDefault(r => r.LocationNumber == locationID && r.RowNumber == rowID);

                var stacks = db.InventoryRowStacks.Where(s => s.InventoryRowGUID == row.InventoryRowGUID).ToList();

                if(stacks.Count > 0)
                {
                    foreach (var stack in stacks)
                    {
                        db.InventoryRowStacks.Remove(stack);
                    }
                }
                
                db.ActiveLocationRows.Remove(row);

                db.SaveChanges();

                return RedirectToAction(actionName: "ActiveLocationRows", controllerName: "Inventory", routeValues: new { locationID });
            }
        }

        public void PartNumberDescription()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var parts = db.PalletTypes.ToList();

                foreach(PalletType part in parts)
                {
                    part.PartNumberDescription = part.PartNumber + " - " + part.Description;
                }
                db.SaveChanges();
            }
        }
    }
}