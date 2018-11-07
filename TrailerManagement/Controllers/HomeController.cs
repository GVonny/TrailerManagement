using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TrailerManagement.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            return View();
        }
        
        //Made controller changes, simple redirects for the time being
        public ActionResult ActiveTrailerList()
        {
            return RedirectToAction(actionName: "ActiveTrailerList", controllerName: "Trailer");
        }

        public ActionResult TrailerList()
        {
            return RedirectToAction(actionName: "TrailerList", controllerName: "Trailer");
        }

        public ActionResult InactiveTrailerList()
        {
            return RedirectToAction(actionName: "InactiveTrailerList", controllerName: "Trailer");
        }

        public ActionResult CreateTrailer()
        {
            return RedirectToAction(actionName: "CreateTrailer", controllerName: "Trailer");
        }
    }
}