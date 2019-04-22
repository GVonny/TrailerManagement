using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrailerManagement.Models;

namespace TrailerManagement.Controllers
{
    public class ProductionController : Controller
    {
        public ActionResult WorkStations()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                dynamic model = new ExpandoObject();
                var workstations = db.Workstations.OrderBy(w => w.WorkstationNumber).ToList();
                model.Workstations = workstations;

                var employees = db.ProductionEmployees.ToList();
                model.Employees = employees;

                this.ViewData["employees"] = new SelectList(db.ProductionEmployees.Where(e => e.Status == "ACTIVE").OrderBy(e => e.EmployeeName), "EmployeeBadgeNumber", "NameAndBadgeNumber").ToList();

                return View(model);
            }
        }

        public ActionResult ChangeWorkstationAssignment(int workstationID, int? employees)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var workstation = db.Workstations.FirstOrDefault(w => w.WorkstationGUID == workstationID);

                if (employees != null)
                {
                    var employee = db.ProductionEmployees.FirstOrDefault(e => e.EmployeeBadgeNumber == employees);
                    workstation.EmployeeBadgeNumberAssigned = employee.EmployeeBadgeNumber;
                }
                else
                {
                    workstation.EmployeeBadgeNumberAssigned = null;
                }

                db.SaveChanges();
                return RedirectToAction(actionName: "Workstations", controllerName: "Production");
            }
        }

        public ActionResult CreateWorkStation(int workstationNumber)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                Workstation newStation = new Workstation()
                {
                    WorkstationNumber = workstationNumber,
                };
                db.Workstations.Add(newStation);
                db.SaveChanges();
                return RedirectToAction(actionName: "Workstations", controllerName: "Production");
            }
        }

        public ActionResult ProductionEmployees()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var employees = db.ProductionEmployees.OrderBy(e => e.Status).ThenBy(e => e.EmployeeName).ToList();
                return View(employees);
            }
        }

        public ActionResult CreateProductionEmployee(string name, int badgeNumber)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                ProductionEmployee newEmployee = new ProductionEmployee()
                {
                    EmployeeName = name,
                    EmployeeBadgeNumber = badgeNumber,
                    NameAndBadgeNumber = (name + " - " + badgeNumber),
                    Status = "ACTIVE",
                };
                db.ProductionEmployees.Add(newEmployee);
                db.SaveChanges();
                return RedirectToAction(actionName: "ProductionEmployees", controllerName: "Production");
            }
        }

        public ActionResult EditProductionEmployee(int employeeID, string name, int badgeNumber, string status)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var employee = db.ProductionEmployees.FirstOrDefault(e => e.ProductionEmployeeGUID == employeeID);

                var workstation = db.Workstations.FirstOrDefault(w => w.EmployeeBadgeNumberAssigned == employee.EmployeeBadgeNumber);

                employee.EmployeeName = name;
                employee.EmployeeBadgeNumber = badgeNumber;
                employee.Status = status;
                employee.NameAndBadgeNumber = (employee.EmployeeName + " - " + employee.EmployeeBadgeNumber);

                if (workstation != null)
                {
                    if (employee.Status == "INACTIVE")
                    {
                        workstation.EmployeeBadgeNumberAssigned = null;
                    }
                    else
                    {
                        workstation.EmployeeBadgeNumberAssigned = badgeNumber;
                    }
                }

                db.SaveChanges();
                return RedirectToAction(actionName: "ProductionEmployees", controllerName: "Production");
            }
        }

        public ActionResult WorkstationInput()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                this.ViewData["workstation1"] = new SelectList(db.Workstations.Where(w => w.EmployeeBadgeNumberAssigned != null).OrderBy(w => w.WorkstationNumber), "WorkstationNumber", "WorkstationNumber").ToList();
                this.ViewData["workstation2"] = new SelectList(db.Workstations.Where(w => w.EmployeeBadgeNumberAssigned != null).OrderBy(w => w.WorkstationNumber), "WorkstationNumber", "WorkstationNumber").ToList();

                return View();
            }
        }
        
        public ActionResult SubmitWorkstationInput(int? workstation1, int? workstation2, int? workstation1Quantity, int? workstation2Quantity, string partNumber, bool endOfDay = false)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var name = Session["name"].ToString();
                DateTime now = DateTime.Now;

                if(workstation1 != null)
                {
                    var workstation = db.Workstations.FirstOrDefault(w => w.WorkstationNumber == workstation1);
                    var employee = db.ProductionEmployees.FirstOrDefault(e => e.EmployeeBadgeNumber == workstation.EmployeeBadgeNumberAssigned);
                    var workorder = db.ProductionWorkOrders.FirstOrDefault(w => w.PartNumber == partNumber);

                    var endOfDayCheck = db.ProductionStacks.Where(e => e.WorkstationNumber == workstation1).ToList();
                    if (endOfDayCheck.Count > 0)
                    {
                        var lastDay = endOfDayCheck.Last();
                        if (lastDay.IsEndOfDay == true)
                        {
                            var subtractQuantity = Convert.ToInt32(lastDay.StackQuantity);
                            workstation1Quantity -= subtractQuantity;
                        }
                    }

                    ProductionStack newStack = new ProductionStack()
                    {
                        WorkstationNumber = workstation.WorkstationNumber,
                        EmployeeBadgeNumber = workstation.EmployeeBadgeNumberAssigned,
                        EmployeeName = employee.EmployeeName,
                        PartNumber = partNumber,
                        StackQuantity = workstation1Quantity,
                        ForkliftDriver = db.Users.FirstOrDefault(u => u.FirstName == name).FirstName,
                        TimeStamp = now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Date = now.ToString("yyyy-MM-dd"),
                        IsEndOfDay = endOfDay,
                        WorkOrderNumber = workorder.WorkOrderNumber,
                    };
                    db.ProductionStacks.Add(newStack);
                    db.SaveChanges();
                }

                if(workstation2 != null)
                {
                    var workstation = db.Workstations.FirstOrDefault(w => w.WorkstationNumber == workstation2);
                    var employee = db.ProductionEmployees.FirstOrDefault(e => e.EmployeeBadgeNumber == workstation.EmployeeBadgeNumberAssigned);
                    var workorder = db.ProductionWorkOrders.FirstOrDefault(w => w.PartNumber == partNumber);

                    var endOfDayCheck = db.ProductionStacks.Where(e => e.WorkstationNumber == workstation2).ToList();
                    if (endOfDayCheck.Count > 0)
                    {
                        var lastDay = endOfDayCheck.Last();
                        if (lastDay.IsEndOfDay == true)
                        {
                            var subtractQuantity = Convert.ToInt32(lastDay.StackQuantity);
                            workstation2Quantity -= subtractQuantity;
                        }
                    }

                    ProductionStack newStack = new ProductionStack()
                    {
                        WorkstationNumber = workstation.WorkstationNumber,
                        EmployeeBadgeNumber = workstation.EmployeeBadgeNumberAssigned,
                        EmployeeName = employee.EmployeeName,
                        PartNumber = partNumber,
                        StackQuantity = workstation2Quantity,
                        ForkliftDriver = db.Users.FirstOrDefault(u => u.FirstName == name).FirstName,
                        TimeStamp = now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Date = now.ToString("yyyy-MM-dd"),
                        IsEndOfDay = endOfDay,
                        WorkOrderNumber = workorder.WorkOrderNumber,
                    };
                    db.ProductionStacks.Add(newStack);
                    db.SaveChanges();
                }
            };
            return RedirectToAction(actionName: "WorkstationInput", controllerName: "Production");
        }

        //public ActionResult SubmitWorkstationInput(int workstationNumber, int quantity, string partNumber, bool endOfDay = false)
        //{
        //    using (TrailerEntities db = new TrailerEntities())
        //    {
        //        int? inputQuantity = quantity;
        //        var endOfDayCheck = db.ProductionStacks.Where(e => e.WorkstationNumber == workstationNumber).ToList();
        //        if (endOfDayCheck.Count > 0)
        //        {
        //            var lastDay = endOfDayCheck.Last();
        //            if (lastDay.IsEndOfDay == true)
        //            {
        //                inputQuantity -= lastDay.StackQuantity;
        //            }
        //        }

        //        ProductionStack newStack = new ProductionStack();

        //        var workstation = db.Workstations.FirstOrDefault(w => w.WorkstationNumber == workstationNumber);
        //        var employee = db.ProductionEmployees.FirstOrDefault(e => e.EmployeeBadgeNumber == workstation.EmployeeBadgeNumberAssigned);
        //        var workorder = db.ProductionWorkOrders.FirstOrDefault(w => w.PartNumber == partNumber);

        //        newStack.WorkstationNumber = workstation.WorkstationNumber;
        //        newStack.EmployeeBadgeNumber = workstation.EmployeeBadgeNumberAssigned;
        //        newStack.EmployeeName = employee.EmployeeName;
        //        newStack.PartNumber = partNumber;
        //        newStack.StackQuantity = inputQuantity;

        //        var name = Session["name"].ToString();
        //        newStack.ForkliftDriver = db.Users.FirstOrDefault(u => u.FirstName == name).FirstName;

        //        DateTime now = DateTime.Now;
        //        newStack.TimeStamp = now.ToString("yyyy-MM-dd HH:mm:ss");
        //        newStack.Date = now.ToString("yyyy-MM-dd");
        //        newStack.IsEndOfDay = endOfDay;
        //        newStack.WorkOrderNumber = workorder.WorkOrderNumber;

        //        db.ProductionStacks.Add(newStack);
        //        db.SaveChanges();

        //        return RedirectToAction(actionName: "WorkstationInput", controllerName: "Production", routeValues: new { workstationNumber });
        //    }
        //}

        public ActionResult WorkOrders()
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var workOrders = db.ProductionWorkOrders.ToList();
                return View(workOrders);
            }
        }

        public ActionResult EdidWorkOrderNumber(int workOrderID, int workOrderNumber)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var workOrder = db.ProductionWorkOrders.FirstOrDefault(w => w.ProductionWorkOrderGUID == workOrderID);
                workOrder.WorkOrderNumber = workOrderNumber;
                db.SaveChanges();
                return RedirectToAction(actionName: "WorkOrders", controllerName: "Production");
            }
        }

        public ActionResult ProductionWorkHours(string date)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                if(db.ProductionStacks.Any(d => d.Date == date))
                {
                    dynamic model = new ExpandoObject();

                    var stacks = db.ProductionStacks.Where(s => s.Date == date).OrderBy(s => s.WorkstationNumber).ToList();

                    var workstations = stacks.Select(w => w.WorkstationNumber).Distinct().ToList();
                    model.Workstations = workstations;

                    var users = stacks.Select(u => u.EmployeeName).Distinct().ToList();
                    model.Users = users;

                    var hours = db.ProductionHours.Where(h => h.Date == date).OrderBy(h => h.WorkstationNumber).ToList();
                    if (hours.Count > 0)
                    {
                        List<Decimal> hoursWorked = new List<decimal>();
                        foreach (ProductionHour hour in hours)
                        {
                            hoursWorked.Add(Convert.ToDecimal(hour.HoursWorked));
                        }
                        model.Hours = hoursWorked;
                    }
                    else
                    {
                        List<Decimal> hoursWorked = new List<decimal>();
                        foreach (int workstation in workstations)
                        {
                            hoursWorked.Add(0);
                        }
                        model.Hours = hoursWorked;
                    }


                    var badgeNumbers = stacks.Select(b => b.EmployeeBadgeNumber).Distinct().ToList();
                    model.BadgeNumber = badgeNumbers;

                    model.Date = date;

                    return View(model);
                }
                else
                {
                    return RedirectToAction(actionName: "ProductionDates", controllerName: "Reporting");
                }
            }
        }

        [HttpPost]
        public ActionResult ProductionWorkHoursInput(FormCollection fc)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                var date = fc["date"];
                var badges = fc["badge"].Split(',');
                var hoursWorked = fc["hours"].Split(',');
                var workstationNumbers = fc["workstation"].Split(',');

                var counter = 0;
                foreach(string badgeNumber in badges)
                {
                    var number = Convert.ToInt32(badgeNumber);
                    var dateCheck = db.ProductionHours.FirstOrDefault(d => d.EmployeeBadgeNumber == number && d.Date == date);
                    if(dateCheck != null)
                    {
                        if(hoursWorked[counter] != "")
                        {
                            dateCheck.HoursWorked = Convert.ToInt32(hoursWorked[counter]);
                        }
                    }
                    else
                    {
                        var user = db.ProductionEmployees.FirstOrDefault(u => u.EmployeeBadgeNumber == number);
                        ProductionHour hour = new ProductionHour()
                        {
                            EmployeeName = user.EmployeeName,
                            EmployeeBadgeNumber = Convert.ToInt32(badgeNumber),
                            Date = date,
                            WorkstationNumber = Convert.ToInt32(workstationNumbers[counter]),
                        };
                        if (hoursWorked[counter] != "")
                        {
                            hour.HoursWorked = Convert.ToDecimal(hoursWorked[counter]);
                        }
                        db.ProductionHours.Add(hour);
                        
                    }
                    counter++;
                }
                db.SaveChanges();
                return RedirectToAction(actionName: "ProductionDateInfo", controllerName: "Reporting", routeValues: new { date });
            }
        }
    }
}