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
                if (Session["name"].ToString() != "Bruce" && Session["name"].ToString() != "Grant")
                {
                    return RedirectToAction(actionName: "Index", controllerName: "Home");
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
                var payouts = db.Payouts.Where(p => p.Vendor != "").Select(p => p.Vendor).Distinct().ToList();
                return View(payouts);
            }
        }

        public ActionResult PayoutInfoByVendor(string vendor)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();

                ViewBag.Vendor = vendor;

                var stacks = db.CompletedSorts.Where(s => s.Vendor == vendor).OrderBy(s => s.SortGUID).ThenBy(s => s.PartNumber).ThenByDescending(s => s.Quantity);

                var payouts = db.Payouts.Where(p => p.Vendor == vendor).OrderBy(p => p.DateCompleted);

                model.Stacks = stacks.ToList();
                model.Payouts = payouts.ToList();

                return View(model);
            }
        }

        public ActionResult PayoutInfoByPart(string vendor, string startDate, string endDate)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();
                ViewBag.Vendor = vendor;

                if (startDate == null && endDate == null)
                {
                    ViewBag.Data = false;
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
                    if(completeStacks.Count > 1)
                    {
                        foreach (CompletedSort stack in completeStacks)
                        {
                            if (!uniqueParts.Contains(stack.PartNumber))
                            {
                                uniqueParts.Add(stack.PartNumber);
                            }
                        }

                        int[] quantities = new int[uniqueParts.Count];
                        foreach (CompletedSort stack in completeStacks)
                        {
                            if (uniqueParts.Contains(stack.PartNumber))
                            {
                                var index = uniqueParts.IndexOf(stack.PartNumber);
                                quantities[index] += Convert.ToInt32(stack.Quantity);
                            }
                        }
                        
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
    }
}