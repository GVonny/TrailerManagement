using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TrailerManagement.Models;

namespace TrailerManagement.Controllers
{
    public class PalletRepairController : Controller
    {
        Constants constant = new Constants();
                
        public ActionResult SortList(string status)
        {
            if(Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if (((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || 
                 Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN || 
                 Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_TRANSPORTATION) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_EDIT) ||
                 (Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_HR_SAFETY && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_ADMIN))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    dynamic model = new ExpandoObject();
                    var trailers = from x in db.SortLists select x;
                    var notArrived = from x in db.SortLists select x;
                    switch (status)
                    {
                        case "CLOSED":
                        {
                            trailers = trailers.Where(t => t.Status == "CLOSED");
                            ViewBag.Closed = true;
                            break;
                        }
                        default:
                        {
                            trailers = trailers.Where(t => t.Status == "OPEN" || t.Status == "NEW" && t.DateArrived != null);
                            notArrived = notArrived.Where(t => t.Status == "NEW" && t.DateArrived == null);
                            break;
                        }
                    }

                    this.ViewData["Vendors"] = new SelectList(db.CustomersAndVendors.OrderBy(t => t.Name), "Name", "Name").ToList();
                    this.ViewData["stackNotes"] = new SelectList(db.StackNotes.OrderBy(n => n.StackNoteOption), "StackNoteOption", "StackNoteOption").ToList();

                    model.Trailers = trailers.OrderByDescending(t => t.Status).ThenByDescending(t => t.DateArrived).ThenBy(t => t.Customer).ToList();
                    model.NotArrived = notArrived.OrderByDescending(t => t.ExpectedArrivalDate).ThenBy(t => t.ExpectedArrivalTime).ToList(); ;
                    return View(model);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        public ActionResult RemoveSort(int sortID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var sort = db.SortLists.FirstOrDefault(s => s.SortGUID == sortID);
                DateTime date = DateTime.Now;
                sort.DateCompleted = date.ToString("yyyy-MM-dd");
                db.SortLists.Remove(sort);
                db.SaveChanges();
                return RedirectToAction(actionName: "SortList", controllerName: "PalletRepair");
            }
        }
        
        public ActionResult SortTrailer(int sortID, int? stackNumber, int? numberOfPeople, bool? imageUploaded)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_EDIT)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var trailer = db.SortLists.Find(sortID);
                    trailer.Status = "OPEN";
                    trailer.SortChoice = "STACK";
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

                    if (imageUploaded == null)
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

                    this.ViewData["palletTypes"] = new SelectList(db.PalletTypes.Where(c => c.PartNumber != "50-0140" && c.PartNumber != "50-0001" && c.PartNumber != "50-0004" && c.Type != "SCRAP" && c.Type != "DEDUCTION").OrderBy(c => c.Description), "PartNumber", "Description").ToList();

                    this.ViewData["scrapTypes"] = new SelectList(db.PalletTypes.Where(c => c.Description.ToLower().Contains("scrap")).OrderByDescending(c => c.PartNumber), "PartNumber", "Description").ToList();

                    this.ViewData["stackNotes"] = new SelectList(db.StackNotes.OrderBy(n => n.StackNoteOption), "StackNoteOption", "StackNoteOption").ToList();
                    this.ViewData["50-0001"] = new SelectList(db.StackNotes.OrderBy(n => n.StackNoteOption), "StackNoteOption", "StackNoteOption").ToList();
                    this.ViewData["50-0004"] = new SelectList(db.StackNotes.OrderBy(n => n.StackNoteOption), "StackNoteOption", "StackNoteOption").ToList();
                    this.ViewData["50-0140"] = new SelectList(db.StackNotes.OrderBy(n => n.StackNoteOption), "StackNoteOption", "StackNoteOption").ToList();

                    return View(model);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }
        
        [HttpPost]
        public ActionResult CreateNewStack(FormCollection fc)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var sortID = Convert.ToInt32(fc["sortID"]);
                var stackNumber = Convert.ToInt32(fc["stackNumber"]);
                var stackSize = Convert.ToInt32(fc["stackSize"]);
                var numberOfPeople = Convert.ToInt32(fc["numberOfPeople"]);
                var timeOnStack = Convert.ToDouble(fc["timeOnStack"]);
                var stackNotes = fc["stackNotes"].Split(',');

                if (stackSize > 0)
                {
                    var trailer = db.SortLists.FirstOrDefault(t => t.SortGUID == sortID);
                    trailer.MaxStackNumber = stackNumber;
                    if (trailer.TimeToSort == null)
                    {
                        trailer.TimeToSort = timeOnStack;
                    }
                    else
                    {
                        trailer.TimeToSort += timeOnStack;
                    }

                    var stacks = db.MasterStacks.Where(t => t.SortGUID == sortID && t.StackNumber == stackNumber).ToList();
                    if (stacks.Count > 0)
                    {
                        foreach (MasterStack stack in stacks)
                        {
                            db.MasterStacks.Remove(stack);
                        }
                    }

                    var counter = 0;
                    for (int x = 5; x < fc.Count - 2; x++)
                    {
                        var key = fc.GetKey(x);
                        if (key != "stackNotes" && key != "palletTypes" && key != "scrapTypes")
                        {
                            var quantity = fc[key];

                            var pallet = db.PalletTypes.FirstOrDefault(p => p.PartNumber == key);

                            if (quantity != "")
                            {
                                if (stackNotes[counter] == "Other")
                                {
                                    counter++;
                                }
                                MasterStack stack = new MasterStack();

                                stack.SortGUID = sortID;
                                stack.StackNumber = stackNumber;
                                stack.PartNumber = pallet.PartNumber;
                                stack.Description = pallet.Description.ToString();
                                stack.Quantity = Convert.ToInt32(quantity);
                                stack.PeopleOnStack = numberOfPeople;
                                stack.UserSignedIn = Session["name"].ToString();
                                stack.PalletNote = stackNotes[counter];


                                db.MasterStacks.Add(stack);
                            }
                            counter++;
                        }
                    }

                    var customPallet = fc["customPallet"].Split(',');
                    if (customPallet[0] != "")
                    {
                        MasterStack newStack = new MasterStack()
                        {
                            SortGUID = sortID,
                            StackNumber = stackNumber,
                            PartNumber = "CUSTOM",
                            Description = "CUSTOM - " + customPallet[0] + " X " + customPallet[1],
                            Quantity = Convert.ToInt32(customPallet[2]),
                            PeopleOnStack = numberOfPeople,
                            UserSignedIn = Session["name"].ToString(),
                            PalletNote = stackNotes[stackNotes.Length - 1],
                        };

                        db.MasterStacks.Add(newStack);
                    }
                    db.SaveChanges();
                }
                
