using JobPortalCORE.Data;
using JobPortalCORE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JobPortalCORE.Controllers
{
    [Authorize]
    public class JobPostController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;

        public IActionResult Index() => View(_context.JobPosts.ToList());

        [HttpGet]
        public IActionResult Create(int id)
        {
            ViewBag.JopProfile = new SelectList(_context.JobProfile.ToList(), "JPId", "Name");

            if (id > 0)
            {
                var JobPost = _context.JobPosts.Where(_ => _.JobPostId == id).FirstOrDefault();
                ViewBag.Bt = "Update";
                return View(JobPost);
            }
            else
            {
                ViewBag.BT = "Create";
                return View();
            }
        }

        public JsonResult GetJobPost(int JPId) => Json(ViewBag.JopProfile = new SelectList(_context.Categories.Where(_ => _.CatId == JPId).ToList(), "JPId", "Name"));

        [HttpPost]
        public IActionResult Create(JobPost jobPost, int id)
        {
            if (id > 0)
            {
                var JobPost = _context.JobPosts.Find(id);
                JobPost.JobPostId = jobPost.JobPostId;
                JobPost.JobProfileId = jobPost.JobProfileId;
                JobPost.MinExp = jobPost.MinExp;
                JobPost.MaxExp = jobPost.MaxExp;
                JobPost.MinSal = jobPost.MinSal;
                JobPost.MaxSal = jobPost.MaxSal;
                JobPost.NoticePeriod = jobPost.NoticePeriod;
                JobPost.Comment = jobPost.Comment;
                JobPost.InsertedDate = jobPost.InsertedDate;

                _context.JobPosts.Update(JobPost);
                _context.SaveChanges();
            }
            else
            {
                _context.JobPosts.Add(jobPost);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
