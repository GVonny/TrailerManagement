using System;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;
using TrailerManagement.Models;

namespace TrailerManagement.Controllers
{
    public class UsersController : Controller
    {
        Constants constant = new Constants();

        public ActionResult CreateUser()
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_ADMIN)
                {
                    return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                }
                else
                {
                    using (TrailerEntities db = new TrailerEntities())
                    {
                        this.ViewData["departments"] = new SelectList(db.Departments, "DepartmentNumber", "DepartmentName").ToList();
                        return View();
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult CreateUser([Bind(Include = "Username,Password,Department,Permission,FirstName")] CreateUser createUser)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if (ModelState.IsValid)
                {
                    User newUser = createUser.MapToUser();

                    if (db.Users.Any(u => u.Username == newUser.Username))
                    {
                        ModelState.AddModelError("Username", "Username is already in use");
                        return View();
                    }
                    db.Users.Add(newUser);
                    db.SaveChanges();
                    return RedirectToAction(actionName: "UserList", controllerName: "Users");
                }
                return View();
            }
        }

        public ActionResult UserList(string SortOrder)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_ADMIN)
                    {
                        return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                    }
                    else
                    {
                        ViewBag.Username = String.IsNullOrEmpty(SortOrder) ? "Username_desc" : "";
                        ViewBag.Permission = SortOrder == "Permission" ? "Permission_desc" : "Permission";

                        var user = from x in db.Users select x;
                        var department = Session["department"].ToString();
                        if(!department.Equals("10000"))
                        {
                            user = user.Where(t => t.Permission != constant.PERMISSION_ADMIN.ToString() && t.Department.Equals(department));
                        }
                        

                        switch (SortOrder)
                        {
                            case "Username_desc":
                            user = user.OrderByDescending(t => t.Username);
                            break;
                            case "Permission":
                            user = user.OrderByDescending(t => t.Permission);
                            break;
                            case "Permission_desc":
                            user = user.OrderBy(t => t.Permission);
                            break;
                            default:
                            user = user.OrderBy(t => t.Username);
                            break;
                        }

                        dynamic model = new ExpandoObject();
                        var departments = db.Departments.ToList();
                        model.Departments = departments;
                        model.Users = user.ToList();


                        return View(model);
                    }
                }
            }
        }

        public ActionResult EditUser(int id)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    this.ViewData["departments"] = new SelectList(db.Departments, "DepartmentNumber", "DepartmentName").ToList();

                    var user = db.Users.Find(id);
                    if (user.UserGUID != Convert.ToInt32(Session["userID"]) && (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_ADMIN))
                    {
                        return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                    }
                    else
                    {
                        user = db.Users.Find(id);

                        EditUser userEdit = new EditUser()
                        {
                            Username = user.Username,
                            Password = user.Password,
                            Department = user.Department,
                            Permission = user.Permission,
                            FirstName = user.FirstName
                        };
                        return View(userEdit);
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult EditUser([Bind(Include = "Username,Password,Department,Permission,FirstName")] EditUser updatedUser, int id)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                User user = db.Users.Find(id);

                if (db.Users.Any(t => t.Username == updatedUser.Username) && (user.Username != updatedUser.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists");
                    if (updatedUser.Password == null)
                    {
                        ModelState.AddModelError("Password", "Enter a new password or cancel");
                    }
                    return View();
                }
                else if (updatedUser.Username == null)
                {
                    ModelState.AddModelError("Username", "Username required");
                    return View();
                }
                else if (updatedUser.Password == null)
                {
                    ModelState.AddModelError("Password", "Password is required to save");
                    return View();
                }
                else if (updatedUser.FirstName == null)
                {
                    ModelState.AddModelError("FirstName", "Name field is required");
                    return View();
                }
                else
                {
                    if(Session["name"].Equals(updatedUser.FirstName))
                    {
                        Session["name"] = updatedUser.FirstName;
                        Session["permission"] = updatedUser.Permission;
                        Session["department"] = updatedUser.Department;
                    }
                    
                    user.Username = updatedUser.Username;
                    user.Password = updatedUser.Password;
                    user.Department = updatedUser.Department;
                    user.FirstName = updatedUser.FirstName;
                    user.Permission = updatedUser.Permission;

                    db.SaveChanges();
                }

                if ((Convert.ToInt32(Session["permission"]) > 30))
                {
                    return RedirectToAction(actionName: "UserList", controllerName: "Users");
                }
                else
                {
                    return RedirectToAction(actionName: "Index", controllerName: "Home");
                }
            }
        }

        public ActionResult SignIn()
        {
            if (Session["username"] != null)
            {
                return RedirectToAction(actionName: "Index", controllerName: "Home");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult SignIn([Bind(Include = "Username,Password")]SignIn userSignIn)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if (ModelState.IsValid)
                {
                    User user = db.Users.FirstOrDefault(u => u.Username == userSignIn.Username);
                    

                    if (user == null)
                    {
                        ModelState.AddModelError("Username", "User not found");
                        return View();
                    }
                    else if (user.Password != null && user.Password == userSignIn.Password)
                    {
                        Session["username"] = user.Username;
                        Session["permission"] = user.Permission;
                        Session["department"] = user.Department;

                        var num = Convert.ToInt32(user.Department);
                        var department = db.Departments.FirstOrDefault(d => d.DepartmentNumber == num);
                        Session["departmentName"] = department.DepartmentName;

                        Session["name"] = user.FirstName;
                        Session["userID"] = user.UserGUID;
                        Session.Timeout = 60;
                        return RedirectToAction(actionName: "Index", controllerName: "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("Password", "Incorrect password");
                        return View();
                    }
                }
                return View();
            }
        }

        public ActionResult SignOut()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if (Session["username"] == null)
                {
                    return RedirectToAction(actionName: "SignIn", controllerName: "Users");
                }

                Session["username"] = null;
                Session["permission"] = null;
                Session["department"] = null;
                Session["name"] = null;
                return RedirectToAction(actionName:"SignIn", controllerName:"Users");
            }
        }

        public ActionResult RemoveUser(string username)
        {
            if (Session["username"] == null)
            {
                return RedirectToAction(actionName: "SignIn", controllerName: "Users");
            }
            else
            {
                using (TrailerEntities db = new TrailerEntities())
                {
                    if (Convert.ToInt32(Session["permission"]) < constant.PERMISSION_ADMIN)
                    {
                        return RedirectToAction(actionName: "InsufficientPermissions", controllerName: "Error");
                    }
                    else
                    {
                        var user = db.Users.Where(u => u.Username == username).FirstOrDefault();
                        if (user.Username == Session["username"].ToString())
                        {
                            SignOut();
                        }
                        db.Users.Remove(user);
                        db.SaveChanges();
                        return RedirectToAction(actionName: "UserList", controllerName: "Users");
                    }
                }
            }
        }
    }
}