using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TrailerManagement.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult AddToActiveTrailerDoesNotExist()
        {
            return View();
        }

        public ActionResult AddToActiveTrailerAlreadyExists()
        {
            return View();
        }

        public ActionResult InsufficientPermissions()
        {
            return View();
        }
    }
}