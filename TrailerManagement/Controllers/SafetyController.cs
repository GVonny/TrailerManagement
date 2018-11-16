using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrailerManagement.Models;

namespace TrailerManagement.Controllers
{
    public class SafetyController : Controller
    {
        public ActionResult SafetyCodes()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var codes = from x in db.SafetyCodes select x;

                codes = codes.OrderBy(c => c.Type);

                return View(codes.ToList());
            }
        }

        public ActionResult EditSafetyCode(int safetyCodeID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var code = db.SafetyCodes.FirstOrDefault(c => c.SafetyCodeGUID == safetyCodeID);

                var codes = db.SafetyCodes.DistinctBy(c => c.Type).ToList();

                this.ViewData["codeTypes"] = new SelectList(codes, "Type", "Type").ToList();

                return View(code);
            }
        }

        [HttpPost]
        public ActionResult EditSafetyCode([Bind(Include = "SafetyCodeGUID,Type,SubType,OshaCode,Description")] SafetyCode editSafetyCode)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var code = db.SafetyCodes.FirstOrDefault(c => c.SafetyCodeGUID == editSafetyCode.SafetyCodeGUID);

                code.Type = editSafetyCode.Type;
                code.SubType = editSafetyCode.SubType;
                code.OshaCode = editSafetyCode.OshaCode;
                code.Description = editSafetyCode.Description;

                db.SaveChanges();
                return RedirectToAction(actionName: "SafetyCodes", controllerName: "Safety");
            }
        }

        public ActionResult CreateSafetyCode()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var codes = db.SafetyCodes.DistinctBy(c => c.Type).ToList();

                this.ViewData["codeTypes"] = new SelectList(codes, "Type", "Type").ToList();

                return View();
            }
        }

        [HttpPost]
        public ActionResult CreateSafetyCode([Bind(Include = "Type,SubType,OshaCode,Description")] SafetyCode createSafetyCode)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                SafetyCode newCode = new SafetyCode()
                {
                    Type = createSafetyCode.Type,
                    SubType = createSafetyCode.SubType,
                    OshaCode = createSafetyCode.OshaCode,
                    Description = createSafetyCode.Description,
                };

                db.SafetyCodes.Add(newCode);
                db.SaveChanges();

                return RedirectToAction(actionName: "SafetyCodes", controllerName: "Safety");
            }
        }

        public ActionResult DeleteSafetyCode(int safetyCodeID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var code = db.SafetyCodes.FirstOrDefault(c => c.SafetyCodeGUID == safetyCodeID);
                db.SafetyCodes.Remove(code);
                db.SaveChanges();
                return RedirectToAction(actionName: "SafetyCodes", controllerName: "Safety");
            }
        }
    }
}