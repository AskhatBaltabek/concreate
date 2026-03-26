using Microsoft.AspNetCore.Identity;

namespace ContentGenerator.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    // Additional properties can be added here
}
