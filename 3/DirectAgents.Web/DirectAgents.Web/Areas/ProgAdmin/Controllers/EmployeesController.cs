using System.Linq;
using System.Web.Mvc;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Entities;

namespace DirectAgents.Web.Areas.ProgAdmin.Controllers
{
    public class EmployeesController : DirectAgents.Web.Controllers.ControllerBase
    {
        public EmployeesController(ICPProgRepository cpProgRepository)
        {
            this.cpProgRepo = cpProgRepository;
        }

        public ActionResult Index()
        {
            var employees = cpProgRepo.Employees()
                .OrderBy(e => e.FirstName).ThenBy(e => e.LastName);
            return View(employees);
        }

        public ActionResult CreateNew()
        {
            var employee = new Employee
            {
                FirstName = "zNew",
                LastName = "Employee"
            };
            if (cpProgRepo.AddEmployee(employee))
                return RedirectToAction("Index");
            else
                return Content("Error creating Employee");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var employee = cpProgRepo.Employee(id);
            if (employee == null)
                return HttpNotFound();
            //setupforedit
            return View(employee);
        }
        [HttpPost]
        public ActionResult Edit(Employee emp)
        {
            if (ModelState.IsValid)
            {
                if (cpProgRepo.SaveEmployee(emp))
                    return RedirectToAction("Index");
                ModelState.AddModelError("", "Employee could not be saved.");
            }
            return View(emp);
        }
	}
}