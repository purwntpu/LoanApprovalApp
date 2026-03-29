using LoanApprovalApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanApprovalApp.Controllers
{
    [Authorize]
    public class LoanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public LoanController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ================= Staff =================
        [Authorize(Roles = "Staff")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Create(decimal amount, IFormFile file)
        {
            if (file == null || Path.GetExtension(file.FileName).ToLower() != ".pdf")
            {
                ModelState.AddModelError("", "File harus PDF");
                return View();
            }

            var fileName = Guid.NewGuid() + ".pdf";
            var path = Path.Combine(_env.WebRootPath, "uploads", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var loan = new LoanRequest
            {
                RequestNumber = "LN-" + DateTime.Now.Ticks,
                Amount = amount,
                Status = "Pending",
                CurrentApproverRole = amount < 10000000 ? "Manager" : "Direktur",
                AttachmentPath = "/uploads/" + fileName,
                UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value
            };

            _context.LoanRequests.Add(loan);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyRequest");
        }

        [Authorize(Roles = "Staff")]
        public IActionResult MyRequest()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            var data = _context.LoanRequests
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Id)
                .ToList();
            return View(data);
        }

        // ================= Manager =================
        [Authorize(Roles = "Manager")]
        public IActionResult ApprovalManager()
        {
            var data = _context.LoanRequests
                .Where(x => x.Amount < 10000000) // Hanya dibawah 10 juta
                .OrderByDescending(x => x.Id)
                .ToList();
            return View(data);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ApproveManager(int id)
        {
            var loan = await _context.LoanRequests.FindAsync(id);
            if (loan != null && loan.Amount < 10000000)
            {
                loan.Status = "ApprovedManager";
                loan.CurrentApproverRole = "Manager";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ApprovalManager");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> RejectManager(int id)
        {
            var loan = await _context.LoanRequests.FindAsync(id);
            if (loan != null && loan.Amount < 10000000)
            {
                loan.Status = "Rejected";
                loan.CurrentApproverRole = "Manager";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ApprovalManager");
        }

        // ================= Direktur =================
        [Authorize(Roles = "Direktur")]
        public IActionResult ApprovalDirektur()
        {
            var data = _context.LoanRequests
                .Where(x => x.Amount >= 10000000) // Hanya 10 juta ke atas
                .OrderByDescending(x => x.Id)
                .ToList();
            return View(data);
        }

        [Authorize(Roles = "Direktur")]
        public async Task<IActionResult> ApproveDirektur(int id)
        {
            var loan = await _context.LoanRequests.FindAsync(id);
            if (loan != null && loan.Amount >= 10000000)
            {
                loan.Status = "ApprovedDirector";
                loan.CurrentApproverRole = "Direktur";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ApprovalDirektur");
        }

        [Authorize(Roles = "Direktur")]
        public async Task<IActionResult> RejectDirektur(int id)
        {
            var loan = await _context.LoanRequests.FindAsync(id);
            if (loan != null && loan.Amount >= 10000000)
            {
                loan.Status = "Rejected";
                loan.CurrentApproverRole = "Direktur";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ApprovalDirektur");
        }
    }
}
