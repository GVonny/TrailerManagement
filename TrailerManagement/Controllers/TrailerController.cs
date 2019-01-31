using System;
using System.Linq;
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
                        trailer = trailer.Where(t => t.TrailerStatus == constant.TRAILER_STATUS_IN_TRANSIT || t.TrailerStatus == constant.TRAILER_STATUS_DELIVERED || t.TrailerStatus == constant.TRAILER_STATUS_READY_TO_ROLL).OrderBy(t => t.TrailerStatus).ThenBy(t => t.TrailerNumber);
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

        public ActionResult EditTrailer(string id)
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
                        TrailerList trailer = db.TrailerLists.Find(id);
                        
                        return View(trailer);
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult EditTrailer([Bind(Include = "TrailerNumber,DOTDate,TrailerStatus,Leased,InsuranceCard,Insured,Title,PlateNumber,VinNumber,Manufacturer,ManufactureYear,TrailerLocation,TrailerLength,Issues,DateModified")]TrailerList UpdatedTrailer, string id)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                TrailerList trailer = db.TrailerLists.Find(id);

                try
                {
                    ActiveTrailerList editActiveTrailer = db.ActiveTrailerLists.Find(id);
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
        
        public ActionResult CreateInactiveTrailer(string id)
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
                        TrailerList trailer = db.TrailerLists.Find(id);

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
                        return RedirectToAction(actionName: "RemoveFromMasterList/" + id, controllerName: "Trailer");
                    }
                }
            }
        }

        public ActionResult EditInactiveTrailer(int id)
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
                        InactiveTrailerList trailer = db.InactiveTrailerLists.Find(id);
                        
                        return View(trailer);
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult EditInactiveTrailer([Bind(Include = "TrailerNumber,DOTDate,TrailerStatus,Leased,InsuranceCard,Insured,Title,PlateNumber,VinNumber,Manufacturer,ManufactureYear,TrailerLength,Issues,Tags,DateModified")]InactiveTrailerList UpdatedTrailer, int id)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                InactiveTrailerList trailer = db.InactiveTrailerLists.Find(id);
                
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

        public ActionResult CreateActiveTrailer(string id)
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
                            trailer = db.TrailerLists.Find(id);
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

        public ActionResult EditActiveTrailer(string id)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_DRIVER)
                {
                    return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                }
                else
                {
                    using (TrailerEntities db = new TrailerEntities())
                    {
                        ActiveTrailerList trailer = db.ActiveTrailerLists.Find(id);
                        
                        return View(trailer);
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult EditActiveTrailer([Bind(Include = "TrailerNumber,TrailerStatus,LoadStatus,Customer,OrderDate,OrderNumber,LocationStatus,TrailerLocation,Notes,Tags,DateModified")]ActiveTrailerList UpdatedTrailer, string id)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                ActiveTrailerList trailer = db.ActiveTrailerLists.Find(id);
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
                        DateArrived = UpdatedTrailer.DateModified,
                        Status = "NEW",
                    };
                    db.SortLists.Add(newSort);
                }

                trailerEdit.TrailerLocation = UpdatedTrailer.TrailerLocation;

                trailer.TrailerNumber = UpdatedTrailer.TrailerNumber;
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
                trailer.DateModified = UpdatedTrailer.DateModified;
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
                        var trailer = db.InactiveTrailerLists.Where(t => t.TrailerGUID == id).FirstOrDefault();
                        db.InactiveTrailerLists.Remove(trailer);
                        db.SaveChanges();
                        return RedirectToAction(actionName: "InactiveTrailerList", controllerName: "Trailer");
                    }
                }
            }
        }

        public ActionResult RemoveFromMasterList(string id)
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
                        var trailer = db.TrailerLists.Where(t => t.TrailerNumber == id).FirstOrDefault();
                        db.TrailerLists.Remove(trailer);
                        var activeTrailer = db.ActiveTrailerLists.Where(t => t.TrailerNumber == id).FirstOrDefault();
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

        public ActionResult RemoveFromActive(string id)
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
                        var trailer = db.ActiveTrailerLists.Where(t => t.TrailerNumber == id).FirstOrDefault();
                        db.ActiveTrailerLists.Remove(trailer);
                        db.SaveChanges();
                        return RedirectToAction(actionName: "ActiveTrailerList", controllerName: "Trailer");
                    }
                }
            }
        }
    }
}