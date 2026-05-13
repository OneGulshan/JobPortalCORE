using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using JobPortalCORE.Data;
using JobPortalCORE.Models;
using JobPortalCORE.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Net.Mail;
using Azure.Storage.Blobs.Specialized;

namespace JobPortalCORE.Controllers
{
    [Authorize]
    public class JobSeekerController(AppDbContext context, IWebHostEnvironment environment, IConfiguration configuration) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly IWebHostEnvironment _environment = environment;
        private readonly IConfiguration _configuration = configuration;

        public async Task<IActionResult> Index(string searchCity = "")
        {
            // 1. Pehle database se saare employees uthao
            var employees = _context.Employees.ToList();

            // 2. Agar user ne City search ki hai, toh Azure ka jadu chalao
            if (!string.IsNullOrEmpty(searchCity))
            {
                string connectionString = _configuration.GetConnectionString("BlobConnectionString");
                BlobContainerClient containerClient = new BlobContainerClient(connectionString, "resumes");

                // Azure Tag Query
                string query = $"\"City\" = '{searchCity}'";
                List<string> matchingBlobUrls = new List<string>();

                // Azure se matching files dhoondhna
                await foreach (TaggedBlobItem taggedBlobItem in containerClient.FindBlobsByTagsAsync(query))
                {
                    // Hum matching blobs ke poore URL nikal rahe hain match karne ke liye
                    string fullUrl = $"{containerClient.Uri}/{taggedBlobItem.BlobName}";
                    matchingBlobUrls.Add(fullUrl);
                }

                // 3. SQL list ko filter karo: Sirf wahi employees rakho jinka ResumePath Azure results mein mila
                employees = employees.Where(e => matchingBlobUrls.Contains(e.ResumePath)).ToList();

                ViewBag.CurrentSearch = searchCity; // Search box mein naam dikhane ke liye
            }

            return View(employees);
        }

