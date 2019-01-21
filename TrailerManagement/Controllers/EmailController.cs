﻿using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using TrailerManagement.Models;

namespace TrailerManagement.Controllers
{
    public class EmailController : Controller
    {
        public ActionResult NewPayout(int sortID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var sortedTrailer = db.SortLists.FirstOrDefault(s => s.SortGUID == sortID);

                SmtpClient client = new SmtpClient("smtp.outlook.com", 587);
                client.EnableSsl = true;
                client.Timeout = 100000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("grant.vonhaden@palletusa.com", Properties.Settings.Default.EmailPassword);
                
                string vendor;
                string trailerNumber;
                string body;
                if(sortedTrailer.Vendor != null)
                {
                    vendor = sortedTrailer.Vendor.ToString().ToLower();
                    body = "Nancy,<br><br> " + "A new payout was created for " + vendor + "<br><br> This is an autogenerated message.";
                }
                else
                {
                    trailerNumber = sortedTrailer.TrailerNumber;
                    body = "Nancy,<br><br> " + "A new payout was created for trailer number" + trailerNumber + " with no vendor attached. <br><br> This is an autogenerated message.";
                }
                
                //MailMessage message = new MailMessage("grant.vonhaden@palletusa.com", "grant.vonhaden@palletusa.com", "New Payout", body);

                //uncomment to send emails to nancy
                string nancyEmail = "nancy.waddell@palletusa.com";
                MailMessage message = new MailMessage("grant.vonhaden@palletusa.com", nancyEmail, "New Payout", body);
                message.IsBodyHtml = true;
                message.BodyEncoding = UTF8Encoding.UTF8;
                //uncomment to send emails to nancy
                client.Send(message);
                
                if (sortedTrailer != null)
                {
                    sortedTrailer.Status = "CLOSED";

                    var date = DateTime.Now;
                    var day = date.Day.ToString();
                    if (day.Length == 1)
                    {
                        day = "0" + day;
                    }
                    var month = date.Month.ToString();
                    if (month.Length == 1)
                    {
                        month = "0" + month;
                    }
                    var year = date.Year;
                    sortedTrailer.DateCompleted = (year + "-" + month + "-" + day);

                    db.SaveChanges();
                }

                //uncomment to send Cesar emails when trailers are empty
                //string cesarEmail = "cesar.bautista@palletusa.com";
                //body = "Cesar,<br><br> Trailer " + sortedTrailer.TrailerNumber + " is now empty at the PR docks. The trailer status has been changed to EMPTY on the Active List.<br><br>This is an autogenerated message.";
                //message = new MailMessage("grant.vonhaden@palletusa.com", cesarEmail, "Empty Trailer", body);
                //message.IsBodyHtml = true;
                //message.BodyEncoding = UTF8Encoding.UTF8;
                //client.Send(message);

                return RedirectToAction(actionName: "SortList", controllerName: "PalletRepair");
            }
        }
        
        public ActionResult CompletePayout(int sortID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var payout = db.Payouts.FirstOrDefault(p => p.SortGUID == sortID);
                DateTime date = DateTime.Now;
                var currentDate = date.ToString();
                
                var day = date.Day.ToString();
                if(day.Length == 1)
                {
                    day = "0" + day;
                }
                var month = date.Month.ToString();
                if (month.Length == 1)
                {
                    month = "0" + month;
                }
                var year = date.Year;
                payout.DateCompleted = (year + "-" + month + "-" + day);
                db.SaveChanges();

                SmtpClient client = new SmtpClient("smtp.outlook.com", 587);
                client.EnableSsl = true;
                client.Timeout = 100000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("grant.vonhaden@palletusa.com", Properties.Settings.Default.EmailPassword);

                string vendor;
                string body = "";
                if (payout.Vendor != null)
                {
                    vendor = payout.Vendor.ToString();
                    body = "A new completed payout was created for " + vendor + "<br><br> <a href=\"trailermanagement.palletusa.local/PalletRepair/ViewCompletedPayout?sortID=" + sortID.ToString() + "\"> Click to view </a><br><br> This is an autogenerated message.";
                }
                else
                {
                    body = "A new completed payout was created <br><br> <a href=\"trailermanagement.palletusa.local/PalletRepair/ViewCompletedPayout?sortID=" + sortID.ToString() + "\"> Click to view </a><br><br> This is an autogenerated message.";
                }

                //MailMessage message = new MailMessage("grant.vonhaden@palletusa.com", "grant.vonhaden@palletusa.com", "New Payout", body);

                string accountingEmail = "accounting@palletusa.com";
                MailMessage message = new MailMessage("grant.vonhaden@palletusa.com", accountingEmail, "New Payout", body);
                message.IsBodyHtml = true;
                message.BodyEncoding = UTF8Encoding.UTF8;
                //uncomment to send emails to ginger
                client.Send(message);
                
                return RedirectToAction(actionName: "ViewCompletedPayout", controllerName: "PalletRepair", routeValues: new { sortID });
            }
        }
    }
}