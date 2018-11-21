using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
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
                if(Session["username"] != null && (Convert.ToInt32(Session["department"]) == 4500 || Convert.ToInt32(Session["department"]) == 10000))
                {
                    var codes = from x in db.SafetyCodes select x;

                    codes = codes.OrderBy(c => c.Type);

                    return View(codes.ToList());
                }
                else if(Convert.ToInt32(Session["department"]) != 4500 || Convert.ToInt32(Session["department"]) != 10000)
                {
                    return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                }
                else
                {
                    return RedirectToAction(actionName: "SignIn", controllerName: "Users");
                }
            }
        }

        public ActionResult EditSafetyCode(int safetyCodeID)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == 4500 || Convert.ToInt32(Session["department"]) == 10000))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var code = db.SafetyCodes.FirstOrDefault(c => c.SafetyCodeGUID == safetyCodeID);

                    var codes = db.SafetyCodes.DistinctBy(c => c.Type).OrderBy(c => c.Type).ToList();

                    this.ViewData["codeTypes"] = new SelectList(codes, "Type", "Type").ToList();

                    return View(code);
                }
            }
            else if (Convert.ToInt32(Session["department"]) != 4500 || Convert.ToInt32(Session["department"]) != 10000)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        [HttpPost]
        public ActionResult EditSafetyCode([Bind(Include = "SafetyCodeGUID,Type,SubType,OshaCode,Description")] SafetyCode editSafetyCode)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == 4500 || Convert.ToInt32(Session["department"]) == 10000))
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
            else if (Convert.ToInt32(Session["department"]) != 4500 || Convert.ToInt32(Session["department"]) != 10000)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        public ActionResult CreateSafetyCode()
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == 4500 || Convert.ToInt32(Session["department"]) == 10000))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var codes = db.SafetyCodes.DistinctBy(c => c.Type).OrderBy(c => c.Type).ToList();

                    this.ViewData["codeTypes"] = new SelectList(codes, "Type", "Type").ToList();

                    return View();
                }
            }
            else if (Convert.ToInt32(Session["department"]) != 4500 || Convert.ToInt32(Session["department"]) != 10000)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        [HttpPost]
        public ActionResult CreateSafetyCode([Bind(Include = "Type,SubType,OshaCode,Description")] SafetyCode createSafetyCode)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == 4500 || Convert.ToInt32(Session["department"]) == 10000))
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
            else if (Convert.ToInt32(Session["department"]) != 4500 || Convert.ToInt32(Session["department"]) != 10000)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        public ActionResult DeleteSafetyCode(int safetyCodeID)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == 4500 || Convert.ToInt32(Session["department"]) == 10000))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var code = db.SafetyCodes.FirstOrDefault(c => c.SafetyCodeGUID == safetyCodeID);
                    db.SafetyCodes.Remove(code);
                    db.SaveChanges();
                    return RedirectToAction(actionName: "SafetyCodes", controllerName: "Safety");
                }
            }
            else if (Convert.ToInt32(Session["department"]) != 4500 || Convert.ToInt32(Session["department"]) != 10000)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        public ActionResult SafetyConcerns()
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == 4500 || Convert.ToInt32(Session["department"]) == 10000))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    dynamic model = new ExpandoObject();

                    var concerns = db.SafetyConcerns.ToList();
                    model.Concerns = concerns;

                    var violations = db.CodeViolations.ToList();
                    model.Violations = violations;
                
                    return View(model);
                }
            }
            else if (Convert.ToInt32(Session["department"]) != 4500 || Convert.ToInt32(Session["department"]) != 10000)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        public ActionResult AddSafetyConcern()
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == 4500 || Convert.ToInt32(Session["department"]) == 10000))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var codes = db.SafetyCodes.DistinctBy(c => c.OshaCode).OrderBy(c => c.OshaCode).ToList();
                    this.ViewData["codeTypes"] = new SelectList(codes, "OshaCode", "OshaCode").ToList();
                    this.ViewData["codeTypes2"] = new SelectList(codes, "OshaCode", "OshaCode").ToList();
                    this.ViewData["codeTypes3"] = new SelectList(codes, "OshaCode", "OshaCode").ToList();

                    return View();
                }
            }
            else if (Convert.ToInt32(Session["department"]) != 4500 || Convert.ToInt32(Session["department"]) != 10000)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        [HttpPost]
        public ActionResult CreateSafetyConcern(string area, string conditionNoted, string codeTypes, string codeTypes2, string codeTypes3, string correctiveAction, HttpPostedFileBase ImageFile)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == 4500 || Convert.ToInt32(Session["department"]) == 10000))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var path = "";
                    if (ImageFile != null)
                    {
                        if (ImageFile.ContentLength > 0)
                        {
                            var extension = Path.GetExtension(ImageFile.FileName);
                            var fullPath = Server.MapPath("~/SafetyImages/") + area + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();
                            path = area + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();

                            ImageFile.SaveAs(fullPath);
                        }
                    }

                    SafetyConcern newConcern = new SafetyConcern()
                    {
                        Area = area,
                        ConditionNoted = conditionNoted,
                        CorrectiveActionMeasure = correctiveAction,
                        Severity = "NEW",
                        ImagePath = path,
                    };
                    db.SafetyConcerns.Add(newConcern);
                    db.SaveChanges();

                    if(codeTypes != "")
                    {
                        var description = db.SafetyCodes.FirstOrDefault(d => d.OshaCode == codeTypes);
                        CodeViolation newViolation = new CodeViolation()
                        {
                            SafetyConcernGUID = newConcern.SafetyConcernGUID,
                            Type = description.Type,
                            ViolationCode = codeTypes,
                            Description = description.Description,
                        };
                        db.CodeViolations.Add(newViolation);
                    }
                    if (codeTypes2 != "")
                    {
                        var description = db.SafetyCodes.FirstOrDefault(d => d.OshaCode == codeTypes2);
                        CodeViolation newViolation = new CodeViolation()
                        {
                            SafetyConcernGUID = newConcern.SafetyConcernGUID,
                            Type = description.Type,
                            ViolationCode = codeTypes2,
                            Description = description.Description,
                        };
                        db.CodeViolations.Add(newViolation);
                    }
                    if (codeTypes3 != "")
                    {
                        var description = db.SafetyCodes.FirstOrDefault(d => d.OshaCode == codeTypes3);
                        CodeViolation newViolation = new CodeViolation()
                        {
                            SafetyConcernGUID = newConcern.SafetyConcernGUID,
                            Type = description.Type,
                            ViolationCode = codeTypes3,
                            Description = description.Description,
                        };
                        db.CodeViolations.Add(newViolation);
                    }

                    db.SaveChanges();
                    return RedirectToAction(actionName: "SafetyConcerns", controllerName: "Safety");
                }   
            }
            else if (Convert.ToInt32(Session["department"]) != 4500 || Convert.ToInt32(Session["department"]) != 10000)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        public ActionResult EditSafetyConcern(int safetyConcernID)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == 4500 || Convert.ToInt32(Session["department"]) == 10000))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var concern = db.SafetyConcerns.FirstOrDefault(c => c.SafetyConcernGUID == safetyConcernID);

                    var violations = db.CodeViolations.Where(v => v.SafetyConcernGUID == safetyConcernID);

                    var codes = db.SafetyCodes.DistinctBy(c => c.OshaCode).OrderBy(c => c.OshaCode).ToList();
                    this.ViewData["codeTypes"] = new SelectList(codes, "OshaCode", "OshaCode").ToList();
                    this.ViewData["codeTypes2"] = new SelectList(codes, "OshaCode", "OshaCode").ToList();
                    this.ViewData["codeTypes3"] = new SelectList(codes, "OshaCode", "OshaCode").ToList();

                    return View(concern);
                }
            }
            else if (Convert.ToInt32(Session["department"]) != 4500 || Convert.ToInt32(Session["department"]) != 10000)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        [HttpPost]
        public ActionResult EditSafetyConcern([Bind(Include = "SafetyConcernGUID,Area,ConditionNoted,CorrectiveActionMeasure")] SafetyConcern safetyConcern, string codeTypes, string codeTypes2, string codeTypes3, HttpPostedFileBase ImageFile)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == 4500 || Convert.ToInt32(Session["department"]) == 10000))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var concern = db.SafetyConcerns.FirstOrDefault(c => c.SafetyConcernGUID == safetyConcern.SafetyConcernGUID);
                    concern.Area = safetyConcern.Area;
                    concern.ConditionNoted = safetyConcern.ConditionNoted;
                    concern.CorrectiveActionMeasure = safetyConcern.CorrectiveActionMeasure;

                    var path = "";
                    if (ImageFile != null)
                    {
                        if (ImageFile.ContentLength > 0)
                        {
                            var extension = Path.GetExtension(ImageFile.FileName);
                            var fullPath = Server.MapPath("~/SafetyImages/") + concern.Area + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();
                            path = concern.Area + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();

                            ImageFile.SaveAs(fullPath);
                            concern.ImagePath = path;
                        }
                    }

                    var violations = db.CodeViolations.Where(v => v.SafetyConcernGUID == concern.SafetyConcernGUID);
                
                    if (codeTypes != "")
                    {
                        foreach (var v in violations)
                        {
                            db.CodeViolations.Remove(v);
                        }

                        var description = db.SafetyCodes.FirstOrDefault(d => d.OshaCode == codeTypes);
                        CodeViolation newViolation = new CodeViolation()
                        {
                            SafetyConcernGUID = concern.SafetyConcernGUID,
                            Type = description.Type,
                            ViolationCode = codeTypes,
                            Description = description.Description,
                        };
                        db.CodeViolations.Add(newViolation);
                    }
                    if (codeTypes2 != "")
                    {
                        var description = db.SafetyCodes.FirstOrDefault(d => d.OshaCode == codeTypes2);
                        CodeViolation newViolation = new CodeViolation()
                        {
                            SafetyConcernGUID = concern.SafetyConcernGUID,
                            Type = description.Type,
                            ViolationCode = codeTypes2,
                            Description = description.Description,
                        };
                        db.CodeViolations.Add(newViolation);
                    }
                    if (codeTypes3 != "")
                    {
                        var description = db.SafetyCodes.FirstOrDefault(d => d.OshaCode == codeTypes3);
                        CodeViolation newViolation = new CodeViolation()
                        {
                            SafetyConcernGUID = concern.SafetyConcernGUID,
                            Type = description.Type,
                            ViolationCode = codeTypes3,
                            Description = description.Description,
                        };
                        db.CodeViolations.Add(newViolation);
                    }
                    db.SaveChanges();

                    return RedirectToAction(actionName: "SafetyConcerns", controllerName: "Safety");
                }
            }
            else if (Convert.ToInt32(Session["department"]) != 4500 || Convert.ToInt32(Session["department"]) != 10000)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        public ActionResult DeleteSafetyConcern(int safetyConcernID)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == 4500 || Convert.ToInt32(Session["department"]) == 10000))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var concern = db.SafetyConcerns.FirstOrDefault(c => c.SafetyConcernGUID == safetyConcernID);
                    db.SafetyConcerns.Remove(concern);
                    db.SaveChanges();
                    return RedirectToAction(actionName: "SafetyConcerns", controllerName: "Safety");
                }
            }
            else if (Convert.ToInt32(Session["department"]) != 4500 || Convert.ToInt32(Session["department"]) != 10000)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }
    }
}