                var returnType = fc.GetKey(fc.Count - 1);
                if (returnType == "nextStack")
                {
                    stackNumber++;
                    return RedirectToAction(actionName: "SortTrailer", controllerName: "PalletRepair", routeValues: new { sortID, stackNumber, numberOfPeople });
                }
                else
                {
                    return RedirectToAction(actionName: "SortConfirmation", controllerName: "PalletRepair", routeValues: new { sortID });
                }
            }
        }

        public ActionResult SortStacks(int sortID)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_EDIT)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var trailer = db.SortLists.Find(sortID);
                    trailer.Status = "OPEN";
                    trailer.SortChoice = "BULK";
                    db.SaveChanges();

                    dynamic model = new ExpandoObject();

                    model.Sort = trailer;

                    this.ViewData["palletTypes"] = new SelectList(db.PalletTypes.Where(c => c.Type != "SCRAP" && c.Type != "DEDUCTION").OrderBy(c => c.Description), "PartNumber", "Description").ToList();

                    this.ViewData["scrapTypes"] = new SelectList(db.PalletTypes.Where(c => c.Description.ToLower().Contains("scrap")).OrderByDescending(c => c.PartNumber), "PartNumber", "Description").ToList();

                    return View(model);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        public ActionResult CreateNewBulkStack(int sortID, string partNumbers, string stackQuantities, string palletQuantities, double timeOnStack)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var sort = db.SortLists.FirstOrDefault(s => s.SortGUID == sortID);
                var stackCount = 0;

                var stackCheck = db.MasterStacks.Where(s => s.SortGUID == sortID);
                if(stackCheck != null)
                {
                    foreach(MasterStack stack in stackCheck)
                    {
                        db.MasterStacks.Remove(stack);
                    }
                }

                string[] palletTypes = partNumbers.Split('|');
                string[] stacks = stackQuantities.Split('|');
                string[] quantities = palletQuantities.Split('|');

                for(int x = 0; x < palletTypes.Length; x++)
                {
                    var partNumber = palletTypes[x];
                    var palletType = db.PalletTypes.FirstOrDefault(p => p.PartNumber == partNumber);
                    var numberStacks = Convert.ToInt32(stacks[x]);
                    var quantity = Convert.ToInt32(quantities[x]);
                    MasterStack stack = new MasterStack()
                    {
                        SortGUID = sortID,
                        StackNumber = x + 1,
                        PartNumber = palletTypes[x],
                        Description = palletType.Description,
                        Quantity = (numberStacks * quantity),
                        PeopleOnStack = 1,
                        UserSignedIn = Session["name"].ToString(),
                    };
                    db.MasterStacks.Add(stack);
                    stackCount++;
                }
                sort.MaxStackNumber = stackCount;
                db.SaveChanges();

                return RedirectToAction(actionName: "SortConfirmation", controllerName: "PalletRepair", routeValues: new { sortID });
            }
        }

        public ActionResult SortSummary(int sortID)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_EDIT)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    dynamic model = new ExpandoObject();

                    var stacks = db.MasterStacks.Where(t => t.SortGUID == sortID).OrderBy(t => t.StackNumber);
                    model.Trailer = stacks.ToList();

                    var stackNumber = stacks.Max(t => t.StackNumber);
                    stackNumber++;

                    this.ViewData["palletTypes"] = new SelectList(db.PalletTypes.Where(c => c.Type != "DEDUCTION" && c.Type != "SCRAP").OrderBy(c => c.Description), "PartNumber", "Description").ToList();

                    var sort = db.SortLists.FirstOrDefault(s => s.SortGUID == sortID);
                    model.Sort = sort;
                    model.People = sort.NumberOfPeopleToStart;
                    model.StackNumber = stackNumber;

                    return View(model);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        public ActionResult CompleteSortSummary(int sortID)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_EDIT)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    dynamic model = new ExpandoObject();

                    var trailer = db.MasterStacks.Where(t => t.SortGUID == sortID).OrderBy(t => t.StackNumber);
                    model.Trailer = trailer.ToList();

                    var stackNumber = trailer.Max(t => t.StackNumber);

                    this.ViewData["palletTypes"] = new SelectList(db.PalletTypes.Where(c => c.Type != "DEDUCTION" && c.Type != "SCRAP").OrderBy(c => c.Description), "PartNumber", "Description").ToList();

                    var sort = db.SortLists.FirstOrDefault(s => s.SortGUID == sortID);
                    model.Sort = sort;
                    model.People = sort.NumberOfPeopleToStart;
                    model.StackNumber = stackNumber;

                    return View(model);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        public ActionResult SortConfirmation(int sortID)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_EDIT)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    dynamic model = new ExpandoObject();

                    var trailer = db.MasterStacks.Where(t => t.SortGUID == sortID).OrderBy(t => t.StackNumber);
                    model.Trailer = trailer.ToList();

                    this.ViewData["palletTypes"] = new SelectList(db.PalletTypes.Where(c => c.Type != "DEDUCTION" && c.Type != "SCRAP").OrderBy(c => c.Description), "PartNumber", "Description").ToList();

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
                var sortedStacks = db.MasterStacks.Where(t => t.SortGUID == sortID).ToList();

                var completeStacks = db.CompletedSorts.Where(c => c.SortGUID == sortID).ToList();

                if(sortedStacks.Count > 0)
                {
                    if(completeStacks.Count > 0)
                    {
                        foreach (CompletedSort stack in completeStacks)
                        {
                            db.CompletedSorts.Remove(stack);
                        }
                    }
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

                    if (palletTypes.Count != 0)
                    {
                        for (int x = 0; x < palletTypes.Count; x++)
                        {
                            var partNumber = palletTypes[x].ToString();
                            CompletedSort palletType;
                            if (!partNumber.Contains("CUSTOM"))
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
                }
                return RedirectToAction(actionName: "CreatePayout", controllerName: "PalletRepair", routeValues: new { sortID });
            }
        }

        public ActionResult CreatePayout(int sortID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var sort = db.SortLists.Find(sortID);

                //uncomment to change the status of active trailers when sort is complete
                var activeTrailerEdit = db.ActiveTrailerLists.FirstOrDefault(a => a.TrailerNumber == sort.TrailerNumber);
                if (activeTrailerEdit != null && activeTrailerEdit.TrailerStatus == constant.TRAILER_STATUS_NEED_EMPTY)
                {
                    activeTrailerEdit.TrailerStatus = constant.TRAILER_STATUS_EMPTY;
                    activeTrailerEdit.LoadStatus = "";
                    activeTrailerEdit.Customer = "";
                    activeTrailerEdit.OrderNumber = "";
                    activeTrailerEdit.OrderDate = "";
                    activeTrailerEdit.TrailerLocation = "PR";
                    activeTrailerEdit.LocationStatus = "ON SITE";
                    activeTrailerEdit.Notes = "";
                    activeTrailerEdit.Tags = "";

                    SmtpClient client = new SmtpClient("smtp.outlook.com", 587);
                    client.EnableSsl = true;
                    client.Timeout = 100000;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("grant.vonhaden@palletusa.com", Properties.Settings.Default.EmailPassword);
                    
                    string body;

                    //uncomment to send Cesar and Gabe emails when trailers are empty and status was previously NEED EMPTY
                    string cesarEmail = "dispatch@palletusa.com";
                    string gabeEmail = "gabe.dominguez@palletusa.com";

                    body = "Cesar,<br><br> Trailer " + activeTrailerEdit.TrailerNumber + " is ready to be moved at the PR docks. The trailer status has been changed from NEED EMPTY on the Active Trailer List.<br><br>This is an autogenerated message.";

                    MailMessage message = new MailMessage("grant.vonhaden@palletusa.com", cesarEmail, "Empty Trailer", body);
                    message.IsBodyHtml = true;
                    message.BodyEncoding = UTF8Encoding.UTF8;
                    client.Send(message);
                    message = new MailMessage("grant.vonhaden@palletusa.com", gabeEmail, "Empty Trailer", body);
                    message.IsBodyHtml = true;
                    message.BodyEncoding = UTF8Encoding.UTF8;
                    client.Send(message);
                }

                var vendor = db.CustomersAndVendors.FirstOrDefault(v => v.Name == sort.Vendor);

                var payoutCheck = db.Payouts.FirstOrDefault(p => p.SortGUID == sortID);
                if(payoutCheck != null)
                {
                    db.Payouts.Remove(payoutCheck);
                }

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

        public ActionResult PayoutList(string status, string startsWith)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_MANAGER)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    dynamic model = new ExpandoObject();
                    var payouts = from x in db.Payouts select x;
                    var oneWeek = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");

                    Session["startsWith"] = "";
                    List<string> startingLetters = new List<string>();
                    switch (status)
                    {
                        case "CLOSED":
                        {
                            if(startsWith != null && startsWith != "ALL")
                            {
                                payouts = payouts.Where(p => p.Vendor.StartsWith(startsWith) && p.Status == "CLOSED").OrderBy(p => p.Vendor).ThenByDescending(p => p.DateCompleted).ThenByDescending(p => p.DateArrived);

                                Session["startsWith"] = startsWith;
                            }
                            else
                            {
                                payouts = payouts.Where(p => p.Status == "CLOSED").OrderByDescending(p => p.DateCompleted).ThenBy(p => p.Vendor);
                            }
                            var closedPayouts = db.Payouts.Where(p => p.Status == "CLOSED").OrderBy(p => p.Vendor);
                            
                            foreach (Payout payout in closedPayouts)
                            {
                                if (payout.Vendor != "" && payout.Vendor != null)
                                {
                                    var startingLetter = payout.Vendor.Substring(0, 1);
                                    if (!startingLetters.Contains(startingLetter))
                                    {
                                        startingLetters.Add(startingLetter);
                                    }
                                }
                            }
                            ViewBag.Closed = true;
                            break;
                        }
                        default:
                        {
                            payouts = payouts.Where(p => p.Status == "NEW" || p.Status == "IN PROCESS").OrderByDescending(p => p.Status).ThenByDescending(p => p.DateArrived).ThenBy(p => p.Vendor);
                            break;
                        }
                    }

                    

                    this.ViewData["Vendors"] = new SelectList(db.CustomersAndVendors.OrderBy(t => t.Name), "Name", "Name").ToList();

                    model.Payouts = payouts.ToList();
                    model.StartingLetters = startingLetters;
                    return View(model);
                }
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        public ActionResult RemovePayout(int sortID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var payout = db.Payouts.FirstOrDefault(p => p.SortGUID == sortID);
                payout.Status = "CLOSED";
                DateTime date = DateTime.Now;

                payout.DateCompleted = date.ToString("yyyy-MM-dd");
                
                db.SaveChanges();
                return RedirectToAction(actionName: "PayoutList", controllerName: "PalletRepair", routeValues: new { status = "CLOSED" });
            }
        }

        public ActionResult ViewPayout(int sortID, Boolean? isCompleted)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();

                var sortedTrailer = db.SortLists.FirstOrDefault(t => t.SortGUID == sortID);

                var sortedStacks = db.CompletedSorts.Where(t => t.SortGUID == sortID).OrderBy(t => t.PartNumber).ThenBy(t => t.Description);
                model.SortedTrailer = sortedStacks.ToList();
                                    
                var payout = db.Payouts.FirstOrDefault(p => p.SortGUID == sortID);
                if(payout.Status == "NEW")
                {
                    payout.Status = "IN PROCESS";
                }
                if(isCompleted != null)
                {
                    model.IsCompleted = isCompleted;
                    if (isCompleted == false)
                    {
                        payout.Status = "IN PROCESS";
                    }
                }
                else
                {
                    model.IsCompleted = null;
                }
                
                db.SaveChanges();

                var vendor = db.CustomersAndVendors.FirstOrDefault(v => v.Name == payout.Vendor);
                if(vendor != null)
                {
                    model.Vendor = vendor;
                }
                else
                {
                    model.Vendor = null;
                }

                model.Payout = payout;
                
                var images = db.SortImages.Where(i => i.SortGUID == sortID);
                model.Images = images.ToList();
                    
                var notes = db.MasterStacks.Where(n => n.SortGUID == sortID && (n.PalletNote != null && n.PalletNote != ""));
                model.Notes = notes.ToList();

                var stacks = db.MasterStacks.Where(u => u.SortGUID == sortID).ToList();
                if(stacks.Count > 0)
                {
                    var user = stacks.Last().UserSignedIn;
                    model.User = user;
                }
                else
                {
                    model.User = null;
                }

                this.ViewData["deductionTypes"] = new SelectList(db.PalletTypes.Where(c => c.Type == "DEDUCTION").OrderBy(c => c.PartNumber), "PartNumber", "PartNumber").ToList();
                this.ViewData["palletTypes"] = new SelectList(db.PalletTypes.Where(c => c.Type != "DEDUCTION").OrderBy(c => c.Description), "PartNumber", "Description").ToList();
                this.ViewData["vendorID"] = new SelectList(db.CustomersAndVendors.OrderBy(t => t.Name), "CompanyGUID", "Name").ToList();

                return View(model);
            }
        }

        public ActionResult ViewCompletedPayout(int sortID, string startsWith)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();
                                    
                var payout = db.Payouts.FirstOrDefault(p => p.SortGUID == sortID);
                model.Payout = payout;

                payout.Status = "CLOSED";
                db.SaveChanges();

                var completedPayouts = db.CompletedSorts.Where(c => c.SortGUID == sortID);
                model.CompletePayout = completedPayouts.ToList();

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult AddPhotosToPayout(int sortID, HttpPostedFileBase ImageFile, string photoNote)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if (ImageFile != null)
                {
                    var trailer = db.SortLists.FirstOrDefault(t => t.SortGUID == sortID);
                    if (ImageFile.ContentLength > 0)
                    {
                        var extension = Path.GetExtension(ImageFile.FileName);
                        var fullPath = Server.MapPath("~/SortImages/") + trailer.Vendor.ToString() + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();
                        var path = trailer.Vendor + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();

                        ImageFile.SaveAs(fullPath);

                        SortImage initialImage = new SortImage()
                        {
                            SortGUID = sortID,
                            StackNumber = 0,
                            TakenBy = Session["name"].ToString(),
                            Notes = photoNote,
                            ImagePath = path,
                            DateTaken = DateTime.Now.ToString("yyyy-MM-dd"),
                        };
                        db.SortImages.Add(initialImage);
                    }
                }
                db.SaveChanges();
            }
            return RedirectToAction(actionName: "ViewPayout", controllerName: "PalletRepair", routeValues: new { sortID, isCompleted = true });
        }

        public ActionResult UpdateAccountingNote(int sortID, string note)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var payout = db.Payouts.FirstOrDefault(p => p.SortGUID == sortID);
                
                payout.AccountingNote = note;

                
                db.SaveChanges();
                return RedirectToAction(actionName: "ViewPayout", controllerName: "PalletRepair", routeValues: new { sortID });
            }
        }

        //UPDATE LIST INFO

        public ActionResult UpdateArrivalDate(int sortID, string date)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var trailer = db.SortLists.Find(sortID);
                trailer.DateArrived = date;
                trailer.ExpectedArrivalDate = null;
                trailer.ExpectedArrivalTime = null;
                db.SaveChanges();
                return RedirectToAction(actionName: "SortList", controllerName: "PalletRepair");
            }
        }
        
        [HttpPost]
        public ActionResult UpdatePayoutInfo(int sortID, string trailerNumber, string invoiceNumber, string billOfLading, string packingListNumber, string purchaseOrderNumber, string palletOrderNumber)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var payout = db.Payouts.FirstOrDefault(p => p.SortGUID == sortID);

                if(trailerNumber != "")
                {
                    payout.TrailerNumber = trailerNumber.ToUpper();
                }
                else
                {
                    payout.TrailerNumber = trailerNumber;
                }
                if(invoiceNumber != "")
                {
                    payout.InvoiceNumber = invoiceNumber.ToUpper();
                }
                else
                {
                    payout.InvoiceNumber = invoiceNumber;
                }
                if (billOfLading != "")
                {
                    payout.BillOfLading = billOfLading.ToUpper();
                }
                else
                {
                    payout.BillOfLading = billOfLading;
                }
                if (packingListNumber != "")
                {
                    payout.PackingListNumber = packingListNumber.ToUpper();
                }
                else
                {
                    payout.PackingListNumber = packingListNumber;
                }
                if (purchaseOrderNumber != "")
                {
                    payout.PurchaseOrderNumber = purchaseOrderNumber.ToUpper();
                }
                else
                {
                    payout.PurchaseOrderNumber = purchaseOrderNumber;
                }
                if (palletOrderNumber != "")
                {
                    payout.OrderNumber = palletOrderNumber.ToUpper();
                }
                else
                {
                    payout.OrderNumber = palletOrderNumber;
                }
                db.SaveChanges();
                return RedirectToAction(actionName: "ViewPayout", controllerName: "PalletRepair", routeValues: new { sortID });
            }
        }
        
        [HttpPost]
        public ActionResult UpdatePayoutVendor(int sortID, int vendorID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var payout = db.Payouts.FirstOrDefault(p => p.SortGUID == sortID);

                var oldVendor = payout.Vendor;
                
                var vendorFromMaster = db.CustomersAndVendors.FirstOrDefault(v => v.CompanyGUID == vendorID);
                payout.Vendor = vendorFromMaster.Name;
                payout.VendorNumber = Convert.ToInt32(vendorFromMaster.VendorNumber);

                var vendorPrices = db.PalletPrices.Where(p => p.VendorName == vendorFromMaster.Name);

                var completedStacks = db.CompletedSorts.Where(c => c.Vendor == oldVendor && c.SortGUID == payout.SortGUID);

                foreach (CompletedSort cs in completedStacks)
                {
                    cs.Vendor = vendorFromMaster.Name;
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

        public ActionResult ChangeExpectedDate(string date, int sortID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var sort = db.SortLists.FirstOrDefault(s => s.SortGUID == sortID);
                sort.ExpectedArrivalDate = date;
                db.SaveChanges();
                return RedirectToAction(actionName: "SortList", controllerName: "PalletRepair");
            }
        }

        public ActionResult ChangeExpectedTime(int sortID, string time)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var sort = db.SortLists.FirstOrDefault(s => s.SortGUID == sortID);
                sort.ExpectedArrivalTime = time;
                db.SaveChanges();
                return RedirectToAction(actionName: "SortList", controllerName: "PalletRepair");
            }
        }

        //SPLITS

        public ActionResult SplitSortPalletType(int sortID, int stackNumber, int newQuantity, string newPartNumber, string oldPartNumber, string note)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var stack = db.MasterStacks.FirstOrDefault(t => t.SortGUID == sortID && t.StackNumber == stackNumber && t.PartNumber == oldPartNumber);

                var pallet = db.PalletTypes.FirstOrDefault(p => p.PartNumber == newPartNumber);

                var stackCheck = db.MasterStacks.FirstOrDefault(t => t.SortGUID == sortID && t.PartNumber == newPartNumber && t.StackNumber == stackNumber);

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
                        UserSignedIn = Session["name"].ToString(),
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
                var splitToo = db.CompletedSorts.FirstOrDefault(t => t.SortGUID == splitFrom.SortGUID && t.PartNumber == newPartNumber);

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

                var completedSortToUpdate = db.CompletedSorts.FirstOrDefault(t => t.PartNumber == undoSort.SplitFrom && t.SortGUID == undoSort.SortGUID);

                if (completedSortToUpdate != null)
                {
                    completedSortToUpdate.Quantity += undoSort.Quantity;
                    db.CompletedSorts.Remove(undoSort);
                }
                else
                {
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
        public ActionResult CreateSortImage(HttpPostedFileBase ImageFile, string vendors, int peopleOnTrailer, int sortID, string stackNotes, string sortType)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if (ModelState.IsValid)
                {
                    var trailer = db.SortLists.FirstOrDefault(t => t.SortGUID == sortID);
                    if (vendors != "")
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
                                Notes = stackNotes,
                                ImagePath = path,
                                DateTaken = DateTime.Now.ToString("yyyy-MM-dd"),
                            };
                            db.SortImages.Add(initialImage);
                        }
                    }
                    db.SaveChanges();

                    if (sortType == "Stack Sort")
                    {
                        return RedirectToAction(actionName: "SortTrailer", controllerName: "PalletRepair", routeValues: new { sortID, numberOfPeople = peopleOnTrailer });
                    }
                    else if (sortType == "Bulk Sort")
                    {
                        return RedirectToAction(actionName: "SortStacks", controllerName: "PalletRepair", routeValues: new { sortID });
                    }
                    else
                    {
                        return RedirectToAction(actionName: "SortTrailer", controllerName: "PalletRepair", routeValues: new { sortID, numberOfPeople = peopleOnTrailer });
                    }
                }
                else
                {
                    return View();
                }
            }
        }

        public ActionResult CreateStackImage(HttpPostedFileBase ImageFile, int sortID, int stackNumber, string stackNotes, int peopleOnStack)
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
                                Notes = stackNotes,
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
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_EDIT)
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
        public ActionResult CreateSort(string vendors, string trailerNumber, string orderNumber, string loadStatus, string dateArrived, string note, string expectedDate, string expectedTime)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var trailerNumberTemp = trailerNumber.ToUpper();
                if(trailerNumberTemp.Contains("USA"))
                {
                    var spaceIndex = trailerNumberTemp.IndexOf(" ");
                    if(spaceIndex > 0)
                    {
                        trailerNumberTemp = trailerNumberTemp.Substring(0, spaceIndex) + trailerNumberTemp.Substring(spaceIndex + 1);
                    }
                }
                else if(trailerNumberTemp.Contains("FLAT"))
                {
                    var fIndex = trailerNumberTemp.IndexOf("F");
                    if (fIndex > 0)
                    {
                        trailerNumberTemp = trailerNumberTemp.Substring(0, fIndex) + '-' + trailerNumberTemp.Substring(fIndex);
                    }

                }
                SortList sort = new SortList();
                {
                    sort.TrailerNumber = trailerNumberTemp;
                    sort.Vendor = vendors;
                    sort.Customer = vendors;
                    sort.OrderNumber = orderNumber;
                    sort.LoadStatus = loadStatus;
                    sort.DateArrived = dateArrived;
                    sort.Status = "NEW";
                    
                }

                if(note != "")
                {
                    sort.ArrivalNote = note;
                }

                if(expectedDate != "")
                {
                    sort.ExpectedArrivalDate = expectedDate;
                }

                if(expectedTime != "")
                {
                    sort.ExpectedArrivalTime = expectedTime;
                }
                db.SortLists.Add(sort);
                db.SaveChanges();
                return RedirectToAction(actionName: "SortList", controllerName: "PalletRepair");
            }
        }

        public ActionResult CustomersAndVendors(string startsWith)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_MANAGER)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var customersAndVendors = db.CustomersAndVendors.OrderBy(c => c.Name).ToList();
                    if (startsWith != "ALL")
                    {
                        var customersAndVendors2 = customersAndVendors.Where(p => p.Name.StartsWith(startsWith)).OrderBy(p => p.Name);
                        return View(customersAndVendors2);
                    }
                    
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
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_MANAGER)
            {
                return View();
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        [HttpPost]
        public ActionResult CreateCompany([Bind(Include = "CustomerNumber,VendorNumber,Name,Type,SortType,EmailAddresses,SortTypeDescription,PhoneNumber,ContactName,PayoutDescription")] CustomersAndVendor createCompany)
        {
            if(ModelState.IsValid)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    CustomersAndVendor company = new CustomersAndVendor()
                    {
                        CustomerNumber = createCompany.CustomerNumber,
                        VendorNumber = createCompany.VendorNumber,
                        Name = createCompany.Name.ToUpper(),
                        SortType = createCompany.SortType,
                        EmailAddresses = createCompany.EmailAddresses,
                        SortTypeDescription = createCompany.SortTypeDescription,
                        PhoneNumber = createCompany.PhoneNumber,
                        ContactName = createCompany.ContactName,
                        PayoutDescription = createCompany.PayoutDescription,
                    };

                    if (createCompany.VendorNumber == null && createCompany.CustomerNumber != null)
                    {
                        company.Type = "Customer Only";
                    }
                    else if (createCompany.CustomerNumber == null && createCompany.VendorNumber != null)
                    {
                        company.Type = "Vendor Only";
                    }
                    else if (createCompany.CustomerNumber != null && createCompany.VendorNumber != null)
                    {
                        company.Type = "Customer/Vendor";
                    }
                    db.CustomersAndVendors.Add(company);
                    db.SaveChanges();
                }
                return RedirectToAction(actionName: "CustomersAndVendors", controllerName: "PalletRepair", routeValues: new { startsWith = "ALL" });
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
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_MANAGER)
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
        public ActionResult EditCompany([Bind(Include = "CustomerNumber,VendorNumber,Name,Type,SortType,EmailAddresses,SortTypeDescription,PhoneNumber,ContactName,PayoutDescription")] CustomersAndVendor editCompany, int id)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var company = db.CustomersAndVendors.Find(id);

                company.VendorNumber = editCompany.VendorNumber;
                company.CustomerNumber = editCompany.CustomerNumber;
                company.Name = editCompany.Name.ToUpper();
                company.SortType = editCompany.SortType;
                company.EmailAddresses = editCompany.EmailAddresses;
                if(editCompany.SortTypeDescription != null)
                {
                    company.SortTypeDescription = editCompany.SortTypeDescription.ToUpper();
                }
                if(editCompany.PayoutDescription != null)
                {
                    company.PayoutDescription = editCompany.PayoutDescription.ToUpper();
                }
                
                if (editCompany.VendorNumber == null && editCompany.CustomerNumber != null)
                {
                    company.Type = "Customer Only";
                }
                else if (editCompany.CustomerNumber == null && editCompany.VendorNumber != null)
                {
                    company.Type = "Vendor Only";
                }
                else if(editCompany.CustomerNumber != null && editCompany.VendorNumber != null)
                {
                    company.Type = "Customer/Vendor";
                }

                db.SaveChanges();
                return RedirectToAction(actionName: "CustomersAndVendors", controllerName: "PalletRepair", routeValues: new { startsWith = "ALL" } );
            }
        }

        public ActionResult DeleteCompany(int companyID)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_MANAGER)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var company = db.CustomersAndVendors.Find(companyID);
                    db.CustomersAndVendors.Remove(company);
                    db.SaveChanges();
                    return RedirectToAction(actionName: "CustomersAndVendors", controllerName: "PalletRepair", routeValues: new { startsWith = "ALL" });
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
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_MANAGER)
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
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_MANAGER)
            {
                return View();
            }
            else
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
        }

        [HttpPost]
        public ActionResult CreatePalletType([Bind(Include ="PartNumber,Description,Type,TagsRequired,PutAwayLocation")] PalletType palletType)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if(ModelState.IsValid)
                {
                    PalletType newPalletType = new PalletType()
                    {
                        PartNumber = palletType.PartNumber,
                        Description = palletType.PartNumber,
                        Type = palletType.Type,
                        TagsRequired = palletType.TagsRequired,
                        PutAwayLocation = palletType.PutAwayLocation,
                        Status = "ACTIVE",
                        PartNumberDescription = palletType.PartNumber + " - " + palletType.Description,
                    };
                    db.PalletTypes.Add(newPalletType);
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
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_MANAGER)
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
                if(palletType.PartNumber != null)
                {
                    pallet.PartNumber = palletType.PartNumber.ToUpper();
                }
                else
                {
                    pallet.PartNumber = palletType.PartNumber;
                }

                if (palletType.Description != null)
                {
                    pallet.Description = palletType.Description.ToUpper();
                }
                else
                {
                    pallet.Description = palletType.Description;
                }
                pallet.Type = palletType.Type;
                pallet.TagsRequired = palletType.TagsRequired;
                if(palletType.PutAwayLocation != null)
                {
                    pallet.PutAwayLocation = palletType.PutAwayLocation.ToUpper();
                }
                else
                {
                    pallet.PutAwayLocation = palletType.PutAwayLocation;
                }
                
                pallet.Status = palletType.Status;
                pallet.PartNumberDescription = pallet.PartNumber + " - " + pallet.Description;

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
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_MANAGER)
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
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_MANAGER)
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
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_MANAGER)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    this.ViewData["palletTypes"] = new SelectList(db.PalletTypes.Where(c => c.Type != "DEDUCTION").OrderBy(c => c.PartNumber), "PartNumber", "PartNumber").ToList();
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
        public ActionResult CreatePalletPrice([Bind(Include ="VendorNumber,VendorName,PartNumber,Description,PurchasePrice")]PalletPrice newPreset)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                this.ViewData["palletTypes"] = new SelectList(db.PalletTypes.Where(c => c.Type != "DEDUCTION").OrderBy(c => c.PartNumber), "PartNumber", "PartNumber").ToList();
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

                    PalletPrice newPrice = new PalletPrice()
                    {
                        VendorNumber = newPreset.VendorNumber,
                        VendorName = newPreset.VendorName,
                        PartNumber = newPreset.PartNumber,
                        Description = newPreset.Description,
                        PurchasePrice = newPreset.PurchasePrice,
                    };
                    db.PalletPrices.Add(newPrice);
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
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_MANAGER)
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
            if ((Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_PALLET_REPAIR || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN) && Convert.ToInt32(Session["permission"]) >= constant.PERMISSION_MANAGER)
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

        public ActionResult StackNotes()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var notes = db.StackNotes.OrderBy(n => n.StackNoteOption).ToList();
                return View(notes);
            }
        }

        public ActionResult CreateStackNote(string stackNote)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                StackNote newNote = new StackNote()
                {
                    StackNoteOption = stackNote,
                };
                db.StackNotes.Add(newNote);
                db.SaveChanges();
                return RedirectToAction(actionName: "StackNotes", controllerName: "PalletRepair");
            }
        }

        public ActionResult EditStackNote(int noteID, string stackNote)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var note = db.StackNotes.FirstOrDefault(n => n.StackNoteGUID == noteID);
                note.StackNoteOption = stackNote;
                db.SaveChanges();
                return RedirectToAction(actionName: "StackNotes", controllerName: "PalletRepair");
            }
        }

        public ActionResult DeleteStackNote(int noteID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var note = db.StackNotes.FirstOrDefault(n => n.StackNoteGUID == noteID);
                db.StackNotes.Remove(note);
                db.SaveChanges();
                return RedirectToAction(actionName: "StackNotes", controllerName: "PalletRepair");
            }
        }

        public ActionResult EditSortNote(int sortID, string newNote)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var sort = db.SortLists.FirstOrDefault(s => s.SortGUID == sortID);
                sort.ArrivalNote = newNote;
                db.SaveChanges();
                return RedirectToAction(actionName: "SortList", controllerName: "PalletRepair");
            }
        }
        //Testing
        
        public ActionResult SortListTest(string status)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();
                var trailers = from x in db.SortListTests select x;
                switch (status)
                {
                    case "CLOSED":
                    {
                        trailers = trailers.Where(t => t.Status == "CLOSED");
                        ViewBag.Closed = true;
                        break;
                    }
                    default:
                    {
                        trailers = trailers.Where(t => t.Status == "OPEN" || t.Status == "NEW");
                        break;
                    }
                }

                this.ViewData["Vendors"] = new SelectList(db.CustomersAndVendors.OrderBy(t => t.Name), "Name", "Name").ToList();
                this.ViewData["stackNotes"] = new SelectList(db.StackNotes.OrderBy(n => n.StackNoteOption), "StackNoteOption", "StackNoteOption").ToList();

                model.Trailers = trailers.OrderByDescending(t => t.Status).ThenByDescending(t => t.DateArrived).ThenBy(t => t.Customer).ToList();
                return View(model);
            }

        }
        
        public ActionResult SortTrailerTest(int sortID, int? stackNumber, int? numberOfPeople, bool? imageUploaded)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var trailer = db.SortListTests.Find(sortID);
                trailer.Status = "OPEN";
                trailer.SortChoice = "STACK";
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

                if (imageUploaded == null)
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

                this.ViewData["palletTypes"] = new SelectList(db.PalletTypes.Where(c => c.PartNumber != "50-0140" && c.PartNumber != "50-0001" && c.PartNumber != "50-0004" && c.Type != "SCRAP" && c.Type != "DEDUCTION").OrderBy(c => c.Description), "PartNumber", "Description").ToList();

                this.ViewData["scrapTypes"] = new SelectList(db.PalletTypes.Where(c => c.Description.ToLower().Contains("scrap")).OrderByDescending(c => c.PartNumber), "PartNumber", "Description").ToList();

                this.ViewData["stackNotes"] = new SelectList(db.StackNotes.OrderBy(n => n.StackNoteOption), "StackNoteOption", "StackNoteOption").ToList();
                this.ViewData["50-0001"] = new SelectList(db.StackNotes.OrderBy(n => n.StackNoteOption), "StackNoteOption", "StackNoteOption").ToList();
                this.ViewData["50-0004"] = new SelectList(db.StackNotes.OrderBy(n => n.StackNoteOption), "StackNoteOption", "StackNoteOption").ToList();
                this.ViewData["50-0140"] = new SelectList(db.StackNotes.OrderBy(n => n.StackNoteOption), "StackNoteOption", "StackNoteOption").ToList();

                return View(model);
            }
        }

        [HttpPost]
        public ActionResult CreateNewStackTest(FormCollection fc)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var sortID = Convert.ToInt32(fc["sortID"]);
                var stackNumber = Convert.ToInt32(fc["stackNumber"]);
                var stackSize = Convert.ToInt32(fc["stackSize"]);
                var numberOfPeople = Convert.ToInt32(fc["numberOfPeople"]);
                var timeOnStack = Convert.ToDouble(fc["timeOnStack"]);
                var stackNotes = fc["stackNotes"].Split(',');

                if (stackSize > 0)
                {
                    var trailer = db.SortListTests.FirstOrDefault(t => t.SortGUID == sortID);
                    trailer.MaxStackNumber = stackNumber;
                    if (trailer.TimeToSort == null)
                    {
                        trailer.TimeToSort = timeOnStack;
                    }
                    else
                    {
                        trailer.TimeToSort += timeOnStack;
                    }

                    var stacks = db.MasterStacksTests.Where(t => t.SortGUID == sortID && t.StackNumber == stackNumber).ToList();
                    if (stacks.Count > 0)
                    {
                        foreach (MasterStacksTest stack in stacks)
                        {
                            db.MasterStacksTests.Remove(stack);
                        }
                    }

                    var counter = 0;
                    for (int x = 5; x < fc.Count - 2; x++)
                    {
                        var key = fc.GetKey(x);
                        var quantity = fc[key];
                        if (key != "stackNotes" && key != "palletTypes" && key != "scrapTypes")
                        {
                            

                            var pallet = db.PalletTypes.FirstOrDefault(p => p.PartNumber == key);

                            if (quantity != "")
                            {
                                if(stackNotes[counter] == "Other")
                                {
                                    counter++;
                                }
                                MasterStacksTest stack = new MasterStacksTest();

                                stack.SortGUID = sortID;
                                stack.StackNumber = stackNumber;
                                stack.PartNumber = pallet.PartNumber;
                                stack.Description = pallet.Description.ToString();
                                stack.Quantity = Convert.ToInt32(quantity);
                                stack.PeopleOnStack = numberOfPeople;
                                stack.UserSignedIn = Session["name"].ToString();
                                stack.PalletNote = stackNotes[counter];
                                

                                db.MasterStacksTests.Add(stack);
                            }
                            counter++;
                        }
                    }

                    var customPallet = fc["customPallet"].Split(',');
                    if (customPallet[0] != "")
                    {
                        MasterStacksTest newStack = new MasterStacksTest()
                        {
                            SortGUID = sortID,
                            StackNumber = stackNumber,
                            PartNumber = "CUSTOM",
                            Description = "CUSTOM - " + customPallet[0] + " X " + customPallet[1],
                            Quantity = Convert.ToInt32(customPallet[2]),
                            PeopleOnStack = numberOfPeople,
                            UserSignedIn = Session["name"].ToString(),
                            PalletNote = stackNotes[stackNotes.Length - 1],
                        };

                        db.MasterStacksTests.Add(newStack);
                    }
                    db.SaveChanges();
                }



                var returnType = fc.GetKey(fc.Count - 1);
                if (returnType == "nextStack")
                {
                    stackNumber++;
                    return RedirectToAction(actionName: "SortTrailerTest", controllerName: "PalletRepair", routeValues: new { sortID, stackNumber, numberOfPeople });
                }
                else
                {
                    return RedirectToAction(actionName: "SortConfirmation", controllerName: "PalletRepair", routeValues: new { sortID });
                }
            }
        }

        [HttpPost]
        public ActionResult CreateSortImageTest(HttpPostedFileBase ImageFile, string vendors, int peopleOnTrailer, int sortID, string stackNotes, string sortType)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if (ModelState.IsValid)
                {
                    var trailer = db.SortLists.FirstOrDefault(t => t.SortGUID == sortID);
                    if (vendors != "")
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
                                Notes = stackNotes,
                                ImagePath = path,
                                DateTaken = DateTime.Now.ToString("yyyy-MM-dd"),
                            };
                            db.SortImages.Add(initialImage);
                        }
                    }
                    db.SaveChanges();

                    if (sortType == "Stack Sort")
                    {
                        return RedirectToAction(actionName: "SortTrailerTest", controllerName: "PalletRepair", routeValues: new { sortID, numberOfPeople = peopleOnTrailer });
                    }
                    else if (sortType == "Bulk Sort")
                    {
                        return RedirectToAction(actionName: "SortStacks", controllerName: "PalletRepair", routeValues: new { sortID });
                    }
                    else
                    {
                        return RedirectToAction(actionName: "SortTrailerTest", controllerName: "PalletRepair", routeValues: new { sortID, numberOfPeople = peopleOnTrailer });
                    }
                }
                else
                {
                    return View();
                }
            }
        }

        public ActionResult CreateStackImageTest(HttpPostedFileBase ImageFile, int sortID, int stackNumber, string stackNotes, int peopleOnStack)
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
                                Notes = stackNotes,
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
    }
}