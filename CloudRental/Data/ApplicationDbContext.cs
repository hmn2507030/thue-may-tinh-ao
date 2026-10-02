using CloudRental.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace CloudRental.Data;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options) {
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<VirtualMachine> VirtualMachines => Set<VirtualMachine>();
    protected override void OnModelCreating(ModelBuilder b) {
        base.OnModelCreating(b);
        b.Entity<Plan>().Property(x=>x.PricePerMonth).HasPrecision(18,2);
        b.Entity<Order>().Property(x=>x.UnitPrice).HasPrecision(18,2);
        b.Entity<Order>().Property(x=>x.Total).HasPrecision(18,2);
        b.Entity<Order>().Property(x=>x.RowVersion).IsRowVersion();
        b.Entity<VirtualMachine>().Property(x=>x.RowVersion).IsRowVersion();
        b.Entity<Order>().HasOne(x=>x.User).WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Order>().HasOne(x=>x.Plan).WithMany().HasForeignKey(x=>x.PlanId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Order>().HasIndex(x=>new {x.UserId,x.CreatedAt});
        b.Entity<VirtualMachine>().HasOne(x=>x.Order).WithOne(x=>x.VirtualMachine).HasForeignKey<VirtualMachine>(x=>x.OrderId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<VirtualMachine>().HasOne(x=>x.User).WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<VirtualMachine>().HasOne(x=>x.Plan).WithMany().HasForeignKey(x=>x.PlanId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<VirtualMachine>().HasIndex(x=>x.UserId);
    }
}