        public IActionResult Create(int id)
        {
            ViewBag.JobProfile = new SelectList(_context.JobProfile.ToList(), "JPId", "Name");
            ViewBag.Country = new SelectList(_context.Countries.ToList(), "CId", "Name");

            if (id > 0)
            {
                var Employee = _context.Employees.Where(_ => _.EId == id).FirstOrDefault();
                GetSkills(Employee.JobProfileId);
                GetStates(Employee.CountryId);
                GetCity(Employee.StateId);
                ViewBag.Bt = "Update";
                return View(Employee);
            }
            else
            {
                ViewBag.BT = "Create";
                return View(new Employee());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Employee employee, int id)
        {
            if (id == 0) // --- CREATE NEW EMPLOYEE ---
            {
                // 1. Profile Image Upload
                if (employee.ImageFile != null)
                {
                    // Naye employee ki koi purani photo nahi hoti, isliye seedha upload
                    string rawUrl = await UploadToBlobAsync(employee.ImageFile, "izmages", employee);

                    // 🪄 JADU: DB mein save hone se pehle link badal do!
                    // Taaki website directly naye container se photo uthaye
                    employee.ImagePath = rawUrl.Replace("/izmages/", "/compressed-images/");
                }

                // 2. Resume Upload 
                if (employee.ResumeFile != null)
                {
                    employee.ResumePath = await UploadToBlobAsync(employee.ResumeFile, "resumes", employee);
                }

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
            }
            else // --- UPDATE EXISTING EMPLOYEE ---
            {
                var existingEmployee = await _context.Employees.FindAsync(id);
                if (existingEmployee == null)
                {
                    TempData["Message"] = "Employee not found!";
                    return RedirectToAction("Create");
                }

                // Update basic properties
                existingEmployee.Name = employee.Name;
                existingEmployee.Gender = employee.Gender;
                existingEmployee.JobProfileId = employee.JobProfileId;
                existingEmployee.SkillsId = employee.SkillsId;
                existingEmployee.Email = employee.Email;
                existingEmployee.Age = employee.Age;
                existingEmployee.Mobno = employee.Mobno;
                existingEmployee.CountryId = employee.CountryId;
                existingEmployee.StateId = employee.StateId;
                existingEmployee.CityId = employee.CityId;
                existingEmployee.Comment = employee.Comment;

                if (!string.IsNullOrEmpty(employee.Password))
                {
                    existingEmployee.Password = employee.Password;
                }

                // 👉 Handle Image Update & Deletion
                if (employee.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(existingEmployee.ImagePath))
                    {
                        // Purani file compressed wale container se udhani hai
                        await DeleteBlobAsync(existingEmployee.ImagePath, "compressed-images");
                    }

                    // 1. Pehle 'izmages' mein upload karo
                    string rawUrl = await UploadToBlobAsync(employee.ImageFile, "izmages", employee);

                    // 2. 🪄 JADU: DB mein save hone se pehle link badal do!
                    existingEmployee.ImagePath = rawUrl.Replace("/izmages/", "/compressed-images/");
                }

                // 👉 Handle Resume Update & Deletion
                if (employee.ResumeFile != null)
                {
                    if (!string.IsNullOrEmpty(existingEmployee.ResumePath))
                    {
                        await DeleteBlobAsync(existingEmployee.ResumePath, "resumes");
                    }
                    existingEmployee.ResumePath = await UploadToBlobAsync(employee.ResumeFile, "resumes", employee);
                }

                _context.Employees.Update(existingEmployee);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        #region Azure Blob Storage Learning Start

        // 👈 NAYA LOGIC: Parameter mein 'Employee employee' add kiya hai taaki dynamic data mil sake
        private async Task<string> UploadToBlobAsync(IFormFile file, string containerName, Employee employee)
        {
            string connectionString = _configuration.GetConnectionString("BlobConnectionString");
            BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
            await containerClient.CreateIfNotExistsAsync();

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            string ext = Path.GetExtension(file.FileName).ToLower();
            string myContentType = file.ContentType;

            if (ext == ".pdf") myContentType = "application/pdf";
            else if (ext == ".jpg" || ext == ".jpeg") myContentType = "image/jpeg";
            else if (ext == ".png") myContentType = "image/png";

            var blobHttpHeader = new BlobHttpHeaders { ContentType = myContentType };

            string jobProfileName = _context.JobProfile.FirstOrDefault(j => j.JPId == employee.JobProfileId)?.Name ?? "NA";
            string skillName = _context.Skills.FirstOrDefault(s => s.SklId == employee.SkillsId)?.Name ?? "NA";
            string cityName = _context.Cities.FirstOrDefault(c => c.CityId == employee.CityId)?.Name ?? "NA";

            var blobTags = new Dictionary<string, string>
            {
                { "Category", containerName },
                { "JobProfile", jobProfileName },
                { "Skill", skillName },
                { "City", cityName }
            };

            var metadata = new Dictionary<string, string>
            {
                { "OriginalFileName", file.FileName },
                { "UploadedBy", string.IsNullOrEmpty(employee.Name) ? "Candidate" : employee.Name },
                { "Project", "JobPortal" }
            };

            var options = new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeader,
                Tags = blobTags,
                Metadata = metadata
            };

            // 🚀 FAST UPLOAD (Bina ruke seedha cloud par phek do)
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, options);
            }

