using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Capstone;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
}

public class User
{
    public Guid Id { get; set;}

    [Required]
    public string UserName { get; set; }

    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    public string Role { get; set; }
}