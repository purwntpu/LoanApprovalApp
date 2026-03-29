using System.ComponentModel.DataAnnotations;

namespace LoanApprovalApp.Data
{
    public class LoanRequest
    {
        public int Id { get; set; }

        [Required]
        public string RequestNumber { get; set; }

        [Required]
        public string UserId { get; set; }

        public decimal Amount { get; set; }

        // Status: Pending, ApprovedManager, NeedDirector, ApprovedDirector, Rejected
        public string Status { get; set; } = "Pending";

        // Bisa ditambahkan siapa yang approve terakhir
        public string CurrentApproverRole { get; set; } = "Manager";

        public string? AttachmentPath { get; set; }
    }
}
