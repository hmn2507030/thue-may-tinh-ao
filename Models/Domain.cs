using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace CloudRental.Models;
public class ApplicationUser : IdentityUser {
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public enum PlanType { VPS, VirtualDesktop }
public enum OrderStatus { Pending, Approved, Rejected, Cancelled }
public enum VmStatus { Creating, Running, Stopped }
public class Plan {
    public int Id { get; set; }
    [Required, StringLength(80)] public string Name { get; set; } = "";
    [EnumDataType(typeof(PlanType))] public PlanType Type { get; set; }
    [Range(1,128)] public int CPU { get; set; } = 2;
    [Range(1,1024)] public int RAM { get; set; } = 4;
    [Range(10,100000)] public int Storage { get; set; } = 60;
    [Range(1000,100000000)] public decimal PricePerMonth { get; set; }
    [Required, StringLength(2000)] public string Description { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
public class Order {
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;
    public string PlanName { get; set; } = "";
    public int CPU { get; set; }
    public int RAM { get; set; }
    public int Storage { get; set; }
    public PlanType Type { get; set; }
    public int Months { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public VirtualMachine? VirtualMachine { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
public class VirtualMachine {
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;
    public string VmName { get; set; } = "";
    public VmStatus Status { get; set; } = VmStatus.Creating;
    public string IpAddress { get; set; } = "";
    public string Username { get; set; } = "student";
    public string ProtectedPassword { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
