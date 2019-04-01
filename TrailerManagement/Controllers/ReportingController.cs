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
    public class ReportingController : Controller
    {
        Constants constant = new Constants();

        public ActionResult InsuranceInfo()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if (Session["username"] == null)
                {
                    return RedirectToAction(actionName: "SignIn", controllerName: "Users");
                }
                if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_ADMIN)
                {
                    return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                }
                var trailer = from x in db.TrailerLists select x;

                trailer = trailer.OrderBy(t => t.TrailerNumber);
                return View(trailer.ToList());
            }
        }

        public ActionResult ActiveListReporting(Boolean? needEmpty, Boolean? readyToRoll, Boolean? loading, Boolean? empty, Boolean? needWork, Boolean? inTransit, Boolean? delivered)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var trailer = from x in db.ActiveTrailerLists select x;

                    int counter = 0;
                    ArrayList values = new ArrayList();

                    if (needEmpty == true)
                    {
                        counter++;
                        values.Add(constant.TRAILER_STATUS_NEED_EMPTY);
                    }
                    if (readyToRoll == true)
                    {
                        counter++;
                        values.Add(constant.TRAILER_STATUS_READY_TO_ROLL);
                    }
                    if (loading == true)
                    {
                        counter++;
                        values.Add(constant.TRAILER_STATUS_LOADING);
                    }
                    if (empty == true)
                    {
                        counter++;
                        values.Add(constant.TRAILER_STATUS_EMPTY);
                    }
                    if (needWork == true)
                    {
                        counter++;
                        values.Add(constant.TRAILER_STATUS_NEED_WORK);
                    }
                    if (inTransit == true)
                    {
                        counter++;
                        values.Add(constant.TRAILER_STATUS_IN_TRANSIT);
                    }
                    if (delivered == true)
                    {
                        counter++;
                        values.Add(constant.TRAILER_STATUS_DELIVERED);
                    }

                    if (counter == 1)
                    {
                        var value1 = values[0].ToString();
                        trailer = trailer.Where(t => t.TrailerStatus == value1).OrderBy(t => t.DateModified);
                    }
                    else if (counter == 2)
                    {
                        var value1 = values[0].ToString();
                        var value2 = values[1].ToString();
                        trailer = trailer.Where(t => t.TrailerStatus == value1 || t.TrailerStatus == value2).OrderBy(t => t.TrailerStatus).ThenBy(t => t.TrailerNumber);
                    }
                    else if (counter == 3)
                    {
                        var value1 = values[0].ToString();
                        var value2 = values[1].ToString();
                        var value3 = values[2].ToString();
                        trailer = trailer.Where(t => t.TrailerStatus == value1 || t.TrailerStatus == value2 || t.TrailerStatus == value3).OrderBy(t => t.TrailerStatus).ThenBy(t => t.TrailerNumber);
                    }
                    else if (counter == 4)
                    {
                        var value1 = values[0].ToString();
                        var value2 = values[1].ToString();
                        var value3 = values[2].ToString();
                        var value4 = values[3].ToString();
                        trailer = trailer.Where(t => t.TrailerStatus == value1 || t.TrailerStatus == value2 || t.TrailerStatus == value3 || t.TrailerStatus == value4).OrderBy(t => t.TrailerStatus).ThenBy(t => t.TrailerNumber);
                    }
                    else if (counter == 5)
                    {
                        var value1 = values[0].ToString();
                        var value2 = values[1].ToString();
                        var value3 = values[2].ToString();
                        var value4 = values[3].ToString();
                        var value5 = values[4].ToString();
                        trailer = trailer.Where(t => t.TrailerStatus == value1 || t.TrailerStatus == value2 || t.TrailerStatus == value3 || t.TrailerStatus == value4 || t.TrailerStatus == value5).OrderBy(t => t.TrailerStatus).ThenBy(t => t.TrailerNumber);
                    }
                    else if (counter == 6)
                    {
                        var value1 = values[0].ToString();
                        var value2 = values[1].ToString();
                        var value3 = values[2].ToString();
                        var value4 = values[3].ToString();
                        var value5 = values[4].ToString();
                        var value6 = values[5].ToString();
                        trailer = trailer.Where(t => t.TrailerStatus == value1 || t.TrailerStatus == value2 || t.TrailerStatus == value3 || t.TrailerStatus == value4 || t.TrailerStatus == value5 || t.TrailerStatus == value6).OrderBy(t => t.TrailerStatus).ThenBy(t => t.TrailerNumber);
                    }
                    else if (counter == 7)
                    {
                        var value1 = values[0].ToString();
                        var value2 = values[1].ToString();
                        var value3 = values[2].ToString();
                        var value4 = values[3].ToString();
                        var value5 = values[4].ToString();
                        var value6 = values[5].ToString();
                        var value7 = values[6].ToString();
                        trailer = trailer.Where(t => t.TrailerStatus == value1 || t.TrailerStatus == value2 || t.TrailerStatus == value3 || t.TrailerStatus == value4 || t.TrailerStatus == value5 || t.TrailerStatus == value6 || t.TrailerStatus == value7).OrderBy(t => t.TrailerStatus).ThenBy(t => t.TrailerNumber);
                    }
                    else
                    {
                        trailer = trailer.OrderBy(t => t.TrailerStatus);
                    }
                    return View(trailer.ToList());
                }
            }
        }

        public ActionResult PayoutVendors()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var payouts = db.Payouts.Where(p => p.Vendor != "" && p.Vendor != null).Select(p => p.Vendor).Distinct().ToList();
                return View(payouts);
            }
        }

        public ActionResult PayoutInfoByVendor(string vendor)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();

                if(vendor.Contains("@"))
                {
                    vendor = vendor.Replace('@', '&');
                }

                ViewBag.Vendor = vendor;

                var stacks = db.CompletedSorts.Where(s => s.Vendor == vendor).OrderBy(s => s.SortGUID).ThenBy(s => s.PartNumber).ThenByDescending(s => s.Quantity);

                var payouts = db.Payouts.Where(p => p.Vendor == vendor).OrderByDescending(p => p.DateCompleted).ToList();

                List<String> users = new List<string>();
                List<int> sortIDs = new List<int>();
                List<double> unloadTimes = new List<double>();
                List<int> pictureCount = new List<int>();
                foreach(Payout payout in payouts)
                {
                    var sort = db.MasterStacks.Where(s => s.SortGUID == payout.SortGUID).ToList();
                    if(sort.Count > 0)
                    {
                        var stack = sort.Last();
                        var user = stack.UserSignedIn;
                        unloadTimes.Add(Convert.ToDouble(payout.TimeToSort));
                        sortIDs.Add(Convert.ToInt32(stack.SortGUID));
                        users.Add(user);

                        var imageCount = db.SortImages.Where(i => i.SortGUID == payout.SortGUID).Count();
                        pictureCount.Add(imageCount);
                    }
                    
                }

                model.Users = users;
                model.Times = unloadTimes;
                model.SortIDs = sortIDs;
                model.Stacks = stacks.ToList();
                model.Payouts = payouts;
                model.ImageCount = pictureCount;

                return View(model);
            }
        }

        public ActionResult PayoutImagesByVendor(int sortID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var images = db.SortImages.Where(i => i.SortGUID == sortID).OrderBy(i => i.StackNumber).ToList();
                var date = images.Last().DateTaken;
                ViewBag.Date = date;
                return View(images);
            }
        }

        public ActionResult PayoutInfoByPart(string vendor, string startDate, string endDate)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if(vendor.Contains("@"))
                {
                    vendor = vendor.Replace('@', '&');
                }

                dynamic model = new ExpandoObject();
                ViewBag.Vendor = vendor;

                if (startDate == null && endDate == null)
                {
                    ViewBag.Data = "first";
                    model.Quantities = "";
                    model.Parts = "";
                    return View();
                }
                else
                {
                    var browser = Request.Browser.Type;

                    DateTime beginningDate;
                    DateTime endingDate;

                    if(browser.Contains("InternetExplorer"))
                    {
                        var month = startDate.Substring(0, 2);
                        var day = startDate.Substring(3, 2);
                        var year = startDate.Substring(6, 4);

                        beginningDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));

                        month = endDate.Substring(0, 2);
                        day = endDate.Substring(3, 2);
                        year = endDate.Substring(6, 4);

                        endingDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));

                        ViewBag.StartDate = beginningDate.ToString("MM/dd/yyyy");
                        ViewBag.EndDate = endingDate.ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        var year = Convert.ToInt32(startDate.Substring(0, 4));
                        var month = Convert.ToInt32(startDate.Substring(5, 2));
                        var day = Convert.ToInt32(startDate.Substring(8, 2));

                        beginningDate = new DateTime(year, month, day);
                        year = Convert.ToInt32(endDate.Substring(0, 4));
                        month = Convert.ToInt32(endDate.Substring(5, 2));
                        day = Convert.ToInt32(endDate.Substring(8, 2));
                        endingDate = new DateTime(year, month, day);

                        ViewBag.StartDate = beginningDate.ToString("MM/dd/yyyy");
                        ViewBag.EndDate = endingDate.ToString("MM/dd/yyyy");
                    }
                    var sorts = db.SortLists.Where(s => s.Vendor == vendor && s.Status == "CLOSED").ToList();

                    List<Int32> sortIDs = new List<int>();

                    foreach(SortList sort in sorts)
                    {
                        var date = sort.DateCompleted;
                        var year = Convert.ToInt32(date.Substring(0, 4));
                        var month = Convert.ToInt32(date.Substring(5, 2));
                        var day = Convert.ToInt32(date.Substring(8, 2));

                        DateTime dateCompleted = new DateTime(year, month, day);

                        if(dateCompleted > beginningDate && dateCompleted < endingDate)
                        {
                            sortIDs.Add(Convert.ToInt32(sort.SortGUID));
                        }
                    }

                    List<CompletedSort> completeStacks = new List<CompletedSort>();

                    foreach(Int32 sortID in sortIDs)
                    {
                        var stacks = db.CompletedSorts.Where(s => s.Vendor == vendor && s.SortGUID == sortID).ToList();
                        foreach(CompletedSort stack in stacks)
                        {
                            completeStacks.Add(stack);
                        }
                    }

                    List<string> uniqueParts = new List<string>();
                    //db.CompletedSorts.Where(u => u.Vendor == vendor).OrderBy(u => u.PartNumber).Select(u => u.PartNumber).Distinct().ToList();
                    if(completeStacks.Count >= 1)
                    {
                        foreach (CompletedSort stack in completeStacks)
                        {
                            if (!uniqueParts.Contains(stack.PartNumber))
                            {
                                uniqueParts.Add(stack.PartNumber);
                            }
                        }

                        double[] lastCosts = new double[uniqueParts.Count];
                        for(int x = 0; x < uniqueParts.Count; x++)
                        {
                            var partNumber = uniqueParts[x];
                            var part = completeStacks.Where(p => p.PartNumber == partNumber && p.Vendor == vendor).Last();
                            if(part.Cost == null)
                            {
                                lastCosts[x] = 0;
                            }
                            else
                            {
                                lastCosts[x] = Convert.ToDouble(part.Cost);
                            }
                        }

                        int[] quantities = new int[uniqueParts.Count];
                        int[] averageCount = new int[uniqueParts.Count];

                        double[] addedTotalCosts = new double[uniqueParts.Count];
                        double[] totalCosts = new double[uniqueParts.Count];

                        foreach (CompletedSort stack in completeStacks)
                        {
                            if (uniqueParts.Contains(stack.PartNumber))
                            {
                                var index = uniqueParts.IndexOf(stack.PartNumber);
                                quantities[index] += Convert.ToInt32(stack.Quantity);
                                totalCosts[index] += (Convert.ToInt32(stack.Quantity) * Convert.ToDouble(stack.Cost));

                                if (stack.Cost == null)
                                {
                                    addedTotalCosts[index] += 0;
                                }
                                else
                                {
                                    addedTotalCosts[index] += Convert.ToDouble(stack.Cost);
                                }
                                averageCount[index]++;
                            }
                        }
                        
                        double[] averageCosts = new double[uniqueParts.Count];
                        for (int x = 0; x < averageCosts.Length; x++)
                        {
                            averageCosts[x] = addedTotalCosts[x] / averageCount[x];
                        }

                        model.SortIDs = sortIDs;
                        model.LastCosts = lastCosts;
                        model.TotalCost = totalCosts;
                        model.AverageCost = averageCosts;
                        model.Quantities = quantities;
                        model.Parts = uniqueParts;
                        ViewBag.Data = true;

                        return View(model);
                    }
                    else
                    {
                        ViewBag.Data = false;
                        model.Quantities = "";
                        model.Parts = "";
                        return View();
                    }
                }
            }
        }

        public ActionResult PartsRecievedByDate(string date)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();
                if (date == null || date == "")
                {
                    ViewBag.Data = "first";
                    model.Quantities = "";
                    model.Parts = "";
                    return View();
                }

                var month = "";
                var day = "";
                var year = "";
                var browser = Request.Browser.Type;

                if (Request.Browser.Type.Contains("InternetExplorer"))
                {
                    month = date.Substring(0, 2);
                    day = date.Substring(3, 2);
                    year = date.Substring(6, 4);
                    
                    date = (year.ToString() + '-' + month.ToString() + '-' + day.ToString());
                }
                else
                {
                    year = date.Substring(0, 4);
                    month = date.Substring(5, 2);
                    day = date.Substring(8, 2);
                }
               
                var sorts = db.SortLists.Where(s => s.DateCompleted == date).ToList();
                
                DateTime reportDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));
                
                List<CompletedSort> completeStacks = new List<CompletedSort>();
                foreach (SortList sort in sorts)
                {
                    var stacks = db.CompletedSorts.Where(s => s.SortGUID == sort.SortGUID && s.PartNumber != "DEDUCTION").OrderBy(s => s.PartNumber).ToList();
                    foreach (CompletedSort stack in stacks)
                    {
                        completeStacks.Add(stack);
                    }
                }
                var sortedStacks = completeStacks.OrderBy(s => s.PartNumber).ToList();
                List<string> uniqueParts = new List<string>();
                //db.CompletedSorts.Where(u => u.Vendor == vendor).OrderBy(u => u.PartNumber).Select(u => u.PartNumber).Distinct().ToList();
                if (completeStacks.Count >= 1)
                {
                    foreach (CompletedSort stack in sortedStacks)
                    {
                        if (!uniqueParts.Contains(stack.PartNumber))
                        {
                            uniqueParts.Add(stack.PartNumber);
                        }
                    }

                    int[] quantities = new int[uniqueParts.Count];
                    double[] prices = new double[uniqueParts.Count];

                    foreach (CompletedSort stack in sortedStacks)
                    {
                        if (uniqueParts.Contains(stack.PartNumber))
                        {
                            var index = uniqueParts.IndexOf(stack.PartNumber);
                            quantities[index] += Convert.ToInt32(stack.Quantity);
                            var vendor = stack.Vendor;
                            var partNumber = stack.PartNumber;
                            var quantity = Convert.ToDouble(stack.Quantity);

                            var price = db.PalletPrices.Where(p => p.VendorName == vendor && p.PartNumber == partNumber).FirstOrDefault();
                            if(price != null)
                            {
                                var purchasePrice = Convert.ToDouble(price.PurchasePrice);
                                var cost = (purchasePrice * quantity);
                                prices[index] += cost;
                            }
                        }
                    }
                    model.Quantities = quantities;
                    model.Parts = uniqueParts;
                    model.Prices = prices;
                    ViewBag.Data = true;
                    ViewBag.Date = reportDate.ToString("MM-dd-yyyy");
                    return View(model);
                }
                else
                {
                    ViewBag.Data = false;
                    model.Quantities = "";
                    model.Parts = "";
                    return View();
                }
            }
        }

        public ActionResult PartsReceivedBetweenDates(string startDate, string endDate)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();
                if (startDate == null && endDate == null)
                {
                    ViewBag.Data = "first";
                    model.Quantities = "";
                    model.Parts = "";
                    return View();
                }
                else
                {
                    var browser = Request.Browser.Type;

                    DateTime beginningDate;
                    DateTime endingDate;

                    if (browser.Contains("InternetExplorer"))
                    {
                        var month = startDate.Substring(0, 2);
                        var day = startDate.Substring(3, 2);
                        var year = startDate.Substring(6, 4);

                        beginningDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));

                        month = endDate.Substring(0, 2);
                        day = endDate.Substring(3, 2);
                        year = endDate.Substring(6, 4);

                        endingDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));

                        ViewBag.StartDate = beginningDate.ToString("MM/dd/yyyy");
                        ViewBag.EndDate = endingDate.ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        var year = Convert.ToInt32(startDate.Substring(0, 4));
                        var month = Convert.ToInt32(startDate.Substring(5, 2));
                        var day = Convert.ToInt32(startDate.Substring(8, 2));

                        beginningDate = new DateTime(year, month, day);
                        year = Convert.ToInt32(endDate.Substring(0, 4));
                        month = Convert.ToInt32(endDate.Substring(5, 2));
                        day = Convert.ToInt32(endDate.Substring(8, 2));
                        endingDate = new DateTime(year, month, day);

                        ViewBag.StartDate = beginningDate.ToString("MM/dd/yyyy");
                        ViewBag.EndDate = endingDate.ToString("MM/dd/yyyy");
                    }

                    var sorts = db.SortLists.Where(s => s.Status == "CLOSED").OrderBy(s => s.DateCompleted).ToList();

                    List<Int32> sortIDs = new List<int>();

                    foreach (SortList sort in sorts)
                    {
                        var date = sort.DateCompleted;
                        var year = Convert.ToInt32(date.Substring(0, 4));
                        var month = Convert.ToInt32(date.Substring(5, 2));
                        var day = Convert.ToInt32(date.Substring(8, 2));

                        DateTime dateCompleted = new DateTime(year, month, day);

                        if (dateCompleted >= beginningDate && dateCompleted <= endingDate)
                        {
                            sortIDs.Add(Convert.ToInt32(sort.SortGUID));
                        }
                    }

                    List<CompletedSort> completeStacks = new List<CompletedSort>();

                    foreach (Int32 sortID in sortIDs)
                    {
                        var stacks = db.CompletedSorts.Where(s => s.SortGUID == sortID && s.PartNumber != "DEDUCTION").ToList();
                        foreach (CompletedSort stack in stacks)
                        {
                            completeStacks.Add(stack);
                        }
                    }
                    List<string> uniqueParts = new List<string>();
                    if (completeStacks.Count > 1)
                    {
                        foreach (CompletedSort stack in completeStacks)
                        {
                            if (!uniqueParts.Contains(stack.PartNumber))
                            {
                                uniqueParts.Add(stack.PartNumber);
                            }
                        }

                        uniqueParts = uniqueParts.OrderBy(u => u).ToList();

                        List<String> descriptions = new List<string>();
                        foreach(var part in uniqueParts)
                        {
                            var partMaster = db.PalletTypes.FirstOrDefault(p => p.PartNumber == part);
                            if(partMaster != null)
                            {
                                descriptions.Add(partMaster.Description);
                            }
                            else
                            {
                                descriptions.Add("CUSTOM");
                            }
                            
                        }

                        int[] quantities = new int[uniqueParts.Count];
                        int[] averageCount = new int[uniqueParts.Count];

                        double[] addedTotalCosts = new double[uniqueParts.Count];
                        double[] totalCosts = new double[uniqueParts.Count];

                        foreach (CompletedSort stack in completeStacks)
                        {
                            if (uniqueParts.Contains(stack.PartNumber))
                            {
                                var index = uniqueParts.IndexOf(stack.PartNumber);
                                quantities[index] += Convert.ToInt32(stack.Quantity);
                                totalCosts[index] += (Convert.ToInt32(stack.Quantity) * Convert.ToDouble(stack.Cost));

                                if (stack.Cost == null)
                                {
                                    addedTotalCosts[index] += 0;
                                }
                                else
                                {
                                    addedTotalCosts[index] += Convert.ToDouble(stack.Cost);
                                }
                                averageCount[index]++;
                            }
                        }

                        double[] averageCosts = new double[uniqueParts.Count];
                        for (int x = 0; x < averageCosts.Length; x++)
                        {
                            averageCosts[x] = addedTotalCosts[x] / averageCount[x];
                        }


                        model.Descriptions = descriptions;
                        model.TotalCost = totalCosts;
                        model.AverageCost = averageCosts;
                        model.Quantities = quantities;
                        model.Parts = uniqueParts;
                        ViewBag.Data = true;

                        return View(model);
                    }
                    else
                    {
                        ViewBag.Data = false;
                        model.Quantities = "";
                        model.Parts = "";
                        return View();
                    }
                }
            }
        }

        public ActionResult PartsByVendorBetweenDates(string startDate, string endDate, string partNumber)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();

                PalletType part = db.PalletTypes.FirstOrDefault(p => p.PartNumber == partNumber);

                DateTime beginningDate;
                DateTime endingDate;
                
                var month = startDate.Substring(0, 2);
                var day = startDate.Substring(3, 2);
                var year = startDate.Substring(6, 4);

                beginningDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));

                month = endDate.Substring(0, 2);
                day = endDate.Substring(3, 2);
                year = endDate.Substring(6, 4);

                endingDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));

                ViewBag.StartDate = beginningDate.ToString("MM/dd/yyyy");
                ViewBag.EndDate = endingDate.ToString("MM/dd/yyyy");

                var sorts = db.SortLists.Where(s => s.Status == "CLOSED").ToList();

                List<Int32> sortIDs = new List<int>();

                foreach (SortList sort in sorts)
                {
                    var date = sort.DateCompleted;
                    year = date.Substring(0, 4);
                    month = date.Substring(5, 2);
                    day = date.Substring(8, 2);

                    DateTime dateCompleted = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day));

                    if (dateCompleted >= beginningDate && dateCompleted <= endingDate)
                    {
                        sortIDs.Add(Convert.ToInt32(sort.SortGUID));
                    }
                }

                List<CompletedSort> completeStacks = new List<CompletedSort>();

                foreach (Int32 sortID in sortIDs)
                {
                    //&& (s.Vendor != "" && s.Vendor != null)
                    var stacks = db.CompletedSorts.Where(s => s.PartNumber == partNumber && s.SortGUID == sortID ).ToList();
                    foreach (CompletedSort stack in stacks)
                    {
                        completeStacks.Add(stack);
                    }
                }

                completeStacks = completeStacks.OrderBy(c => c.Vendor).ToList();

                List<String> uniqueVendors = new List<string>();
                

                foreach (CompletedSort stack in completeStacks)
                {
                    if(!uniqueVendors.Contains(stack.Vendor))
                    {
                        uniqueVendors.Add(stack.Vendor);
                    }
                }

                int[] quantities = new int[uniqueVendors.Count];
                int[] averageCount = new int[uniqueVendors.Count];

                double[] addedTotalCosts = new double[uniqueVendors.Count];
                double[] totalCosts = new double[uniqueVendors.Count];
                
                foreach (CompletedSort stack in completeStacks)
                {
                    var index = uniqueVendors.IndexOf(stack.Vendor);
                    quantities[index] += Convert.ToInt32(stack.Quantity);
                    totalCosts[index] += (Convert.ToInt32(stack.Quantity) * Convert.ToDouble(stack.Cost));

                    if (stack.Cost == null)
                    {
                        addedTotalCosts[index] += 0;
                    }
                    else
                    {
                        addedTotalCosts[index] += Convert.ToDouble(stack.Cost);
                    }
                    averageCount[index]++;
                }

                double[] averageCosts = new double[uniqueVendors.Count];
                for (int x = 0; x < averageCosts.Length; x++)
                {
                    averageCosts[x] = addedTotalCosts[x] / averageCount[x];
                }

                model.Vendors = uniqueVendors;
                model.Part = part;
                model.Quantity = quantities;
                model.TotalCost = totalCosts;
                model.AverageCost = averageCosts;

                return View(model);
            }
        }

        public ActionResult ActiveTrailerListFileReport()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var trailers = db.ActiveTrailerLists.OrderBy(t => t.TrailerStatus).ToList();

                var filePath = Server.MapPath("~/Reports/") + "ActiveList.csv";
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Trailer Number,Trailer Status,Load Status,Customer,Location Status,TrailerLocaion,Notes,Tags,Date Modified");
                    foreach (ActiveTrailerList trailer in trailers)
                    {
                        var trailerStatus = trailer.TrailerStatus;
                        switch(trailerStatus)
                        {
                            case "1":
                                trailerStatus = "NEED EMPTY";
                                break;
                            case "2":
                                trailerStatus = "READY TO ROLL";
                                break;
                            case "3":
                                trailerStatus = "LOADING";
                                break;
                            case "4":
                                trailerStatus = "EMPTY";
                                break;
                            case "5":
                                trailerStatus = "NEED WORK";
                                break;
                            case "6":
                                trailerStatus = "IN TRANSIT";
                                break;
                            case "7":
                                trailerStatus = "DELIVERED";
                                break;
                        }

                        writer.WriteLine(trailer.TrailerNumber + ", " +
                                         trailerStatus + ", " +
                                         trailer.LoadStatus + ", " +
                                         trailer.Customer + ", " +
                                         trailer.LocationStatus + ", " +
                                         trailer.TrailerLocation + ", " +
                                         trailer.Notes + ", " +
                                         trailer.Tags + ", " +
                                         trailer.DateModified);
                    }
                }

                SmtpClient client = new SmtpClient("smtp.outlook.com", 587);
                client.EnableSsl = true;
                client.Timeout = 100000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("grant.vonhaden@palletusa.com", Properties.Settings.Default.EmailPassword);

                string body = "Attached is the current Active Trailer Board in an excell file.";
                var username = Session["username"].ToString();
                var user = db.Users.FirstOrDefault(u => u.Username == username);
                var email = user.Email;

                MailMessage message = new MailMessage("grant.vonhaden@palletusa.com", email, "Active List Report", body);

                message.Attachments.Add(new Attachment(filePath));
                message.IsBodyHtml = true;
                message.BodyEncoding = UTF8Encoding.UTF8;
                client.Send(message);

                return View();
            }
        }

        public ActionResult PayoutInfoFileReport()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();

                var payouts = db.Payouts.Where(p => p.Status == "CLOSED" && (p.Vendor != "" && p.Vendor != null)).OrderBy(p => p.Vendor).ThenBy(p => p.DateCompleted).ToList();

                var filePath = Server.MapPath("~/Reports/") + "Payouts.csv";

                List<Object> stacksByPayout = new List<Object>();

                try
                {
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        writer.WriteLine("Vendor,Vendor Number,Date,Part Number,Quantity,Description,Price,Total");
                        foreach (Payout payout in payouts)
                        {
                            var stacks = db.CompletedSorts.Where(s => s.SortGUID == payout.SortGUID).ToList();
                            foreach (CompletedSort stack in stacks)
                            {
                                double total;
                                double cost;
                                if (stack.Cost != null)
                                {
                                    cost = Convert.ToDouble(stack.Cost);
                                    total = Convert.ToDouble(stack.Quantity * cost);
                                }
                                else
                                {
                                    cost = 0;
                                    total = 0;
                                }

                                writer.WriteLine(payout.Vendor + "," + payout.VendorNumber + "," + payout.DateCompleted + "," + stack.PartNumber + "," + stack.Quantity + "," + stack.Description + ",$" + cost + ",$" + total);
                            }
                        }
                    }

                    SmtpClient client = new SmtpClient("smtp.outlook.com", 587);
                    client.EnableSsl = true;
                    client.Timeout = 100000;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("grant.vonhaden@palletusa.com", Properties.Settings.Default.EmailPassword);

                    string body = "Attached is the list of all Payout info in an excell file.";


                    MailMessage message = new MailMessage("grant.vonhaden@palletusa.com", "janice.vonhaden@palletusa.com", "Payout Report", body);

                    message.Attachments.Add(new Attachment(filePath));
                    message.IsBodyHtml = true;
                    message.BodyEncoding = UTF8Encoding.UTF8;
                    client.Send(message);
                }
                catch(Exception ex)
                {
                }

                return View();
            }
        }

        public ActionResult FileAlreadyOpen()
        {
            return View();
        }

        public ActionResult DriverCustomers(string order)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var concerns = db.DriverConcerns.OrderBy(c => c.Customer).ToList();

                List<string> uniqueCustomers = new List<string>();

                foreach(DriverConcern concern in concerns)
                {
                    if(!uniqueCustomers.Contains(concern.Customer))
                    {
                        uniqueCustomers.Add(concern.Customer);
                    }
                }
                return View(uniqueCustomers);
            }
        }

        public ActionResult CustomerList(string customer)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var customers = db.DriverConcerns.Where(c => c.Customer == customer).OrderByDescending(c => c.DateTaken).ToList();

                if(customers.Count == 0)
                {
                    return RedirectToAction(actionName: "DriverCustomers", controllerName: "Reporting");
                }

                this.ViewData["driver"] = new SelectList(db.Users.Where(u => u.Permission == "20"), "FirstName", "FirstName").ToList();

                return View(customers);
            }
        }

        public ActionResult CustomerInfo(int driverConcernID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();
                var concern = db.DriverConcerns.FirstOrDefault(c => c.DriverConcernGUID == driverConcernID);

                var images = db.DriverConcernImages.Where(c => c.DriverConcernGUID == driverConcernID).ToList();

                this.ViewData["driver"] = new SelectList(db.Users.Where(u => u.Permission == "20"), "FirstName", "FirstName").ToList();

                model.Concern = concern;
                model.Images = images;
                return View(model);
            }
        }

        public ActionResult DeleteInfo(int driverConcernID, string customer)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var concern = db.DriverConcerns.FirstOrDefault(c => c.DriverConcernGUID == driverConcernID);

                var images = db.DriverConcernImages.Where(c => c.DriverConcernGUID == driverConcernID).ToList();

                if(images.Count > 0)
                {
                    foreach (DriverConcernImage image in images)
                    {
                        var path = Server.MapPath("~/DriverImages/" + image.ImagePath);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                        db.DriverConcernImages.Remove(image);
                    }
                }

                db.DriverConcerns.Remove(concern);
                db.SaveChanges();

                return RedirectToAction(actionName: "CustomerList", controllerName: "Reporting", routeValues: new { customer });
            }
        }

        public ActionResult EditImageNote(int imageID, int concernID, string note)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var image = db.DriverConcernImages.FirstOrDefault(i => i.DriverConcernImageGUID == imageID);
                image.Note = note;
                db.SaveChanges();
                return RedirectToAction(actionName: "CustomerInfo", controllerName: "Reporting", routeValues: new { driverConcernID = concernID });
            }
        }

        public ActionResult EditCustomerInfo(int concernID, string customer, string note, string driver)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var concern = db.DriverConcerns.FirstOrDefault(c => c.DriverConcernGUID == concernID);
                var oldCustomer = concern.Customer;

                concern.Customer = customer;
                concern.Notes = note;
                concern.DriverSignedIn = driver;
                db.SaveChanges();

                return RedirectToAction(actionName: "CustomerList", controllerName: "Reporting", routeValues: new { customer = oldCustomer });
            }
        }

        public ActionResult EditCustomerHeaderInfo(int concernID, string customer, string note, string driver)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var concern = db.DriverConcerns.FirstOrDefault(c => c.DriverConcernGUID == concernID);
                var oldCustomer = concern.Customer;

                concern.Customer = customer;
                concern.Notes = note;
                concern.DriverSignedIn = driver;
                db.SaveChanges();

                return RedirectToAction(actionName: "CustomerInfo", controllerName: "Reporting", routeValues: new { driverConcernID = concern.DriverConcernGUID });
            }
        }

        public ActionResult ProductionDates()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();
                List<String> uniqueDates = db.ProductionStacks.Select(u => u.Date).Distinct().ToList();

                model.Dates = uniqueDates;

                return View(model);
            }
        }

        public ActionResult ProductionDateInfo(string date)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();
                var stacks = db.ProductionStacks.Where(s => s.Date == date).ToList();
                model.Stacks = stacks;

                var workstations = db.ProductionStacks.Where(w => w.Date == date).Select(w => w.WorkstationNumber).Distinct().ToList();
                model.Workstations = workstations;

                List<String> users = new List<string>();
                List<int?> a = new List<int?>();
                List<int?> b = new List<int?>();
                List<int?> six = new List<int?>();
                List<IndividualWorkstation> individualWorkstations = new List<IndividualWorkstation>();
                foreach(int workstation in workstations)
                {
                    var user = stacks.FirstOrDefault(u => u.WorkstationNumber == workstation).EmployeeName;
                    users.Add(user);


                    var partStacks = stacks.Where(p => p.WorkstationNumber == workstation);
                    IndividualWorkstation individualWorkstation = new IndividualWorkstation();
                    foreach(ProductionStack stack in partStacks)
                    {
                        switch(stack.PartNumber)
                        {
                            case "50-0001":
                                a.Add(stack.StackQuantity);
                                break;
                            case "50-0004":
                                b.Add(stack.StackQuantity);
                                break;
                            case "50-0308":
                                b.Add(stack.StackQuantity);
                                break;
                        }
                        individualWorkstation.SetA(a);
                        individualWorkstation.SetB(b);
                        individualWorkstation.SetSix(six);
                        individualWorkstations.Add(individualWorkstation);
                    }
                }
                model.Users = users;
                model.IndividualWorkstations = individualWorkstations;

                var parts = db.ProductionWorkOrders.ToList();
                model.Parts = parts;

                return View(model);
            }
        }
    }
}