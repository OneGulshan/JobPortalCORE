using JobPortalCORE.Data;
using JobPortalCORE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JobPortalCORE.Controllers
{
    public class JobRecruiterController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;

        public IActionResult Index()
        {
            return View(_context.Companies.ToList());
        }

        [HttpGet]
        public IActionResult Create(int id)
        {
            ViewBag.JobProfile = new SelectList(_context.JobProfile.ToList(), "JPId", "Name");

            if (id > 0)
            {
                ViewBag.Bt = "Update";
                return View(_context.Companies.Where(_ => _.Id == id).FirstOrDefault());
            }
            else
            {
                ViewBag.BT = "Create";
                return View();
            }
        }

        public JsonResult GetCategory(int JPId) => Json(ViewBag.JobProfile = new SelectList(_context.JobProfile.Where(_ => _.JPId == JPId).ToList(), "JPId", "Name"));

        [HttpPost]
        public IActionResult Create(Company company, int id)
        {
            if (id > 0)
            {
                var Company = _context.Companies.Find(id);
                Company.Id = company.Id;
                Company.Name = company.Name;
                Company.Email = company.Email;
                Company.Password = company.Password;
                Company.Comment = company.Comment;
                Company.JobProfileId = company.JobProfileId;

                _context.Companies.Update(Company);
                _context.SaveChanges();
            }
            else
            {
                _context.Companies.Add(company);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
