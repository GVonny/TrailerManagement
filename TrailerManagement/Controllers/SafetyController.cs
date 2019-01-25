using Microsoft.Ajax.Utilities;
using Rotativa;
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
        Constants constant = new Constants();

        public ActionResult SafetyCodes(string search)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if(Session["username"] != null && (Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN))
                {
                    var codes = from x in db.SafetyCodes select x;

                    if(search != "" && search != null)
                    {
                        if(search == "INACTIVE")
                        {
                            ViewBag.Closed = true;
                            codes = codes.Where(c => c.Status == "INACTIVE");
                        }
                        else
                        {
                            codes = codes.Where(c => c.Type.ToLower().Contains(search.ToLower()) || c.SubType.ToLower().Contains(search.ToLower()) && c.Status == "ACTIVE").OrderBy(c => c.OshaCode);
                        }
                    }
                    else
                    {
                        codes = codes.Where(c => c.Status == "ACTIVE");
                    }

                    codes = codes.OrderBy(c => c.Type).ThenBy(c => c.Status);

                    return View(codes.ToList());
                }
                else if(Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN)
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
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var code = db.SafetyCodes.FirstOrDefault(c => c.SafetyCodeGUID == safetyCodeID);

                    if(code.Status == "INACTIVE")
                    {
                        ViewBag.Closed = true;
                    }

                    var codes = db.SafetyCodes.DistinctBy(c => c.Type).OrderBy(c => c.Type).ToList();

                    this.ViewData["codeTypes"] = new SelectList(codes, "Type", "Type").ToList();

                    return View(code);
                }
            }
            else if (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN)
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
                    code.TypeSubType = editSafetyCode.Type + " - " + editSafetyCode.SubType;

                    db.SaveChanges();
                    return RedirectToAction(actionName: "SafetyCodes", controllerName: "Safety");
                }
            }
            else if (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN)
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
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var codes = db.SafetyCodes.DistinctBy(c => c.Type).OrderBy(c => c.Type).ToList();

                    this.ViewData["codeTypes"] = new SelectList(codes, "Type", "Type").ToList();

                    return View();
                }
            }
            else if (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN)
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
                        TypeSubType = createSafetyCode.Type + " - " + createSafetyCode.SubType, 
                        Status = "ACTIVE",
                    };

                    db.SafetyCodes.Add(newCode);
                    db.SaveChanges();

                    return RedirectToAction(actionName: "SafetyCodes", controllerName: "Safety");
                }
            }
            else if (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        public ActionResult DisableSafetyCode(int safetyCodeID)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var code = db.SafetyCodes.FirstOrDefault(c => c.SafetyCodeGUID == safetyCodeID);
                    code.Status = "INACTIVE";
                    db.SaveChanges();
                    return RedirectToAction(actionName: "SafetyCodes", controllerName: "Safety");
                }
            }
            else if (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        public ActionResult EnableSafetyCode(int safetyCodeID)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var code = db.SafetyCodes.FirstOrDefault(c => c.SafetyCodeGUID == safetyCodeID);
                    code.Status = "ACTIVE";
                    db.SaveChanges();
                    return RedirectToAction(actionName: "SafetyCodes", controllerName: "Safety");
                }
            }
            else if (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        public ActionResult SafetyAudit(string area)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    dynamic model = new ExpandoObject();
                    var concerns = from x in db.SafetyConcerns select x;


                    List<string> distinctAreas = new List<string>();
                    var areas = concerns.DistinctBy(a => a.Area).OrderBy(a => a.Area);
                    foreach(var a in areas)
                    {
                        distinctAreas.Add(a.Area);
                    }

                    model.Areas = distinctAreas.ToList();

                    if (area != null)
                    {
                        model.Area = area;
                        concerns = concerns.Where(c => c.Status == "OPEN" && c.Area == area).OrderBy(c => c.Area).ThenBy(c => c.SubArea);
                    }
                    else
                    {
                        model.Area = "";
                        concerns = concerns.Where(c => c.Status == "OPEN").OrderBy(c => c.Area).ThenBy(c => c.SubArea);
                    }

                    model.Concerns = concerns.ToList();
                    var violations = db.CodeViolations.ToList();
                    model.Violations = violations;

                    return View(model);
                }
            }
            else if (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        public ActionResult SafetyAuditPDF()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();

                var concerns = db.SafetyConcerns.Where(c => c.Status == "OPEN").OrderBy(c => c.Area).ThenBy(c => c.SubArea).ToList();
                model.Concerns = concerns;

                var violations = db.CodeViolations.ToList();
                model.Violations = violations;

                return View(model);
            }
        }

        public ActionResult ClosedSafetyConcerns()
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    dynamic model = new ExpandoObject();

                    var concerns = db.SafetyConcerns.Where(c => c.Status == "CLOSED").OrderBy(c => c.DateClosed).ToList();
                    model.Concerns = concerns;

                    var violations = db.CodeViolations.ToList();
                    model.Violations = violations;

                    return View(model);
                }
            }
            else if (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }
        
        public ActionResult AddSafetyConcern(string area)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var codes = db.SafetyCodes.OrderBy(c => c.TypeSubType).ToList();

                    ViewBag.Area = area;

                    this.ViewData["typeSubType"] = new SelectList(codes, "TypeSubType", "TypeSubType");
                    this.ViewData["typeSubType2"] = new SelectList(codes, "TypeSubType", "TypeSubType");

                    var areas = db.Departments.Where(a => a.DepartmentName != "ADMIN").OrderBy(a => a.DepartmentName).ToList();
                    this.ViewData["departments"] = new SelectList(areas, "DepartmentName", "DepartmentName").ToList();

                    return View();
                }
            }
            else if (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        [HttpPost]
        public ActionResult CreateSafetyConcern(string departments, string areaNote, string conditionNoted, string typeSubType, string typeSubType2, string correctiveAction, string severity, HttpPostedFileBase ImageFile)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var path = "";
                    if (ImageFile != null)
                    {
                        if (ImageFile.ContentLength > 0)
                        {
                            var extension = Path.GetExtension(ImageFile.FileName);
                            var fullPath = Server.MapPath("~/SafetyImages/") + departments + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();
                            path = departments + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();

                            ImageFile.SaveAs(fullPath);
                        }
                    }

                    if (!conditionNoted.EndsWith(".") && conditionNoted != "")
                    {
                        conditionNoted += ".";
                    }
                    if (!correctiveAction.EndsWith(".") && correctiveAction != "")
                    {
                        correctiveAction += ".";
                    }

                    SafetyConcern newConcern = new SafetyConcern()
                    {
                        Area = departments,
                        
                        ConditionNoted = conditionNoted,
                        CorrectiveAction = correctiveAction,
                        ViolationCount = 1,
                        Status = "OPEN",
                        DateOpened = DateTime.Now.ToString("yyyy-MM-dd"),
                        
                        ImagePath = path,
                    };

                    if(areaNote == "")
                    {
                        newConcern.SubArea = null;
                    }
                    else
                    {
                        newConcern.SubArea = areaNote;
                    }

                    if (severity == "")
                    {
                        newConcern.Severity = "LOW";
                    }
                    else
                    {
                        newConcern.Severity = severity;
                    }
                    db.SafetyConcerns.Add(newConcern);
                    db.SaveChanges();

                    if(typeSubType != "")
                    {
                        var violation = db.SafetyCodes.FirstOrDefault(d => d.TypeSubType == typeSubType);
                        CodeViolation newViolation = new CodeViolation()
                        {
                            SafetyConcernGUID = newConcern.SafetyConcernGUID,
                            Type = violation.Type,
                            OshaCode = violation.OshaCode,
                            Description = violation.Description,
                            TypeSubType = violation.TypeSubType,
                        };
                        db.CodeViolations.Add(newViolation);
                    }
                    if (typeSubType2 != "")
                    {
                        var violation = db.SafetyCodes.FirstOrDefault(d => d.TypeSubType == typeSubType2);
                        CodeViolation newViolation = new CodeViolation()
                        {
                            SafetyConcernGUID = newConcern.SafetyConcernGUID,
                            Type = violation.Type,
                            OshaCode = violation.OshaCode,
                            Description = violation.Description,
                            TypeSubType = violation.TypeSubType,
                        };
                        db.CodeViolations.Add(newViolation);
                    }

                    db.SaveChanges();
                    return RedirectToAction(actionName: "SafetyAudit", controllerName: "Safety", routeValues: new { area = departments });
                }   
            }
            else if (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN)
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
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var concern = db.SafetyConcerns.FirstOrDefault(c => c.SafetyConcernGUID == safetyConcernID);
                    ViewBag.Area = concern.Area;

                    var violations = db.CodeViolations.Where(v => v.SafetyConcernGUID == safetyConcernID);

                    var codes = db.SafetyCodes.OrderBy(c => c.TypeSubType).ToList();

                    List<string> violationsList = new List<string>();
                    foreach(CodeViolation violation in violations)
                    {
                        violationsList.Add(violation.TypeSubType);
                    }

                    ViewData["Violations"] = violationsList;

                    var areas = db.Departments.Where(a => a.DepartmentName != "ADMIN").OrderBy(a => a.DepartmentName).ToList();
                    this.ViewData["departments"] = new SelectList(areas, "DepartmentName", "DepartmentName").ToList();

                    this.ViewData["typeSubType"] = new SelectList(codes, "TypeSubType", "TypeSubType");
                    this.ViewData["typeSubType2"] = new SelectList(codes, "TypeSubType", "TypeSubType");

                    return View(concern);
                }
            }
            else if (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        [HttpPost]
        public ActionResult EditSafetyConcern([Bind(Include = "SafetyConcernGUID,Area,SubArea,ConditionNoted,CorrectiveAction,Severity")] SafetyConcern safetyConcern, string typeSubType, string typeSubType2, HttpPostedFileBase ImageFile)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var concern = db.SafetyConcerns.FirstOrDefault(c => c.SafetyConcernGUID == safetyConcern.SafetyConcernGUID);
                    concern.Area = safetyConcern.Area;
                    if(safetyConcern.SubArea == "")
                    {
                        concern.SubArea = null;
                    }
                    else
                    {
                        concern.SubArea = safetyConcern.SubArea;
                    }

                    if (safetyConcern.ConditionNoted != null)
                    {
                        if (!safetyConcern.ConditionNoted.EndsWith("."))
                        {
                            safetyConcern.ConditionNoted += ".";
                        }
                    
                        concern.ConditionNoted = safetyConcern.ConditionNoted;
                    }
                    if (safetyConcern.CorrectiveAction != null)
                    {
                        if (!safetyConcern.CorrectiveAction.EndsWith("."))
                        {
                            safetyConcern.CorrectiveAction += ".";
                        }
                    
                        concern.CorrectiveAction = safetyConcern.CorrectiveAction;
                    }
                    
                    concern.Severity = safetyConcern.Severity;
                    
                    if (ImageFile != null)
                    {
                        if (ImageFile.ContentLength > 0)
                        {
                            var extension = Path.GetExtension(ImageFile.FileName);
                            var fullPath = Server.MapPath("~/SafetyImages/") + concern.Area + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();
                            var path = concern.Area + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();

                            var oldPath = Server.MapPath("~/SafetyImages/" + concern.ImagePath);
                            if(System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                            
                            ImageFile.SaveAs(fullPath);
                            concern.ImagePath = path;
                        }
                    }

                    var violations = db.CodeViolations.Where(v => v.SafetyConcernGUID == concern.SafetyConcernGUID);
                
                    if (typeSubType != "")
                    {
                        foreach (var v in violations)
                        {
                            db.CodeViolations.Remove(v);
                        }

                        var violation = db.SafetyCodes.FirstOrDefault(d => d.TypeSubType == typeSubType);
                        CodeViolation newViolation = new CodeViolation()
                        {
                            SafetyConcernGUID = concern.SafetyConcernGUID,
                            Type = violation.Type,
                            OshaCode = violation.OshaCode,
                            Description = violation.Description,
                            TypeSubType = violation.TypeSubType,
                        };
                        db.CodeViolations.Add(newViolation);
                    }
                    if (typeSubType2 != "")
                    {
                        var violation = db.SafetyCodes.FirstOrDefault(d => d.TypeSubType == typeSubType2);
                        CodeViolation newViolation = new CodeViolation()
                        {
                            SafetyConcernGUID = concern.SafetyConcernGUID,
                            Type = violation.Type,
                            OshaCode = violation.OshaCode,
                            Description = violation.Description,
                            TypeSubType = violation.TypeSubType,
                        };
                        db.CodeViolations.Add(newViolation);
                    }
                    db.SaveChanges();

                    return RedirectToAction(actionName: "SafetyAudit", controllerName: "Safety", routeValues: new { area = safetyConcern.Area });
                }
            }
            else if (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        public ActionResult CloseSafetyConcern(int safetyConcernID)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var concern = db.SafetyConcerns.FirstOrDefault(c => c.SafetyConcernGUID == safetyConcernID);
                    concern.Status = "CLOSED";
                    concern.DateClosed = DateTime.Now.ToString("yyyy-MM-dd");
                    db.SaveChanges();
                    return RedirectToAction(actionName: "SafetyAudit", controllerName: "Safety");
                }
            }
            else if (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        public ActionResult RepeatSafetyConcern(int safetyConcernID)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var concern = db.SafetyConcerns.FirstOrDefault(c => c.SafetyConcernGUID == safetyConcernID);
                    concern.ViolationCount++;
                    db.SaveChanges();
                    return RedirectToAction(actionName: "SafetyAudit", controllerName: "Safety");
                }
            }
            else if (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        public ActionResult ReOpenSafetyConcern(int safetyConcernID)
        {
            if (Session["username"] != null && (Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) == constant.DEPARTMENT_SUPER_ADMIN))
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    var concern = db.SafetyConcerns.FirstOrDefault(c => c.SafetyConcernGUID == safetyConcernID);
                    concern.Status = "OPEN";
                    concern.ViolationCount++;
                    concern.DateClosed = null;
                    concern.DateOpened = DateTime.Now.ToString("yyyy-MM-dd");
                    db.SaveChanges();
                    return RedirectToAction(actionName: "SafetyAudit", controllerName: "Safety");
                }
            }
            else if (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_HR_SAFETY || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN)
            {
                return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        public ActionResult ViewSafetyAuditPDF()
        {
            var date = DateTime.Now.ToString("MM-dd-yyyy");
            var report = new ActionAsPdf("SafetyAuditPDF"); //{ FileName = "SafetyAudit - " + date + ".pdf" };
            return report;
        }

        public ActionResult ManagerDashboard()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if (Session["username"] != null)
                {
                    dynamic model = new ExpandoObject();

                    //var concerns = from x in db.SafetyConcerns select x;
                    //concerns = concerns.Where(c => c.Status == "OPEN" && c.Area == area).OrderBy(c => c.Area).ThenBy(c => c.SubArea);

                    var department = Session["departmentName"].ToString();
                    if (department == "Admin")
                    {
                        var concerns = db.SafetyConcerns.Where(c => c.Status == "OPEN").OrderBy(c => c.Area).ThenBy(c => c.SubArea).ToList();
                        return View(concerns);
                    }
                    else
                    {
                        var concerns = db.SafetyConcerns.Where(c => c.Status == "OPEN" && c.Area == department && c.SupposedlyFixed != true).ToList();
                        return View(concerns);
                    }
                }
                else
                {
                    return RedirectToAction(actionName: "SignIn", controllerName: "Users");
                }
            }
        }

        public ActionResult MarkConcernAsFixed(int safetyConcernID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var concern = db.SafetyConcerns.FirstOrDefault(c => c.SafetyConcernGUID == safetyConcernID);
                concern.SupposedlyFixed = true;
                db.SaveChanges();
                return RedirectToAction(actionName: "ManagerDashboard", controllerName: "Safety");
            }
        }
    }
}