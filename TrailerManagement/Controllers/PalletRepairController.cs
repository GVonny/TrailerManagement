using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrailerManagement.Models;

namespace TrailerManagement.Controllers
{
    public class PalletRepairController : Controller
    {
        Constants constant = new Constants();
                
        public ActionResult SortList()
        {
            if(Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_EDIT)
            { 
                using (TrailerEntities db = new TrailerEntities())
                {
                    dynamic model = new CreateSortImage();
                    var trailers = from x in db.SortLists select x;
                    this.ViewData["Vendors"] = new SelectList(db.CustomersAndVendors.OrderBy(t => t.Name), "Name", "Name").ToList();

                    model.Trailers = trailers.OrderByDescending(t => t.Status).ThenBy(t => t.DateArrived).ToList();

                    return View(model);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }
       
        public ActionResult SortTrailer(int sortID, int? stackNumber, int? numberOfPeople, bool? imageUploaded)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) 
                && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_EDIT)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var trailer = db.SortLists.Find(sortID);
                    trailer.Status = "OPEN";
                    db.SaveChanges();

                    if (stackNumber != null)
                    {
                        ViewBag.StackNumber = stackNumber;
                    }
                    else if (trailer.MaxStackNumber != null)
                    {
                        ViewBag.StackNumber = trailer.MaxStackNumber + 1;
                    }
                    else
                    {
                        //sort for trailer has not started yet
                        ViewBag.StackNumber = 1;
                    }

                    if (numberOfPeople != null)
                    {
                        ViewBag.NumberOfPeople = numberOfPeople;
                    }

                    if(imageUploaded == null)
                    {
                        ViewBag.ImageUploaded = false;
                    }
                    else
                    {
                        ViewBag.ImageUploaded = true;
                    }

                    var vendor = db.CustomersAndVendors.FirstOrDefault(v => v.Name == trailer.Vendor);

                    dynamic model = new ExpandoObject();
                    model.Trailer = trailer;
                    model.Vendor = vendor;

                    this.ViewData["palletTypes"] = new SelectList(db.PalletTypes.Where(c => c.PartNumber != "50-0140" && c.PartNumber != "50-0001" && c.PartNumber != "50-0004" && c.Type != "SCRAP" && c.Type != "DEDUCTION" && (!c.Description.Contains("REMAN"))).OrderBy(c => c.Description), "PartNumber", "Description").ToList();

                    this.ViewData["scrapTypes"] = new SelectList(db.PalletTypes.Where(c => c.Description.ToLower().Contains("scrap")).OrderByDescending(c => c.PartNumber), "PartNumber", "Description").ToList();

                    return View(model);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        public ActionResult CreateNewStack(int sortID, int stackNumber, string partNumbers, string quantities, int numberOfPeople, string notes, Boolean? allDone, double timeOnStack)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var trailer = db.SortLists.Find(sortID);
                trailer.MaxStackNumber = stackNumber;
                if(trailer.TimeToSort == null)
                {
                    trailer.TimeToSort = timeOnStack;
                }
                else
                {
                    trailer.TimeToSort += timeOnStack;
                }
                

                string[] palletTypes = partNumbers.Split('|');
                string[] palletQuantities = quantities.Split('|');
                string[] palletNotes = notes.Split('|');

                //loops through as many different types were in each stack
                for (int x = 0; x < palletTypes.Length; x++)
                {
                    //check to see if values under entered stack number already exists
                    var stacks = db.MasterStacks.Where(t => t.SortGUID == trailer.SortGUID && t.StackNumber == stackNumber);
                    //if so removes all values under stack number to allow for new values to over ride old stack info
                    if (stacks != null)
                    {
                        foreach (MasterStack stack in stacks)
                        {
                            db.MasterStacks.Remove(stack);
                        }
                    }

                    var palletType = palletTypes[x];

                    if (palletNotes[x] == "-")
                    {
                        palletNotes[x] = null;
                    }

                    PalletType pallet;
                    if (!palletType.Contains('x'))
                    {
                        pallet = db.PalletTypes.FirstOrDefault(p => p.PartNumber == palletType);
                        
                        MasterStack newStack = new MasterStack
                        {
                            SortGUID = sortID,
                            StackNumber = stackNumber,
                            PartNumber = pallet.PartNumber.ToString(),
                            Description = pallet.Description.ToString(),
                            Quantity = Convert.ToInt32(palletQuantities[x]),
                            PeopleOnStack = numberOfPeople,
                            UserSignedIn = Session["name"].ToString(),
                            PalletNote = palletNotes[x],
                        };

                        db.MasterStacks.Add(newStack);
                    }
                    else
                    {
                        MasterStack newStack = new MasterStack
                        {
                            SortGUID = sortID,
                            StackNumber = stackNumber,
                            PartNumber = palletType,
                            Description = "CUSTOM",
                            Quantity = Convert.ToInt32(palletQuantities[x]),
                            PeopleOnStack = numberOfPeople,
                            UserSignedIn = Session["name"].ToString(),
                            PalletNote = palletNotes[x],
                        };
                        db.MasterStacks.Add(newStack);
                    }
                }
                db.SaveChanges();
                stackNumber++;

