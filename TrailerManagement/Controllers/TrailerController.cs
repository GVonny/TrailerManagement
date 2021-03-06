﻿using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrailerManagement.Models;

namespace TrailerManagement.Controllers
{
    public class TrailerController : Controller
    {
        Constants constant = new Constants();

        public ActionResult ActiveTrailerList(string SortOrder)
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

                    switch (SortOrder)
                    {
                        case "NEEDEMPTY":
                        trailer = trailer.Where(t => t.TrailerStatus == constant.TRAILER_STATUS_NEED_EMPTY).OrderBy(t => t.DateModified);
                        break;
                        case "READYTOROLL":
                        trailer = trailer.Where(t => t.TrailerStatus == constant.TRAILER_STATUS_READY_TO_ROLL).OrderBy(t => t.DateModified);
                        break;
                        case "LOADING":
                        trailer = trailer.Where(t => t.TrailerStatus == constant.TRAILER_STATUS_LOADING).OrderBy(t => t.DateModified);
                        break;
                        case "EMPTY":
                        trailer = trailer.Where(t => t.TrailerStatus == constant.TRAILER_STATUS_EMPTY).OrderBy(t => t.DateModified);
                        break;
                        case "NEEDWORK":
                        trailer = trailer.Where(t => t.TrailerStatus == constant.TRAILER_STATUS_NEED_WORK).OrderBy(t => t.DateModified);
                        break;
                        case "INTRANSIT":
                        trailer = trailer.Where(t => t.TrailerStatus == constant.TRAILER_STATUS_IN_TRANSIT).OrderBy(t => t.DateModified);
                        break;
                        case "DELIVERED":
                        trailer = trailer.Where(t => t.TrailerStatus == constant.TRAILER_STATUS_DELIVERED).OrderBy(t => t.DateModified);
                        break;
                        case "ALL":
                        trailer = trailer.OrderBy(t => t.TrailerStatus).ThenBy(t => t.DateModified);
                        break;
                        case "CUSTOMER":
                        trailer = trailer.OrderBy(t => t.Customer).ThenBy(t => t.DateModified).ThenBy(t => t.TrailerStatus).Where(t => t.TrailerStatus != constant.TRAILER_STATUS_NEED_WORK && t.Customer != null);
                        break;
                        default:
                        trailer = trailer.OrderBy(t => t.TrailerStatus).ThenBy(t => t.DateModified);
                        break;

                    }
                    if (Convert.ToInt32(Session["permission"]) == constant.PERMISSION_DRIVER)
                    {
                        trailer = trailer.Where(t => t.TrailerStatus == constant.TRAILER_STATUS_NEED_EMPTY || t.TrailerStatus == constant.TRAILER_STATUS_IN_TRANSIT || t.TrailerStatus == constant.TRAILER_STATUS_DELIVERED || t.TrailerStatus == constant.TRAILER_STATUS_READY_TO_ROLL || t.TrailerStatus == constant.TRAILER_STATUS_EMPTY).OrderBy(t => t.TrailerStatus).ThenBy(t => t.TrailerNumber);
                    }
                    return View(trailer.ToList());
                }
            }
        }

        public ActionResult InactiveTrailerList(string SortOrder)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_EDIT)
                {
                    return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                }
                else
                {
                    ViewBag.TrailerNumber = String.IsNullOrEmpty(SortOrder) ? "TrailerNumber_desc" : "";
                    ViewBag.DOTDate = SortOrder == "DotDate" ? "DotDate_desc" : "DotDate";
                    ViewBag.PlateNumber = SortOrder == "PlateNumber_desc" ? "PlateNumber" : "PlateNumber_desc";
                    ViewBag.TrailerLength = SortOrder == "TrailerLength" ? "TrailerLength_desc" : "TrailerLength";
                    ViewBag.DateModified = SortOrder == "DateModified" ? "DateModified_desc" : "DateModified";

                    using (TrailerEntities db = new TrailerEntities())
                    {
                        var trailer = from x in db.InactiveTrailerLists select x;

                        switch (SortOrder)
                        {
                            case "TrailerNumber_desc":
                            trailer = trailer.OrderByDescending(t => t.TrailerNumber);
                            break;
                            case "DotDate_desc":
                            trailer = trailer.OrderByDescending(t => t.DOTDate);
                            break;
                            case "DotDate":
                            trailer = trailer.OrderBy(t => t.DOTDate);
                            break;
                            case "PlateNumber_desc":
                            trailer = trailer.OrderBy(t => t.PlateNumber);
                            break;
                            case "DateModified_desc":
                            trailer = trailer.OrderBy(t => t.DateModified);
                            break;
                            case "DateModified":
                            trailer = trailer.OrderByDescending(t => t.DateModified);
                            break;
                            case "PlateNumber":
                            trailer = trailer.OrderByDescending(t => t.PlateNumber);
                            break;
                            case "TrailerLength_desc":
                            trailer = trailer.OrderBy(t => t.TrailerLength);
                            break;
                            case "TrailerLength":
                            trailer = trailer.OrderByDescending(t => t.TrailerLength);
                            break;
                            default:
                            trailer = trailer.OrderBy(t => t.TrailerGUID);
                            break;
                        }
                        return View(trailer.ToList());
                    }
                }
            }
        }

        public ActionResult TrailerList(string SortOrder)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_MANAGER)
                {
                    return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                }
                else
                {
                    ViewBag.TrailerNumber = String.IsNullOrEmpty(SortOrder) ? "TrailerNumber_desc" : "";
                    ViewBag.DOTDate = SortOrder == "DotDate" ? "DotDate_desc" : "DotDate";
                    ViewBag.PlateNumber = SortOrder == "PlateNumber_desc" ? "PlateNumber" : "PlateNumber_desc";
                    ViewBag.TrailerLocation = SortOrder == "TrailerLocation" ? "TrailerLocation_desc" : "TrailerLocation";
                    ViewBag.TrailerLength = SortOrder == "TrailerLength" ? "TrailerLength_desc" : "TrailerLength";
                    ViewBag.DateModified = SortOrder == "DateModified" ? "DateModified_desc" : "DateModified";

                    using (TrailerEntities db = new TrailerEntities())
                    {
                        var trailer = from x in db.TrailerLists select x;

                        switch (SortOrder)
                        {
                            case "TrailerNumber_desc":
                            trailer = trailer.OrderByDescending(t => t.TrailerNumber);
                            break;
                            case "DotDate_desc":
                            trailer = trailer.OrderByDescending(t => t.DOTDate);
                            break;
                            case "DotDate":
                            trailer = trailer.OrderBy(t => t.DOTDate);
                            break;
                            case "DateModified_desc":
                            trailer = trailer.OrderByDescending(t => t.DateModified);
                            break;
                            case "DateModified":
                            trailer = trailer.OrderBy(t => t.DateModified);
                            break;
                            case "PlateNumber_desc":
                            trailer = trailer.OrderBy(t => t.PlateNumber);
                            break;
                            case "PlateNumber":
                            trailer = trailer.OrderByDescending(t => t.PlateNumber);
                            break;
                            case "TrailerLength_desc":
                            trailer = trailer.OrderBy(t => t.TrailerLength);
                            break;
                            case "TrailerLength":
                            trailer = trailer.OrderByDescending(t => t.TrailerLength);
                            break;
                            case "TrailerLocation_desc":
                            trailer = trailer.OrderBy(t => t.TrailerLocation);
                            break;
                            case "TrailerLocation":
                            trailer = trailer.OrderByDescending(t => t.TrailerLocation);
                            break;
                            default:
                            trailer = trailer.OrderBy(t => t.TrailerNumber);
                            break;
                        }
                        return View(trailer.ToList());
                    }
                }
            }
        }

        public ActionResult CreateTrailer()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_MANAGER)
                {
                    return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                }
                else
                {
                    return View();
                }
            }
        }

        [HttpPost]
        public ActionResult CreateTrailer([Bind(Include = "TrailerNumber,DOTDate,TrailerStatus,Leased,InsuranceCard,Insured,Title,PlateNumber,VinNumber,Manufacturer,ManufactureYear,TrailerLocation,TrailerLength,Issues,DateModified")] TrailerList createTrailer)
        {
            //add new trailer to database
            using (TrailerEntities db = new TrailerEntities())
            {
                if (ModelState.IsValid)
                {
                    //create trailer
                    TrailerList trailer = new TrailerList()
                    {
                        TrailerNumber = createTrailer.TrailerNumber,
                        DOTDate = createTrailer.DOTDate,
                        TrailerStatus = createTrailer.TrailerStatus,
                        Leased = createTrailer.Leased,
                        InsuranceCard = createTrailer.InsuranceCard,
                        Insured = createTrailer.Insured,
                        Title = createTrailer.Title,
                        PlateNumber = createTrailer.PlateNumber,
                        VinNumber = createTrailer.VinNumber,
                        Manufacturer = createTrailer.Manufacturer,
                        ManufactureYear = createTrailer.ManufactureYear,
                        TrailerLocation = createTrailer.TrailerLocation,
                        TrailerLength = createTrailer.TrailerLength,
                        Issues = createTrailer.Issues,
                        DateModified = createTrailer.DateModified
                    };

                    //check to make sure trailer number does not exist already
                    if (db.TrailerLists.Any(t => t.TrailerNumber == trailer.TrailerNumber))
                    {
                        //duplicate trailer number
                        ModelState.AddModelError("TrailerNumber", "Trailer number already exists");
                        return View();
                    }
                    if (createTrailer.TrailerNumber.Contains(" "))
                    {
                        ModelState.AddModelError("TrailerNumber", "Trailer number must not contain any spaces");
                        return View();
                    }

                    db.TrailerLists.Add(trailer);
                    db.SaveChanges();
                    return RedirectToAction(actionName: "TrailerList", controllerName: "Trailer");
                }
                return View();
            }
        }

        public ActionResult EditTrailer(string trailerNumber)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_EDIT)
                {
                    return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                }
                else
                {
                    using (TrailerEntities db = new TrailerEntities())
                    {
                        TrailerList trailer = db.TrailerLists.FirstOrDefault(t => t.TrailerNumber == trailerNumber);
                        
                        return View(trailer);
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult EditTrailer([Bind(Include = "TrailerNumber,DOTDate,TrailerStatus,Leased,InsuranceCard,Insured,Title,PlateNumber,VinNumber,Manufacturer,ManufactureYear,TrailerLocation,TrailerLength,Issues,DateModified")]TrailerList UpdatedTrailer)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                TrailerList trailer = db.TrailerLists.FirstOrDefault(t => t.TrailerNumber == UpdatedTrailer.TrailerNumber);

                try
                {
                    ActiveTrailerList editActiveTrailer = db.ActiveTrailerLists.FirstOrDefault(t => t.TrailerNumber == UpdatedTrailer.TrailerNumber);
                    editActiveTrailer.TrailerLocation = UpdatedTrailer.TrailerLocation;
                }
                catch{}

                trailer.DOTDate = UpdatedTrailer.DOTDate;
                trailer.TrailerStatus = UpdatedTrailer.TrailerStatus;
                trailer.Leased = UpdatedTrailer.Leased;
                trailer.InsuranceCard = UpdatedTrailer.InsuranceCard;
                trailer.Insured = UpdatedTrailer.Insured;
                trailer.Title = UpdatedTrailer.Title;
                trailer.PlateNumber = UpdatedTrailer.PlateNumber;
                trailer.VinNumber = UpdatedTrailer.VinNumber;
                trailer.Manufacturer = UpdatedTrailer.Manufacturer;
                trailer.ManufactureYear = UpdatedTrailer.ManufactureYear;
                trailer.TrailerLength = UpdatedTrailer.TrailerLength;
                trailer.TrailerLocation = UpdatedTrailer.TrailerLocation;
                trailer.Issues = UpdatedTrailer.Issues;
                trailer.DateModified = UpdatedTrailer.DateModified;
                
                db.SaveChanges();
                return RedirectToAction(actionName: "TrailerList", controllerName: "Trailer");
            }
        }
        
        public ActionResult CreateInactiveTrailer(string trailerNumber)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_EDIT)
                    {
                        return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                    }
                    else
                    {
                        TrailerList trailer = db.TrailerLists.FirstOrDefault(t => t.TrailerNumber == trailerNumber);

                        InactiveTrailerList trailerEdit = new InactiveTrailerList()
                        {
                            TrailerNumber = trailer.TrailerNumber,
                            DOTDate = trailer.DOTDate,
                            TrailerStatus = trailer.TrailerStatus,
                            Leased = trailer.Leased,
                            InsuranceCard = trailer.InsuranceCard,
                            Insured = trailer.Insured,
                            Title = trailer.Title,
                            PlateNumber = trailer.PlateNumber,
                            VinNumber = trailer.VinNumber,
                            Manufacturer = trailer.Manufacturer,
                            ManufactureYear = trailer.ManufactureYear,
                            TrailerLength = trailer.TrailerLength,
                            Issues = trailer.Issues,
                            DateModified = trailer.DateModified
                        };
                        db.InactiveTrailerLists.Add(trailerEdit);
                        db.SaveChanges();
                        return RedirectToAction(actionName: "RemoveFromMasterList", controllerName: "Trailer", routeValues: new { trailerNumber });
                    }
                }
            }
        }

        public ActionResult EditInactiveTrailer(string trailerNumber)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_MANAGER)
                {
                    return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                }
                else
                {
                    using (TrailerEntities db = new TrailerEntities())
                    {
                        InactiveTrailerList trailer = db.InactiveTrailerLists.FirstOrDefault(t => t.TrailerNumber == trailerNumber);
                        
                        return View(trailer);
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult EditInactiveTrailer([Bind(Include = "TrailerNumber,DOTDate,TrailerStatus,Leased,InsuranceCard,Insured,Title,PlateNumber,VinNumber,Manufacturer,ManufactureYear,TrailerLength,Issues,Tags,DateModified")]InactiveTrailerList UpdatedTrailer)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                InactiveTrailerList trailer = db.InactiveTrailerLists.FirstOrDefault(t => t.TrailerNumber == UpdatedTrailer.TrailerNumber);
                
                trailer.TrailerNumber = UpdatedTrailer.TrailerNumber;
                trailer.DOTDate = UpdatedTrailer.DOTDate;
                trailer.TrailerStatus = UpdatedTrailer.TrailerStatus;
                trailer.Leased = UpdatedTrailer.Leased;
                trailer.InsuranceCard = UpdatedTrailer.InsuranceCard;
                trailer.Insured = UpdatedTrailer.Insured;
                trailer.Title = UpdatedTrailer.Title;
                trailer.PlateNumber = UpdatedTrailer.PlateNumber;
                trailer.VinNumber = UpdatedTrailer.VinNumber;
                trailer.Manufacturer = UpdatedTrailer.Manufacturer;
                trailer.ManufactureYear = UpdatedTrailer.ManufactureYear;
                trailer.TrailerLocation = UpdatedTrailer.TrailerLocation; 
                trailer.TrailerLength = UpdatedTrailer.TrailerLength;
                trailer.Issues = UpdatedTrailer.Issues;
                trailer.DateModified = UpdatedTrailer.DateModified;
                db.SaveChanges();
                return RedirectToAction(actionName: "InactiveTrailerList", controllerName: "Trailer");
            }
        }

        public ActionResult CreateActiveTrailer(string trailerNumber)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if (Session["username"] == null)
                {
                    return RedirectToAction(actionName: "SignIn", controllerName: "Users");
                }
                else
                {
                    if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_MANAGER)
                    {
                        return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                    }
                    else
                    {
                        TrailerList trailer;
                        try
                        {
                            trailer = db.TrailerLists.FirstOrDefault(t => t.TrailerNumber == trailerNumber);
                            if (db.ActiveTrailerLists.Any(t => t.TrailerNumber == trailer.TrailerNumber))
                            {
                                //duplicate trailer number
                                return RedirectToAction(actionName: "AddToActiveTrailerAlreadyExists", controllerName: "Error");
                            }
                            else
                            {
                                //create active trailer with only 3 fields to copy information, the rest is inputted by user
                                ActiveTrailerList trailerEdit = new ActiveTrailerList()
                                {
                                    TrailerNumber = trailer.TrailerNumber,
                                    TrailerLocation = trailer.TrailerLocation,
                                    DateModified = trailer.DateModified,
                                };
                                db.ActiveTrailerLists.Add(trailerEdit);
                                db.SaveChanges();
                                return RedirectToAction(actionName: "ActiveTrailerList", controllerName: "Trailer");
                            }
                        }
                        catch
                        {
                            //return trailer does not exist page if trailer is not found in try
                            return RedirectToAction(actionName: "AddToActiveTrailerDoesNotExist", controllerName: "Error");
                        }
                    }
                }
            }
        }

        public ActionResult EditActiveTrailer(string trailerNumber)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_DRIVER && 
                   (Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_TRANSPORTATION || Convert.ToInt32(Session["department"]) != constant.DEPARTMENT_SUPER_ADMIN))
                {
                    return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                }
                else
                {
                    using (TrailerEntities db = new TrailerEntities())
                    {
                        ActiveTrailerList trailer = db.ActiveTrailerLists.FirstOrDefault(t => t.TrailerNumber == trailerNumber);
                        
                        return View(trailer);
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult EditActiveTrailer([Bind(Include = "TrailerGUID,TrailerNumber,TrailerStatus,LoadStatus,Customer,OrderDate,OrderNumber,LocationStatus,TrailerLocation,Notes,Tags,DateModified,LastModifiedBy")]ActiveTrailerList UpdatedTrailer)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                ActiveTrailerList trailer = db.ActiveTrailerLists.FirstOrDefault(t => t.TrailerNumber == UpdatedTrailer.TrailerNumber);
                if(!constant.checkIfEqual(trailer, UpdatedTrailer))
                {
                    trailer.DateModified = DateTime.Now.ToString("yyyy-MM-dd");
                }

                TrailerList trailerEdit = db.TrailerLists.FirstOrDefault(t => t.TrailerNumber == UpdatedTrailer.TrailerNumber);

                if (trailer.TrailerStatus != constant.TRAILER_STATUS_NEED_EMPTY && UpdatedTrailer.TrailerStatus == constant.TRAILER_STATUS_NEED_EMPTY)
                {
                    SortList newSort = new SortList()
                    {
                        TrailerNumber = UpdatedTrailer.TrailerNumber,
                        Customer = UpdatedTrailer.Customer,
                        OrderNumber = UpdatedTrailer.OrderNumber,
                        SortType = UpdatedTrailer.Notes,
                        LoadStatus = UpdatedTrailer.LoadStatus,
                        DateArrived = DateTime.Now.ToString("yyyy-MM-dd"),
                        Status = "NEW",
                    };
                    db.SortLists.Add(newSort);
                }

                if(trailerEdit != null)
                {
                    trailerEdit.TrailerLocation = UpdatedTrailer.TrailerLocation;
                }
                
                trailer.TrailerStatus = UpdatedTrailer.TrailerStatus;
                trailer.LoadStatus = UpdatedTrailer.LoadStatus;
                trailer.Customer = UpdatedTrailer.Customer;
                trailer.OrderDate = UpdatedTrailer.OrderDate;
                trailer.OrderNumber = UpdatedTrailer.OrderNumber;
                trailer.LocationStatus = UpdatedTrailer.LocationStatus;

                if(UpdatedTrailer.TrailerStatus == constant.TRAILER_STATUS_DELIVERED)
                {
                    trailer.TrailerLocation = UpdatedTrailer.Customer;
                }
                else
                {
                    trailer.TrailerLocation = UpdatedTrailer.TrailerLocation;
                }

                trailer.Notes = UpdatedTrailer.Notes;
                trailer.Tags = UpdatedTrailer.Tags;
                trailer.LastModifiedBy = Session["name"].ToString();

                db.SaveChanges();
                return RedirectToAction(actionName: "ActiveTrailerList", controllerName: "Trailer");
            }
        }

        public ActionResult Delete(int? id)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_MANAGER)
                {
                    return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                }
                else
                {
                    using (TrailerEntities db = new TrailerEntities())
                    {
                        var trailer = db.InactiveTrailerLists.FirstOrDefault(t => t.TrailerGUID == id);
                        db.InactiveTrailerLists.Remove(trailer);
                        db.SaveChanges();
                        return RedirectToAction(actionName: "InactiveTrailerList", controllerName: "Trailer");
                    }
                }
            }
        }

        public ActionResult RemoveFromMasterList(string trailerNumber)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_MANAGER)
                {
                    return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                }
                else
                {
                    using (TrailerEntities db = new TrailerEntities())
                    {
                        var trailer = db.TrailerLists.FirstOrDefault(t => t.TrailerNumber == trailerNumber);
                        db.TrailerLists.Remove(trailer);
                        var activeTrailer = db.ActiveTrailerLists.FirstOrDefault(t => t.TrailerNumber == trailerNumber);
                        //removes trailer from master and active if trailer exists in active
                        if (activeTrailer != null)
                        {
                            db.ActiveTrailerLists.Remove(activeTrailer);
                        }
                        db.SaveChanges();
                        return RedirectToAction(actionName: "InactiveTrailerList", controllerName: "Trailer");
                    }
                }
            }
        }

        public ActionResult RemoveFromActive(string trailerNumber)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_MANAGER)
                {
                    return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                }
                else
                {
                    using (TrailerEntities db = new TrailerEntities())
                    {
                        var trailer = db.ActiveTrailerLists.FirstOrDefault(t => t.TrailerNumber == trailerNumber);
                        db.ActiveTrailerLists.Remove(trailer);
                        db.SaveChanges();
                        return RedirectToAction(actionName: "ActiveTrailerList", controllerName: "Trailer");
                    }
                }
            }
        }

        public ActionResult CreateDriverConcern()
        {
            if(Session["username"] != null)
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    this.ViewData["concerns"] = new SelectList(db.DriverConcernsLists.OrderBy(t => t.Concern), "Concern", "Concern").ToList();
                }
                return View();
            }
            else
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
        }

        [HttpPost]
        public ActionResult CreateDriverConcern(string customer, string notes)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                DateTime current = DateTime.Now;
                DriverConcern newConcern = new DriverConcern()
                {
                    Customer = customer,
                    DateTaken = current.ToString("yyyy-MM-dd"),
                    DriverSignedIn = Session["name"].ToString(),
                    Notes = notes,
                    Status = "OPEN",
                    CreatedBy = Session["name"].ToString(),
                };
                db.DriverConcerns.Add(newConcern);
                db.SaveChanges();
                return RedirectToAction(actionName: "DriverConcern", controllerName: "Trailer", routeValues: new { driverConcernID = newConcern.DriverConcernGUID });
            }
           
        }

        public ActionResult DriverConcern(int driverConcernID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();
                var concern = db.DriverConcerns.FirstOrDefault(c => c.DriverConcernGUID == driverConcernID);

                var images = db.DriverConcernImages.Where(c => c.DriverConcernGUID == driverConcernID).ToList();

                model.Concern = concern;
                model.Images = images;
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult CreateDriverImage(HttpPostedFileBase ImageFile, string note, int driverConcernID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if(ImageFile == null)
                {
                    return RedirectToAction(actionName: "DriverConcern", controllerName: "Trailer", routeValues: new { driverConcernID });
                }
                var concern = db.DriverConcerns.FirstOrDefault(c => c.DriverConcernGUID == driverConcernID);
                if (ImageFile.ContentLength > 0)
                {
                    var extension = Path.GetExtension(ImageFile.FileName);
                    var fullPath = Server.MapPath("~/DriverImages/") + concern.Customer.ToString() + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();
                    var path = concern.Customer + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + extension.ToLower();

                    ImageFile.SaveAs(fullPath);

                    DriverConcernImage newImage = new DriverConcernImage()
                    {
                        DriverConcernGUID = driverConcernID,
                        ImagePath = path,
                    };

                    if(note != "")
                    {
                        newImage.Note = note;
                    }
                    else
                    {
                        newImage.Note = "No note";
                    }
                    db.DriverConcernImages.Add(newImage);
                    db.SaveChanges();
                }
                return RedirectToAction(actionName: "DriverConcern", controllerName: "Trailer", routeValues: new { driverConcernID });
            }
        }

        public ActionResult RemoveDriverImage(int driverImageConcernID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var concernImage = db.DriverConcernImages.FirstOrDefault(c => c.DriverConcernImageGUID == driverImageConcernID);
                var path = Server.MapPath("~/DriverImages/") + concernImage.ImagePath;
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                db.DriverConcernImages.Remove(concernImage);
                db.SaveChanges();
                return RedirectToAction(actionName: "DriverConcern", controllerName: "Trailer", routeValues: new { driverConcernID = concernImage.DriverConcernGUID });
            }
        }

        [HttpPost]
        public ActionResult ChangeImageNote(int driverImageConcernID, string note)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var concernImage = db.DriverConcernImages.FirstOrDefault(c => c.DriverConcernImageGUID == driverImageConcernID);
                concernImage.Note = note;
                db.SaveChanges();
                return RedirectToAction(actionName: "DriverConcern", controllerName: "Trailer", routeValues: new { driverConcernID = concernImage.DriverConcernGUID });
            }
        }

        public ActionResult MyConcerns()
        {
            if(Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            using (TrailerEntities db = new TrailerEntities())
            {
                DateTime current = DateTime.Now;
                var date = current.ToString("yyyy-MM-dd");
                var user = Session["username"].ToString();
                if(Convert.ToInt32(Session["permission"]) == 10000)
                {
                    var concerns = db.DriverConcerns.Where(c => c.DateTaken == date && c.Status == "OPEN").ToList();
                    return View(concerns);
                }
                else
                {
                    var concerns = db.DriverConcerns.Where(c => c.DriverSignedIn == user && c.DateTaken == date && c.Status == "OPEN").ToList();
                    return View(concerns);
                }
            }
        }

        public ActionResult RemoveConcern(int concernID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var concern = db.DriverConcerns.FirstOrDefault(c => c.DriverConcernGUID == concernID);

                var images = db.DriverConcernImages.Where(c => c.DriverConcernGUID == concernID).ToList();

                if (images.Count > 0)
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
                return RedirectToAction(actionName: "Index", controllerName: "Home");
            }
        }

        public ActionResult AddStorageTrailer(string trailerNumber)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var trailer = db.InactiveTrailerLists.FirstOrDefault(t => t.TrailerNumber == trailerNumber);
                var date = DateTime.Now;


                ActiveTrailerList newTrailer = new ActiveTrailerList()
                {
                    TrailerNumber = trailer.TrailerNumber,
                    TrailerStatus = "8",
                    LoadStatus = "FIXED",
                    LocationStatus = "ON SITE",
                    DateModified = date.ToString("yyyy-MM-dd"),
                    Notes = trailer.TrailerStatus,
                };

                db.ActiveTrailerLists.Add(newTrailer);
                db.SaveChanges();
                return RedirectToAction(actionName: "InactiveTrailerList", controllerName: "Trailer");
            }
        }

        public ActionResult DriverConcernList()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var concerns = db.DriverConcernsLists.OrderBy(c => c.Concern).ToList();
                return View(concerns);
            }
        }

        public ActionResult EditConcernList(int concernID, string concern)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var issue = db.DriverConcernsLists.FirstOrDefault(c => c.DriverConcernListGUID == concernID);
                issue.Concern = concern;
                db.SaveChanges();
                return RedirectToAction(actionName: "DriverConcernList", controllerName: "Trailer");
            }
        }

        public ActionResult DeleteConcernList(int concernID)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var concern = db.DriverConcernsLists.FirstOrDefault(c => c.DriverConcernListGUID == concernID);
                db.DriverConcernsLists.Remove(concern);
                db.SaveChanges();
                return RedirectToAction(actionName: "DriverConcernList", controllerName: "Trailer");
            }
        }

        public ActionResult CreateConcernList(string concern)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                DriverConcernsList newConcern = new DriverConcernsList()
                {
                    Concern = concern,
                };
                db.DriverConcernsLists.Add(newConcern);
                db.SaveChanges();
                return RedirectToAction(actionName: "DriverConcernList", controllerName: "Trailer");
            }
        }
    }
}