            return blobClient.Uri.ToString();
        }

        private async Task DeleteBlobAsync(string fileUrl, string containerName)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("BlobConnectionString");
                BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
                Uri oldBlobUri = new Uri(fileUrl);
                string oldBlobName = Path.GetFileName(oldBlobUri.LocalPath);
                BlobClient oldBlobClient = containerClient.GetBlobClient(oldBlobName);
                await oldBlobClient.DeleteIfExistsAsync();
            }
            catch (Exception)
            {
                // Agar file na mile ya delete fail ho jaye, toh flow break nahi hoga
            }
        }

        // 👉 THE SMART UPLOAD LOGIC (FIXED FOR BROWSER VIEW)
        private async Task<string> FileUploadAsync(Employee employee)
        {
            string fileUrl = null;

            if (employee.ImageFile != null)
            {
                try
                {
                    string connectionString = _configuration.GetConnectionString("BlobConnectionString");
                    string containerName = "izmages"; // Container ka naam

                    BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);

                    string uniqueFileName = Guid.NewGuid().ToString() + "-" + employee.ImageFile.FileName;
                    BlobClient blobClient = containerClient.GetBlobClient(uniqueFileName);

                    using (var fileStream = employee.ImageFile.OpenReadStream())
                    {
                        // File ka extension check karke sahi Content-Type lagana
                        string ext = Path.GetExtension(employee.ImageFile.FileName).ToLower();
                        string myContentType = "application/octet-stream";

                        if (ext == ".png") myContentType = "image/png";
                        else if (ext == ".jpg" || ext == ".jpeg") myContentType = "image/jpeg";
                        else if (ext == ".pdf") myContentType = "application/pdf";

                        var blobHttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = myContentType
                        };

                        await blobClient.UploadAsync(fileStream, new BlobUploadOptions
                        {
                            HttpHeaders = blobHttpHeaders
                        });
                    }

                    fileUrl = blobClient.Uri.ToString();
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Cloud upload failed: " + ex.Message;
                }
            }

            return fileUrl;
        }

        // 💡 Helper Method (Isey Controller mein ya Service class mein daal de)
        private string GenerateSecureSasLink(string originalBlobUrl, string containerName)
        {
            // 1. Connection string uthao
            string connectionString = _configuration.GetConnectionString("BlobConnectionString");
            BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);

            // 2. Original URL se file ka naam nikalo
            Uri uri = new Uri(originalBlobUrl);
            string blobName = Path.GetFileName(uri.LocalPath);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // 3. SAS Token generate karo (Agar connection string ke paas rights hain)
            if (blobClient.CanGenerateSasUri)
            {
                // 🎫 Yahan hum Ticket bana rahe hain
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = containerName,
                    BlobName = blobName,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10),
                    // 💡 Ye line browser ko bolti hai "Download mat karo, dikhao!"
                    ContentDisposition = "inline"
                };

                // Sirf 'Read' (dekhne) ki permission di hai, delete ya edit ki nahi
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                // Naya secure URL generate karo
                Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
                return sasUri.ToString();
            }

            return null; // Agar fail ho jaye
        }

        public async Task<IActionResult> ViewSecureResume(int id)
        {
            // 1. Employee ko database se dhundo
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null || string.IsNullOrEmpty(employee.ResumePath))
            {
                return NotFound("Resume nahi mila!");
            }

            // 2. Live SAS Token generate karo
            string secureUrl = GenerateSecureSasLink(employee.ResumePath, "resumes");

            // 3. User ko is naye 10-minute wale URL par bhejo (Naye tab mein pdf khul jayega)
            return Redirect(secureUrl);
        }

        public async Task<IActionResult> TestCloudSearch()
        {
            string connectionString = _configuration.GetConnectionString("BlobConnectionString");
            // Humein resumes wale container mein dhoondhna hai
            BlobContainerClient containerClient = new BlobContainerClient(connectionString, "resumes");

            // 💡 Query: Double quotes key ke liye aur single quotes value ke liye
            string query = "\"City\" = 'New Delhi'";

            List<string> foundFiles = new List<string>();

            try
            {
                // Azure se matching blobs mangwana
                await foreach (TaggedBlobItem taggedBlobItem in containerClient.FindBlobsByTagsAsync(query))
                {
                    foundFiles.Add(taggedBlobItem.BlobName);
                }
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }

            if (foundFiles.Count > 0)
            {
                return Content("Jadu Ho Gaya! Ye rahi matching files: " + string.Join(", ", foundFiles));
            }
            else
            {
                return Content("Abhi bhi kuch nahi mila. Shayad Azure bot abhi so raha hai (Indexing Delay).");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ShortlistCandidate(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee != null && !string.IsNullOrEmpty(employee.ResumePath))
            {
                // Safely file ko lock karke usme "Status = Shortlisted" chipka do
                bool success = await UpdateMetadataSafelyAsync(employee.ResumePath, "resumes", "Shortlisted");

                if (success)
                {
                    TempData["Message"] = $"{employee.Name} ka resume safely shortlist ho gaya (Lock laga kar)!";
                }
                else
                {
                    TempData["Error"] = $"Abhi {employee.Name} ke resume par kisi aur ka lock laga hai, thodi der baad try karein.";
                }
            }
            return RedirectToAction("Index");
        }

        // 🔒 THE LEASE LOGIC (Naya Helper Method for Safe Updates)
        private async Task<bool> UpdateMetadataSafelyAsync(string fileUrl, string containerName, string newStatus)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("BlobConnectionString");
                BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);

                Uri blobUri = new Uri(fileUrl);
                string blobName = Path.GetFileName(blobUri.LocalPath);
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                // 1. File ka ek "Lease Client" (Taale ki chaabi) banao
                BlobLeaseClient leaseClient = blobClient.GetBlobLeaseClient();

                // 🔒 2. ACQUIRE LEASE (15 seconds ke liye taala lagao)
                // Agar file pehle se locked hogi, toh yahan line fail ho jayegi aur Catch me jayegi
                BlobLease lease = await leaseClient.AcquireAsync(TimeSpan.FromSeconds(15));
                string myLeaseId = lease.LeaseId; // Ye humari unique chaabi hai

                // Purana metadata nikalo taaki wo delete na ho jaye
                BlobProperties properties = await blobClient.GetPropertiesAsync();
                var currentMetadata = properties.Metadata ?? new Dictionary<string, string>();

                // Naya data add ya update karo
                currentMetadata["Status"] = newStatus;
                currentMetadata["LastUpdatedByHR"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");

                // 🛠️ 3. UPDATE METADATA (Chaabi dikha kar)
                BlobRequestConditions conditions = new BlobRequestConditions { LeaseId = myLeaseId };
                await blobClient.SetMetadataAsync(currentMetadata, conditions);

                // 🛑 THE JADU WALI LINE (10 second ke liye code yahin rok do)
                await Task.Delay(10000);

                // 🔓 4. RELEASE LEASE (Kaam khatam, taala khol do)
                await leaseClient.ReleaseAsync();

                return true; // Kaam successfully ho gaya
            }
            catch (Azure.RequestFailedException ex)
            {
                // 409 status: "LeaseAlreadyPresent" (File par pehle se lock laga hai)
                // 412 status: "Precondition Failed" (Aapke paas lock ki sahi chaabi nahi hai)
                if (ex.Status == 409 || ex.Status == 412)
                {
                    // Ab code crash nahi hoga, balki safely false return karega
                    // jisse UI par tera wo warning wala TempData message dikhega!
                    return false;
                }

                // Agar koi aur ganda error ho (jaise net chala gaya), toh throw karo
                throw;
            }
        }
        #endregion Azure Blob Storage Learning End

        public JsonResult GetSkills(int JPId) => Json(ViewBag.Skills = new SelectList(_context.Skills.Where(_ => _.JPId == JPId).ToList(), "SklId", "Name"));

        public JsonResult GetStates(int CId) => Json(ViewBag.State = new SelectList(_context.States.Where(_ => _.CId == CId).ToList(), "SId", "Name"));

        public JsonResult GetCity(int StateId) => Json(ViewBag.City = new SelectList(_context.Cities.Where(_ => _.SId == StateId).ToList(), "CityId", "Name"));

        // Isko async Task banana zaroori hai kyunki hum cloud par request bhej rahe hain
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return RedirectToAction("Index");
            }

            // 👉 1. Delete image from Azure Blob Storage First
            if (!string.IsNullOrEmpty(employee.ImagePath))
            {
                try
                {
                    // Connection string aur Container ka naam
                    string connectionString = _configuration.GetConnectionString("BlobConnectionString");
                    BlobContainerClient containerClient = new BlobContainerClient(connectionString, "izmages");

                    // URL se sirf file ka naam (blob name) extract karna
                    Uri oldBlobUri = new Uri(employee.ImagePath);
                    string oldBlobName = Path.GetFileName(oldBlobUri.LocalPath);

                    // Cloud se permanently delete karna
                    BlobClient oldBlobClient = containerClient.GetBlobClient(oldBlobName);
                    await oldBlobClient.DeleteIfExistsAsync();
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Employee deleted, but error removing cloud image: " + ex.Message;
                }
            }

            // 👉 2. Delete record from Database
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public IActionResult MailSent() => View();

        [HttpPost]
        public IActionResult MailSent(Message messageDetails)
        {
            MailMessage message = new();
            SmtpClient smtpClientsmtp = new();
            message.From = new MailAddress("gulshankumar.mailid01@gmail.com");
            message.To.Add(messageDetails.Email);
            message.Subject = "Test Mail";

            string MailBody = "<!DOCTYPE html>" +
  "<body style=\"display:flex; justify-content:center;\">" +
  "<div style=\"min-height: 10rem; width: 40rem; background-color: aqua; padding: 1rem; border-radius: 10px;\">" +
       " <table style=\"width:100%; color:white; border: 1px solid black; text-align: center;\">" +
           " <thead style=\"color:black;\">" +
                "<tr>" +
                   " <th style=\"border: 1px solid black;\"> Name </th>" +
                   " <th style=\"border: 1px solid black;\"> Email </th>" +
                   " <th style=\"border: 1px solid black;\"> Message </th>" +
               " </tr>" +
            "</thead>" +
            "<tbody>" +
                "<tr>" +
                   $" <td style=\"border: 1px solid black;\"> {messageDetails.Name} </td>" +
                    $"<td style=\"border: 1px solid black;\"> {messageDetails.Email} </td>" +
                   $" <td style=\"border: 1px solid black;\"> {messageDetails.MessageData} </td>" +
                "</tr>" +
           " </tbody>" +
        "</table>" +
"</body>" +

"</html>";
            message.Body = MailBody;
            message.IsBodyHtml = true;
            smtpClientsmtp.Port = 587;
            smtpClientsmtp.Host = "smtp.gmail.com";
            smtpClientsmtp.EnableSsl = true;
            smtpClientsmtp.UseDefaultCredentials = false;
            smtpClientsmtp.Credentials = new NetworkCredential("gulshankumar.mailid01@gmail.com", "aeljomqrgsaqgtkv");
            smtpClientsmtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClientsmtp.Send(message);
            ModelState.Clear();
            return View();
        }

        public IActionResult ViewJobs()
        {
            List<JobsViewJobSeekerViewModel> vm = [];

            foreach (var i in (from jp in _context.JobPosts
                               join c in _context.Companies
                               on jp.JobProfileId equals c.JobProfileId
                               join jpr in _context.JobProfile
                               on jp.JobProfileId equals jpr.JPId
                               select new
                               {
                                   jp.MinExp,
                                   jp.MaxExp,
                                   jp.MinSal,
                                   jp.MaxSal,
                                   jp.NoOfVac,
                                   jp.NoticePeriod,
                                   jp.Comment,
                                   jp.InsertedDate,
                                   c.Id,
                                   CName = c.Name,
                                   JPName = jpr.Name
                               }).ToList())
            {
                vm.Add(new JobsViewJobSeekerViewModel
                {
                    MinExp = (int)i.MinExp,
                    MaxExp = (int)i.MaxExp,
                    MinSal = (int)i.MinSal,
                    MaxSal = (int)i.MaxSal,
                    NoOfVac = (int)i.NoOfVac,
                    NoticePeriod = (int)i.NoticePeriod,
                    Comment = i.Comment,
                    InsertedDate = i.InsertedDate,
                    CompanyId = i.Id,
                    CompanyName = i.CName,
                    JPName = i.JPName
                });
            }

            return View(vm);
        }
    }
}
