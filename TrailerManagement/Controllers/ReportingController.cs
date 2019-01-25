using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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

                var payouts = db.Payouts.Where(p => p.Vendor == vendor).OrderBy(p => p.DateCompleted).ToList();

                List<String> users = new List<string>();
                List<int> sortIDs = new List<int>();
                List<double> unloadTimes = new List<double>();
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
                    }
                    
                }

                model.Users = users;
                model.Times = unloadTimes;
                model.SortIDs = sortIDs;
                model.Stacks = stacks.ToList();
                model.Payouts = payouts;

                return View(model);
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
                    var year = Convert.ToInt32(startDate.Substring(0, 4));
                    var month = Convert.ToInt32(startDate.Substring(5, 2));
                    var day = Convert.ToInt32(startDate.Substring(8, 2));

                    var beginningDate = new DateTime(year, month, day);
                    year = Convert.ToInt32(endDate.Substring(0, 4));
                    month = Convert.ToInt32(endDate.Substring(5, 2));
                    day = Convert.ToInt32(endDate.Substring(8, 2));

                    var endingDate = new DateTime(year, month, day);


                    ViewBag.StartDate = beginningDate.ToString("MM/dd/yyyy");
                    ViewBag.EndDate = endingDate.ToString("MM/dd/yyyy");

                    var sorts = db.SortLists.Where(s => s.Vendor == vendor);

                    List<Int32> sortIDs = new List<int>();

                    foreach(SortList sort in sorts)
                    {
                        var date = sort.DateCompleted;
                        year = Convert.ToInt32(date.Substring(0, 4));
                        month = Convert.ToInt32(date.Substring(5, 2));
                        day = Convert.ToInt32(date.Substring(8, 2));

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
               
                var sorts = db.SortLists.Where(s => s.DateCompleted == date).ToList();

                var year = Convert.ToInt32(date.Substring(0, 4));
                var month = Convert.ToInt32(date.Substring(5, 2));
                var day = Convert.ToInt32(date.Substring(8, 2));

                DateTime reportDate = new DateTime(year, month, day);
                
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
    }
}