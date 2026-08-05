using Microsoft.AspNetCore.Identity;

namespace CV_mng_sys.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string? SalesforceAccountId {get; set;}
    public string? SalesforceContactId {get; set;}
}