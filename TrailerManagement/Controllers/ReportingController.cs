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
                var payouts = db.Payouts.Select(p => p.Vendor).Distinct().ToList();
                return View(payouts);
            }
        }

        public ActionResult PayoutInfoByVendor(string vendor)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();

                var stacks = db.CompletedSorts.Where(s => s.Vendor == vendor);

                var uniqueSortIDs = db.CompletedSorts.Select(u => u.SortGUID).Distinct().ToList();

                var payouts = db.Payouts.Where(p => p.Vendor == vendor);

                model.Stacks = stacks.ToList();
                model.SortIDs = uniqueSortIDs;
                model.Payouts = payouts.ToList();

                return View(model);
            }
        }
    }
}