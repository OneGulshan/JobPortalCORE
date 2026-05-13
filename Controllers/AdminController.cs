using JobPortalCORE.Data;
using JobPortalCORE.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace JobPortalCORE.Controllers
{
    public class AdminController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;

        public IActionResult ManageJobPost() => View(_context.JobPosts.ToList());

        public IActionResult ManageJobSeeker()
        {
            List<ManageJobSeekerViewModel> vm = [];

            foreach (var i in (from e in _context.Employees
                               join jp in _context.JobProfile
                               on e.JobProfileId equals jp.JPId
                               join s in _context.Skills
                               on e.SkillsId equals s.SklId
                               join c in _context.Countries
                               on e.CountryId equals c.CId
                               join st in _context.States
                               on e.StateId equals st.SId
                               join ct in _context.Cities
                               on e.CityId equals ct.CityId
                               select new
                               {
                                   e.EId,
                                   EName = e.Name,
                                   e.Gender,
                                   e.Email,
                                   e.Password,
                                   e.Mobno,
                                   e.Age,
                                   e.ImagePath,
                                   e.Comment,
                                   e.Status,
                                   JPName = jp.Name,
                                   SName = s.Name,
                                   CountryName = c.Name,
                                   StateName = st.Name,
                                   CityName = ct.Name
                               }).AsEnumerable().ToList())
            {
                vm.Add(new ManageJobSeekerViewModel
                {
                    Id = i.EId,
                    EName = i.EName,
                    Gender = i.Gender,
                    Email = i.Email,
                    Password = i.Password,
                    Mobno = (int)i.Mobno,
                    Age = (int)i.Age,
                    ImagePath = i.ImagePath,
                    Comment = i.Comment,
                    JPName = i.JPName,
                    SName = i.SName,
                    CountryName = i.CountryName,
                    StateName = i.StateName,
                    CityName = i.CityName,
                    Status = i.Status
                });
            }

            return View(vm);
        }

        public IActionResult ManageJobRecruiter() => View(_context.Companies.ToList());

        public IActionResult ApproveJobPost(int id)
        {
            var result = _context.JobPosts.Where(_ => _.JobPostId == id).FirstOrDefault();
            result.Status = result.Status != true;
            _context.JobPosts.Update(result);
            _context.SaveChanges();
            return RedirectToAction("ManageJobPost");
        }

        public IActionResult ApproveJobSeeker(int id)
        {
            var result = _context.Employees.Where(_ => _.EId == id).FirstOrDefault();
            result.Status = result.Status != true;
            _context.Employees.Update(result);
            _context.SaveChanges();
            return RedirectToAction("ManageJobSeeker");
        }

        public IActionResult ApproveJobRecruiter(int id)
        {
            var result = _context.Companies.Where(_ => _.Id == id).FirstOrDefault();
            result.Status = result.Status != true;
            _context.Companies.Update(result);
            _context.SaveChanges();
            return RedirectToAction("ManageJobRecruiter");
        }

        public IActionResult DeleteJobPost(int id)
        {
            _context.JobPosts.Remove(_context.JobPosts.Where(_ => _.JobPostId == id).FirstOrDefault());
            _context.SaveChanges();
            return RedirectToAction("ManageJobPost");
        }

        public IActionResult DeleteJobSeeker(int id)
        {
            _context.Employees.Remove(_context.Employees.Where(_ => _.EId == id).FirstOrDefault());
            _context.SaveChanges();
            return RedirectToAction("ManageJobSeeker");
        }

        public IActionResult DeleteJobRecruiter(int id)
        {
            _context.Companies.Remove(_context.Companies.Where(_ => _.Id == id).FirstOrDefault());
            _context.SaveChanges();
            return RedirectToAction("ManageJobRecruiter");
        }

        // 💡 NAYA METHOD: Audit Logs dekhne ke liye
        public IActionResult AuditHistory()
        {
            // Sabse latest logs sabse upar dikhane ke liye OrderByDescending lagaya hai.
            // Take(100) isliye lagaya taaki page fast load ho.
            var logs = _context.AuditLogs
                               .OrderByDescending(a => a.Timestamp)
                               .Take(100)
                               .ToList();

            return View(logs);
        }
    }
}
