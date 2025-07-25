using AppointmentSender.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSender.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; } // Example entity
    }
}
