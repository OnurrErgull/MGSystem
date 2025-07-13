using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AuthService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
}