                if(allDone != null && allDone == true)
                {
                    return RedirectToAction(actionName: "SortConfirmation", controllerName: "PalletRepair", routeValues: new { sortID });
                }
                else
                {
                    return RedirectToAction(actionName: "SortTrailer", controllerName: "PalletRepair", routeValues: new { sortID, stackNumber, numberOfPeople });
                }
            }
        }

        public ActionResult SortConfirmation(int sortID)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_EDIT)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    dynamic model = new ExpandoObject();

                    var trailer = db.MasterStacks.Where(t => t.SortGUID == sortID).OrderBy(t => t.StackNumber);
                    model.Trailer = trailer.ToList();
                    ViewBag.SortID = sortID;
                    this.ViewData["palletTypes"] = new SelectList(db.PalletTypes.Where(c => c.Type != "DEDUCTION" && (c.Description.Contains("REMAN 28 X 28") || !c.Description.Contains("REMAN") || c.PartNumber.Contains("SCRAP"))).OrderBy(c => c.Description), "PartNumber", "Description").ToList();

                    var sort = db.SortLists.FirstOrDefault(s => s.SortGUID == sortID);
                    model.Sort = sort;

                    return View(model);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        public ActionResult CompleteSort(int sortID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var sortedStacks = db.MasterStacks.Where(t => t.SortGUID == sortID);

                ArrayList palletTypes = new ArrayList();
                List<Int32> palletQuantities = new List<Int32>();

                foreach (MasterStack stack in sortedStacks)
                {
                    int foundIndex = -1;

                    //checks to see if palletType already exists in
                    for (int x = 0; x < palletTypes.Count; x++)
                    {
                        if (palletTypes[x].Equals(stack.PartNumber))
                        {
                            foundIndex = x;
                            break;
                        }
                    }
                    if (foundIndex < 0)
                    {
                        palletTypes.Add(stack.PartNumber);
                        palletQuantities.Add(Convert.ToInt32(stack.Quantity));
                    }
                    else
                    {
                        palletQuantities[foundIndex] += Convert.ToInt32(stack.Quantity);
                    }
                }

                SortList sort = db.SortLists.Find(sortID);
                var palletPrices = db.PalletPrices.Where(t => t.VendorName == sort.Vendor);
                
                if(palletTypes.Count != 0)
                {
                    for (int x = 0; x < palletTypes.Count; x++)
                    {
                        var partNumber = palletTypes[x].ToString();
                        CompletedSort palletType;
                        if (!partNumber.Contains('x'))
                        {
                            PalletType pallet = db.PalletTypes.FirstOrDefault(p => p.PartNumber == partNumber);

                            palletType = new CompletedSort()
                            {
                                SortGUID = sortID,
                                PartNumber = pallet.PartNumber,
                                Description = pallet.Description,
                                Quantity = palletQuantities[x],
                                Vendor = sort.Vendor,
                            };
                        }
                        else
                        {
                            palletType = new CompletedSort()
                            {
                                SortGUID = sortID,
                                PartNumber = partNumber,
                                Description = "CUSTOM",
                                Quantity = palletQuantities[x],
                                Vendor = sort.Vendor,
                            };
                        }

                        foreach (PalletPrice vendorPrice in palletPrices)
                        {
                            if (palletType.PartNumber.Equals(vendorPrice.PartNumber))
                            {
                                palletType.Cost = vendorPrice.PurchasePrice;
                                break;
                            }
                        }
                        db.CompletedSorts.Add(palletType);
                    }
                }
                db.SaveChanges();
                return RedirectToAction(actionName: "CreatePayout", controllerName: "PalletRepair", routeValues: new { sortID });
            }
        }

        public ActionResult CreatePayout(int sortID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var sort = db.SortLists.Find(sortID);

                //uncomment to change the status of active trailers when sort is complete
                //var activeTrailerEdit = db.ActiveTrailerLists.FirstOrDefault(a => a.TrailerNumber == sort.TrailerNumber);
                //if(activeTrailerEdit != null)
                //{
                //    activeTrailerEdit.TrailerStatus = constant.TRAILER_STATUS_EMPTY;
                //    activeTrailerEdit.LoadStatus = "";
                //    activeTrailerEdit.Customer = "";
                //    activeTrailerEdit.OrderNumber = "";
                //    activeTrailerEdit.OrderDate = "";
                //    activeTrailerEdit.TrailerLocation = "PR";
                //    activeTrailerEdit.LocationStatus = "ON SITE";
                //    activeTrailerEdit.Notes = "";
                //    activeTrailerEdit.Tags = "";
                //}

                var vendor = db.CustomersAndVendors.FirstOrDefault(v => v.Name == sort.Vendor);

                Payout payout = new Payout()
                {
                    TrailerNumber = sort.TrailerNumber,
                    SortGUID = sort.SortGUID,
                    Vendor = sort.Vendor,
                    OrderNumber = sort.OrderNumber,
                    DateArrived = sort.DateArrived,
                    Status = "NEW",
                    TimeToSort = sort.TimeToSort,

                };

                if (vendor != null)
                {
                    payout.VendorNumber = Convert.ToInt32(vendor.VendorNumber);
                }

                db.Payouts.Add(payout);

                db.SaveChanges();
                return RedirectToAction(actionName: "NewPayout", controllerName: "Email", routeValues: new { sortID });
            }
        }

        public ActionResult PayoutList()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) 
                && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var payouts = from x in db.Payouts select x;
                    payouts = payouts.OrderByDescending(p => p.Status).ThenBy(p => p.Vendor);
                    this.ViewData["Vendors"] = new SelectList(db.CustomersAndVendors.OrderBy(t => t.Name), "Name", "Name").ToList();
                    return View(payouts.ToList());
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        public ActionResult ViewPayout(int sortID)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    dynamic model = new ExpandoObject();

                    var sortedTrailer = db.SortLists.FirstOrDefault(t => t.SortGUID == sortID);
                    string vendor = sortedTrailer.Vendor;

                    var sortedStacks = db.CompletedSorts.Where(t => t.SortGUID == sortID).OrderBy(t => t.PartNumber).ThenBy(t => t.Description);
                    model.SortedTrailer = sortedStacks.ToList();
                    
                    var vendorFromMaster = db.CustomersAndVendors.FirstOrDefault(t => t.Name == vendor.ToString());
                    if(vendorFromMaster != null)
                    {
                        ViewBag.VendorNumber = vendorFromMaster.VendorNumber;
                    }
                    
                    var payout = db.Payouts.FirstOrDefault(p => p.SortGUID == sortID);
                    model.Payout = payout;
                    payout.Status = "IN PROCESS";
                    db.SaveChanges();

                    var images = db.SortImages.Where(i => i.SortGUID == sortID);
                    model.Images = images.ToList();
                    
                    var notes = db.MasterStacks.Where(n => n.SortGUID == sortID && n.PalletNote != null);
                    model.Notes = notes.ToList();

                    this.ViewData["deductionTypes"] = new SelectList(db.PalletTypes.Where(c => c.Type == "DEDUCTION").OrderBy(c => c.PartNumber), "PartNumber", "PartNumber").ToList();
                    this.ViewData["palletTypes"] = new SelectList(db.PalletTypes.Where(c => c.Type != "DEDUCTION" && (c.Description.Contains("REMAN 28 X 28") || !c.Description.Contains("REMAN"))).OrderBy(c => c.Description), "PartNumber", "Description").ToList();
                    this.ViewData["Vendors"] = new SelectList(db.CustomersAndVendors.OrderBy(t => t.Name), "Name", "Name").ToList();

                    return View(model);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        public ActionResult ViewCompletedPayout(int sortID)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    dynamic model = new ExpandoObject();

                    var singleType = db.CompletedSorts.First(c => c.SortGUID == sortID);
                    ViewBag.Vendor = singleType.Vendor;
                    
                    var vendorFromMaster = db.CustomersAndVendors.FirstOrDefault(v => v.Name == singleType.Vendor);
                    if(vendorFromMaster != null)
                    {
                        ViewBag.VendorNumber = vendorFromMaster.VendorNumber;
                    }
                    
                    var payout = db.Payouts.FirstOrDefault(p => p.SortGUID == sortID);
                    model.Payout = payout;

                    payout.Status = "CLOSED";
                    db.SaveChanges();

                    var completedPayouts = db.CompletedSorts.Where(c => c.SortGUID == sortID);
                    model.CompletePayout = completedPayouts.ToList();

                    return View(model);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        //UPDATE LIST INFO

        public ActionResult UpdateArrivalDate(int sortID, string date)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var trailer = db.SortLists.Find(sortID);
                trailer.DateArrived = date;
                db.SaveChanges();
                return RedirectToAction(actionName: "SortList", controllerName: "PalletRepair");
            }
        }
        
        [HttpPost]
        public ActionResult UpdatePayoutInfo(int sortID, string invoiceNumber, string billOfLading, string packingListNumber, string purchaseOrderNumber, string palletOrderNumber, string vendors)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var payout = db.Payouts.FirstOrDefault(p => p.SortGUID == sortID);

                if(invoiceNumber != "")
                {
                    payout.InvoiceNumber = invoiceNumber;
                }
                if(billOfLading != "")
                {
                    payout.BillOfLading = billOfLading;
                }
                if(packingListNumber != "")
                {
                    payout.PackingListNumber = packingListNumber;
                }
                if(purchaseOrderNumber != "")
                {
                    payout.PurchaseOrderNumber = purchaseOrderNumber;
                }
                if(palletOrderNumber != "")
                {
                    payout.OrderNumber = palletOrderNumber;
                }
                db.SaveChanges();
                return RedirectToAction(actionName: "ViewPayout", controllerName: "PalletRepair", routeValues: new { sortID });
            }
        }

        public ActionResult UpdatePayoutVendor(int sortID, string vendors)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var payout = db.Payouts.FirstOrDefault(p => p.SortGUID == sortID);

                var oldVendor = payout.Vendor;
                payout.Vendor = vendors;
                var vendorFromMaster = db.CustomersAndVendors.FirstOrDefault(v => v.Name == vendors);
                payout.VendorNumber = Convert.ToInt32(vendorFromMaster.VendorNumber);

                var vendorPrices = db.PalletPrices.Where(p => p.VendorName == vendorFromMaster.Name);

                var completedStacks = db.CompletedSorts.Where(c => c.Vendor == oldVendor && c.SortGUID == payout.SortGUID);

                foreach (CompletedSort cs in completedStacks)
                {
                    cs.Vendor = vendors;
                    foreach (PalletPrice pp in vendorPrices)
                    {
                        if (cs.PartNumber == pp.PartNumber)
                        {
                            cs.Cost = pp.PurchasePrice;
                        }
                    }
                }
                db.SaveChanges();
                return RedirectToAction(actionName: "ViewPayout", controllerName: "PalletRepair", routeValues: new { sortID });
            }
        }

        //SPLITS

        public ActionResult SplitSortPalletType(int sortID, int stackNumber, int newQuantity, string newPartNumber, string oldPartNumber, string note)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var stack = db.MasterStacks.FirstOrDefault(t => t.SortGUID == sortID && t.StackNumber == stackNumber && t.PartNumber == oldPartNumber);

                var pallet = db.PalletTypes.FirstOrDefault(p => p.PartNumber == newPartNumber);

                var stackCheck = db.MasterStacks.FirstOrDefault(t => t.PartNumber == newPartNumber && t.StackNumber == stackNumber);

                if (stackCheck != null)
                {
                    stackCheck.Quantity += newQuantity;
                }
                else
                {
                    MasterStack newType = new MasterStack()
                    {
                        SortGUID = stack.SortGUID,
                        StackNumber = stackNumber,
                        PartNumber = pallet.PartNumber,
                        Description = pallet.Description,
                        Quantity = newQuantity,
                        PalletNote = note,
                        PeopleOnStack = stack.PeopleOnStack,
                        IsSplit = true,
                        SplitFrom = oldPartNumber,
                    };
                    db.MasterStacks.Add(newType);
                }

                stack.Quantity -= newQuantity;
                if (stack.Quantity == 0)
                {
                    db.MasterStacks.Remove(stack);
                }

                db.SaveChanges();
                return RedirectToAction(actionName: "SortConfirmation", controllerName: "PalletRepair", routeValues: new { sortID = stack.SortGUID });
            }
        }

        public ActionResult SplitPayoutPalletType(int completedSortID, int newSplitQuantity, string newPartNumber)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var splitFrom = db.CompletedSorts.FirstOrDefault(t => t.CompletedSortGUID == completedSortID);
                var splitToo = db.CompletedSorts.FirstOrDefault(t => t.PartNumber == newPartNumber);

                var newPalletType = db.PalletTypes.FirstOrDefault(t => t.PartNumber == newPartNumber);

                CompletedSort newType = new CompletedSort();

                if (splitToo != null)
                {
                    splitToo.Quantity += newSplitQuantity;
                    splitFrom.Quantity -= newSplitQuantity;
                }
                else
                {
                    if (newPartNumber == "")
                    {
                        newPartNumber = splitFrom.PartNumber;
                        newPalletType = db.PalletTypes.FirstOrDefault(t => t.PartNumber == splitFrom.PartNumber);
                    }

                    newType.SortGUID = splitFrom.SortGUID;
                    newType.PartNumber = newPalletType.PartNumber;
                    newType.Description = newPalletType.Description;
                    newType.Quantity = newSplitQuantity;
                    newType.Vendor = splitFrom.Vendor;
                    newType.IsSplit = true;
                    newType.SplitFrom = splitFrom.PartNumber;

                    db.CompletedSorts.Add(newType);
                    splitFrom.Quantity -= newSplitQuantity;
                }

                var palletPrices = db.PalletPrices.FirstOrDefault(t => t.VendorName == splitFrom.Vendor);
                var sortID = splitFrom.SortGUID;

                if (splitFrom.Quantity == 0)
                {
                    db.CompletedSorts.Remove(splitFrom);
                }


                //checking for price in PalletPrices table

                
                if (newType.PartNumber == palletPrices.PartNumber)
                {
                    newType.Cost = palletPrices.PurchasePrice.ToString();
                }

                db.SaveChanges();

                return RedirectToAction(actionName: "ViewPayout", controllerName: "PalletRepair", routeValues: new { sortID });
            }
        }

        //QUANTITIES

        public ActionResult EditPayoutQuantity(int completedSortID, int newQuantity)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var completedSort = db.CompletedSorts.Find(completedSortID);

                completedSort.Quantity = newQuantity;
                db.SaveChanges();

                return RedirectToAction(actionName: "ViewPayout", controllerName: "PalletRepair", routeValues: new { sortID = completedSort.SortGUID });
            }
        }

        public ActionResult EditStackQuantity(int stackID, int newQuantity)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                MasterStack stack = db.MasterStacks.Find(stackID);
                stack.Quantity = newQuantity;
                db.SaveChanges();

                return RedirectToAction(actionName:"SortConfirmation", controllerName:"PalletRepair", routeValues: new { sortID = stack.SortGUID });
            }
        }

        public ActionResult UndoSplit(int completedSortID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var undoSort = db.CompletedSorts.Find(completedSortID);

                var SortID = undoSort.SortGUID;

                var completedSortToUpdate =
                    db.CompletedSorts.FirstOrDefault(t => t.PartNumber == undoSort.SplitFrom && t.SortGUID == undoSort.SortGUID);

                if (completedSortToUpdate != null)
                {
                    completedSortToUpdate.Quantity += undoSort.Quantity;
                    db.CompletedSorts.Remove(undoSort);
                }

                db.SaveChanges();

                return RedirectToAction(actionName: "ViewPayout", controllerName: "PalletRepair", routeValues: new { sortID = SortID });
            }
        }

        public ActionResult UndoSortSplit(int stackID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var undoSplit = db.MasterStacks.FirstOrDefault(u => u.StackGUID == stackID);

                var splitInto = db.MasterStacks.FirstOrDefault(s => s.SortGUID == undoSplit.SortGUID && s.StackNumber == undoSplit.StackNumber && s.PartNumber == undoSplit.SplitFrom);

                splitInto.Quantity += undoSplit.Quantity;

                db.MasterStacks.Remove(undoSplit);
                db.SaveChanges();

                return RedirectToAction(actionName: "SortConfirmation", controllerName: "PalletRepair", routeValues: new { sortID = splitInto.SortGUID });
            }
        }

        public ActionResult Delete(int completedSortID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var delete = db.CompletedSorts.FirstOrDefault(t => t.CompletedSortGUID == completedSortID);
                var sortID = delete.SortGUID;

                var transfer = db.CompletedSorts.FirstOrDefault(t => t.PartNumber == delete.SplitFrom);
                if (transfer != null)
                {
                    transfer.Quantity += delete.Quantity;
                }


                db.CompletedSorts.Remove(delete);
                db.SaveChanges();

                return RedirectToAction(actionName: "ViewPayout", controllerName: "PalletRepair", routeValues: new { sortID });
            }
        }

        //PRICE

        public ActionResult UpdatePrice(int completedSortID, double newPrice, Boolean? keepPreset)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var completedSort = db.CompletedSorts.Find(completedSortID);

                var palletPrice = db.PalletPrices.Where(t => t.VendorName == completedSort.Vendor);
                var vendorNumber = "";
                foreach (var temp in palletPrice)
                {
                    vendorNumber = temp.VendorNumber;
                    break;
                }

                if (keepPreset == true)
                {
                    var pricesPerVendor = db.PalletPrices.Where(t => t.VendorNumber == vendorNumber);

                    Boolean isFound = false;
                    foreach (var part in pricesPerVendor)
                    {
                        if (completedSort.PartNumber == part.PartNumber)
                        {
                            part.PurchasePrice = newPrice.ToString();
                            isFound = true;
                            break;
                        }
                    }
                    if (!isFound)
                    {
                        PalletPrice newPreset = new PalletPrice()
                        {
                            VendorNumber = vendorNumber,
                            VendorName = completedSort.Vendor,
                            PartNumber = completedSort.PartNumber,
                            Description = completedSort.Description,
                            PurchasePrice = newPrice.ToString(),
                        };

                        db.PalletPrices.Add(newPreset);
                    }
                }

                completedSort.Cost = newPrice.ToString();
                db.SaveChanges();
                return RedirectToAction(actionName: "ViewPayout", controllerName: "PalletRepair", routeValues: new { @sortID = completedSort.SortGUID });
            }
        }

        public ActionResult AddDeduction(int sortID, string deduction)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                PalletType getDeduction = db.PalletTypes.FirstOrDefault(d => d.PartNumber == deduction);

                Payout payout = db.Payouts.FirstOrDefault(p => p.SortGUID == sortID);

                var completedSorts = db.CompletedSorts.Where(c => c.SortGUID == sortID);

                Boolean isFound = false;
                foreach (var c in completedSorts)
                {
                    if (c.Description.Equals(deduction.ToUpper()))
                    {
                        isFound = true;
                        break;
                    }
                }

                if (!isFound)
                {
                    CompletedSort newDeduction = new CompletedSort()
                    {
                        SortGUID = sortID,
                        PartNumber = "DEDUCTION",
                        Description = getDeduction.PartNumber.ToUpper(),
                        Quantity = 1,
                        Vendor = payout.Vendor,
                    };
                    db.CompletedSorts.Add(newDeduction);
                    db.SaveChanges();
                }
                return RedirectToAction(actionName: "ViewPayout", controllerName: "PalletRepair", routeValues: new { sortID });
            }
        }

        //ADD/EDIT/VIEW/DELETE

        [HttpPost]
        public ActionResult CreateSortImage(HttpPostedFileBase ImageFile, string vendors, int peopleOnTrailer, int sortID, string notes)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if (ModelState.IsValid)
                {
                    var trailer = db.SortLists.FirstOrDefault(t => t.SortGUID == sortID);
                    if(vendors != "")
                    {
                        trailer.Vendor = vendors;
                        trailer.Customer = vendors;
                    }
                    else
                    {
                        trailer.Vendor = trailer.Customer;
                    }
                    trailer.NumberOfPeopleToStart = peopleOnTrailer;

                    if (ImageFile != null)
                    {
                        if (ImageFile.ContentLength > 0)
                        {
                            var extension = Path.GetExtension(ImageFile.FileName);
                            var fullPath = Server.MapPath("~/SortImages/") + trailer.Vendor.ToString() + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();
                            var path = vendors + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();

                            ImageFile.SaveAs(fullPath);

                            SortImage initialImage = new SortImage()
                            {
                                SortGUID = sortID,
                                StackNumber = 0,
                                TakenBy = Session["name"].ToString(),
                                Notes = notes,
                                ImagePath = path,
                                DateTaken = DateTime.Now.ToString("yyyy-MM-dd"),
                            };
                            db.SortImages.Add(initialImage);
                        }
                    }
                    db.SaveChanges();
                    return RedirectToAction(actionName: "SortTrailer", controllerName: "PalletRepair", routeValues: new { sortID, numberOfPeople = peopleOnTrailer });
                }
                else
                {
                    return View();
                }
            }
        }

        public ActionResult CreateStackImage(HttpPostedFileBase ImageFile, int sortID, int stackNumber, string notes, int peopleOnStack)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if (ModelState.IsValid)
                {
                    var trailer = db.SortLists.FirstOrDefault(t => t.SortGUID == sortID);

                    if (ImageFile != null)
                    {
                        if (ImageFile.ContentLength > 0)
                        {
                            var extension = Path.GetExtension(ImageFile.FileName);
                            var fullPath = Server.MapPath("~/SortImages/") + trailer.Vendor.ToString() + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();
                            var path = trailer.Vendor.ToString() + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();

                            ImageFile.SaveAs(fullPath);

                            SortImage initialImage = new SortImage()
                            {
                                SortGUID = sortID,
                                StackNumber = stackNumber,
                                TakenBy = Session["name"].ToString(),
                                Notes = notes,
                                ImagePath = path,
                                DateTaken = DateTime.Now.ToString("yyyy-MM-dd"),
                            };
                            db.SortImages.Add(initialImage);
                        }
                        db.SaveChanges();
                    }
                    return RedirectToAction(actionName: "SortTrailer", controllerName: "PalletRepair", routeValues: new { sortID, stackNumber, numberOfPeople = peopleOnStack, imageUploaded = true });
                }
                else
                {
                    return View();
                }
            }
        }

        public ActionResult CreateSort()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_EDIT)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    this.ViewData["Vendors"] = new SelectList(db.CustomersAndVendors.OrderBy(t => t.Name), "Name", "Name").ToList();
                    return View();
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        [HttpPost]
        public ActionResult CreateSort(string vendors, string trailerNumber, string orderNumber, string loadStatus, string dateArrived)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                SortList sort = new SortList();
                {
                    sort.TrailerNumber = trailerNumber;
                    sort.Vendor = vendors;
                    sort.Customer = vendors;
                    sort.OrderNumber = orderNumber;
                    sort.LoadStatus = loadStatus;
                    sort.DateArrived = dateArrived;
                    sort.Status = "NEW";
                }
                db.SortLists.Add(sort);
                db.SaveChanges();
                return RedirectToAction(actionName: "SortList", controllerName: "PalletRepair");
            }
        }

        public ActionResult CustomersAndVendors()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var customersAndVendors = db.CustomersAndVendors.OrderBy(c => c.Name).ToList();

                    return View(customersAndVendors);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        public ActionResult CreateCompany()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN)
            {
                return View();
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        [HttpPost]
        public ActionResult CreateCompany([Bind(Include = "CustomerNumber,VendorNumber,Name,Type,SortType,EmailAddresses")] CreateCompany createCompany)
        {
            if(ModelState.IsValid)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    db.CustomersAndVendors.Add(createCompany.MapToCompany());
                    db.SaveChanges();
                }
                return RedirectToAction(actionName: "CustomersAndVendors", controllerName: "PalletRepair");
            }
            else
            {
                return View();
            }
        }

        public ActionResult EditCompany(int id)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var company = db.CustomersAndVendors.Find(id);

                    return View(company);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        [HttpPost]
        public ActionResult EditCompany([Bind(Include = "CustomerNumber,VendorNumber,Name,SortType,EmailAddresses,SortTypeDescription")] CustomersAndVendor editCompany, int id)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var company = db.CustomersAndVendors.Find(id);

                company.VendorNumber = editCompany.VendorNumber;
                company.CustomerNumber = editCompany.CustomerNumber;
                company.Name = editCompany.Name;
                company.SortType = editCompany.SortType;
                company.EmailAddresses = editCompany.EmailAddresses;
                company.SortTypeDescription = editCompany.SortTypeDescription;

                if (editCompany.VendorNumber == null && editCompany.CustomerNumber != null)
                {
                    company.Type = "Customer Only";
                }
                else if (editCompany.CustomerNumber == null && editCompany.VendorNumber != null)
                {
                    company.Type = "Vendor Only";
                }
                else
                {
                    company.Type = "Customer/Vendor";
                }

                db.SaveChanges();
                return RedirectToAction(actionName: "CustomersAndVendors", controllerName: "PalletRepair");
            }
        }

        public ActionResult DeleteCompany(int companyID)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var company = db.CustomersAndVendors.Find(companyID);
                    db.CustomersAndVendors.Remove(company);
                    db.SaveChanges();
                    return RedirectToAction(actionName: "CustomersAndVendors", controllerName: "PalletRepair");
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        public ActionResult PalletTypes()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var palletTypes = db.PalletTypes.OrderBy(p => p.Description).ToList();
                    return View(palletTypes);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        public ActionResult CreatePalletType()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN)
            {
                return View();
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        [HttpPost]
        public ActionResult CreatePalletType([Bind(Include ="PartNumber,Description,Type,TagsRequired,PutAwayLocation")] CreatePalletType palletType, int id)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if(ModelState.IsValid)
                {
                    db.PalletTypes.Add(palletType.MapToType());
                    db.SaveChanges();
                    return RedirectToAction(actionName: "PalletTypes", controllerName: "PalletRepair");
                }
                else
                {
                    return View();
                }
            }
        }

        public ActionResult EditPalletType(int id)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var palletType = db.PalletTypes.Find(id);
                    return View(palletType);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        [HttpPost]
        public ActionResult EditPalletType([Bind(Include = "PartNumber,Description,Type,TagsRequired,PutAwayLocation,Status")] PalletType palletType, int id)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var pallet = db.PalletTypes.Find(id);
                pallet.PartNumber = palletType.PartNumber;
                pallet.Description = palletType.Description;
                pallet.Type = palletType.Type;
                pallet.TagsRequired = palletType.TagsRequired;
                pallet.PutAwayLocation = palletType.PutAwayLocation;
                pallet.Status = palletType.Status;

                db.SaveChanges();
                return RedirectToAction(actionName: "PalletTypes", controllerName: "PalletRepair");
            }
        }

        public ActionResult DeletePalletType(int palletTypeID)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var palletType = db.PalletTypes.FirstOrDefault(p => p.PalletTypeGUID == palletTypeID);
                    db.PalletTypes.Remove(palletType);
                    db.SaveChanges();
                    return RedirectToAction(actionName: "PalletTypes", controllerName: "PalletRepair");
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }
        
        public ActionResult PalletPrices(string startsWith)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var prices = db.PalletPrices.OrderBy(p => p.VendorName).ThenByDescending(p => p.PurchasePrice).ToList();

                    if (startsWith != "ALL")
                    {
                        var prices2 = prices.Where(p => p.VendorName.StartsWith(startsWith)).OrderBy(p => p.VendorName).ThenByDescending(p => p.PurchasePrice);
                        return View(prices2);
                    }
                    return View(prices);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        public ActionResult CreatePalletPrice()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    this.ViewData["palletTypes"] = new SelectList(db.PalletTypes.Where(c => c.Type != "DEDUCTION" && (c.Description.Contains("REMAN 28 X 28") || !c.Description.Contains("REMAN"))).OrderBy(c => c.PartNumber), "PartNumber", "PartNumber").ToList();
                    this.ViewData["vendors"] = new SelectList(db.CustomersAndVendors.OrderBy(t => t.Name), "Name", "Name").ToList();
                    return View();
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        [HttpPost]
        public ActionResult CreatePalletPrice([Bind(Include ="VendorNumber,VendorName,PartNumber,Description,PurchasePrice")]CreatePalletPrice newPreset)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                this.ViewData["palletTypes"] = new SelectList(db.PalletTypes.Where(c => c.Type != "DEDUCTION" && (c.Description.Contains("REMAN 28 X 28") || !c.Description.Contains("REMAN"))).OrderBy(c => c.PartNumber), "PartNumber", "PartNumber").ToList();
                this.ViewData["vendors"] = new SelectList(db.CustomersAndVendors.OrderBy(t => t.Name), "Name", "Name").ToList();
                if (ModelState.IsValid)
                {
                    if (db.PalletPrices.Any(t => t.PartNumber == newPreset.PartNumber && t.VendorName == newPreset.VendorName))
                    {
                        ModelState.AddModelError("PartNumber", "Vendor already has a preset with selected part number");
                        return View();
                    }
                    var palletType = db.PalletTypes.FirstOrDefault(p => p.PartNumber == newPreset.PartNumber);
                    var vendor = db.CustomersAndVendors.FirstOrDefault(v => v.Name == newPreset.VendorName);

                    newPreset.VendorNumber = vendor.VendorNumber;
                    newPreset.Description = palletType.Description;

                    db.PalletPrices.Add(newPreset.MapToPrice());
                    db.SaveChanges();

                    return RedirectToAction(actionName: "PalletPrices", controllerName: "PalletRepair", routeValues: new { startsWith = "ALL" });
                }
                else
                {
                    return View();
                }
                
            }
        }

        public ActionResult EditPalletPrice(int id)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var preset = db.PalletPrices.Find(id);
                    return View(preset);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        [HttpPost]
        public ActionResult EditPalletPrice([Bind(Include = "VendorNumber,VendorName,PartNumber,Description,PurchasePrice")] PalletPrice editPreset, int id)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var preset = db.PalletPrices.Find(id);

                var palletType = db.PalletTypes.FirstOrDefault(p => p.PartNumber == editPreset.PartNumber);
                var vendor = db.CustomersAndVendors.FirstOrDefault(v => v.Name == editPreset.VendorName);

                preset.VendorNumber = vendor.VendorNumber;
                preset.Description = palletType.Description;
                preset.VendorName = editPreset.VendorName;
                preset.PurchasePrice = editPreset.PurchasePrice;
                preset.PartNumber = editPreset.PartNumber;

                db.SaveChanges();
                return RedirectToAction(actionName: "PalletPrices", controllerName: "PalletRepair", routeValues: new { startsWith = "ALL" });
            }
        }

        public ActionResult DeletePalletPrice(int id)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == 2100 || Convert.ToInt32(Session["department"]) == 10000) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var preset = db.PalletPrices.Find(id);
                    db.PalletPrices.Remove(preset);
                    db.SaveChanges();
                    return RedirectToAction(actionName: "PalletPrices", controllerName: "PalletRepair", routeValues: new { startsWith = "ALL" });
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }
    }
}