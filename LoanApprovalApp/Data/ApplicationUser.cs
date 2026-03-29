using Microsoft.AspNetCore.Identity;

namespace LoanApprovalApp.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
