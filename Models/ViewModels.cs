using System.ComponentModel.DataAnnotations;
namespace CloudRental.Models;
public class RegisterVm {
    [Required(ErrorMessage="Nhập họ tên"), StringLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = "";
    [Required, StringLength(100, MinimumLength=10), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required, Compare(nameof(Password), ErrorMessage="Mật khẩu xác nhận không khớp"), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}
public class LoginVm {
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
public class CatalogVm {
    public string? Q { get; set; }
    public PlanType? Type { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinCpu { get; set; }
    public int? MinRam { get; set; }
    public int? MinStorage { get; set; }
    public List<Plan> Plans { get; set; } = new();
}
public class CheckoutVm {
    public int PlanId { get; set; }
    public int Months { get; set; } = 1;
    public Plan? Plan { get; set; }
}
public class DashboardVm {
    public List<VirtualMachine> Machines { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
}
public class MachineDetailsVm {
    public VirtualMachine Machine { get; set; } = null!;
    public string Password { get; set; } = "";
}
public class AdminOverviewVm {
    public int Users { get; set; }
    public int Plans { get; set; }
    public int Pending { get; set; }
    public int Machines { get; set; }
    public decimal ApprovedTotal { get; set; }
    public List<Order> Recent { get; set; } = new();
}
public static class Display {
    public static string Money(decimal amount) => amount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + " ₫";
    public static string Date(DateTime date) => date.AddHours(7).ToString("dd/MM/yyyy HH:mm");
    public static string Status(OrderStatus status) => status switch { OrderStatus.Pending => "Chờ duyệt", OrderStatus.Approved => "Đã duyệt", OrderStatus.Rejected => "Từ chối", _ => "Đã hủy" };
    public static string Status(VmStatus status) => status switch { VmStatus.Creating => "Đang khởi tạo", VmStatus.Running => "Đang chạy", _ => "Đã dừng" };
}
