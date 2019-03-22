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
                this.ViewData["workstations"] = new SelectList(db.Workstations.Where(w => w.EmployeeBadgeNumberAssigned != null).OrderBy(w => w.WorkstationNumber), "WorkstationNumber", "WorkstationNumber").ToList();

                return View();
            }
        }

        public ActionResult SubmitWorkstationInput(int workstationNumber, int quantity, string partNumber, bool endOfDay = false)
        {
            using (TrailerEntities db = new TrailerEntities())
            {
                int? inputQuantity = quantity;
                var endOfDayCheck = db.ProductionStacks.Where(e => e.WorkstationNumber == workstationNumber).ToList();
                if (endOfDayCheck.Count > 0)
                {
                    var lastDay = endOfDayCheck.Last();
                    if (lastDay.IsEndOfDay == true)
                    {
                        inputQuantity -= lastDay.StackQuantity;
                    }
                }

                ProductionStack newStack = new ProductionStack();

                var workstation = db.Workstations.FirstOrDefault(w => w.WorkstationNumber == workstationNumber);
                var employee = db.ProductionEmployees.FirstOrDefault(e => e.EmployeeBadgeNumber == workstation.EmployeeBadgeNumberAssigned);
                var workorder = db.ProductionWorkOrders.FirstOrDefault(w => w.PartNumber == partNumber);

                newStack.WorkstationNumber = workstation.WorkstationNumber;
                newStack.EmployeeBadgeNumber = workstation.EmployeeBadgeNumberAssigned;
                newStack.EmployeeName = employee.EmployeeName;
                newStack.PartNumber = partNumber;
                newStack.StackQuantity = inputQuantity;

                DateTime now = DateTime.Now;
                newStack.TimeStamp = now.ToString("yyyy-MM-dd HH:mm:ss");
                newStack.Date = now.ToString("yyyy-MM-dd");
                newStack.IsEndOfDay = endOfDay;
                newStack.WorkOrderNumber = workorder.WorkOrderNumber;

                db.ProductionStacks.Add(newStack);
                db.SaveChanges();

                return RedirectToAction(actionName: "WorkstationInput", controllerName: "Production");
            }
        }

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
    }
}