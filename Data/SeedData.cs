using CloudRental.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace CloudRental.Data;
public static class SeedData {
    public static async Task Initialize(IServiceProvider services, IConfiguration config) {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync(); // Đồ án/demo. Khi phát triển schema, chuyển sang migrations.
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[]{"Admin","Customer"}) if (!await roles.RoleExistsAsync(role)) {
            var result = await roles.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded) throw new InvalidOperationException("Không tạo được role: " + role);
        }
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        await AddUser(users, config["Seed:AdminEmail"], config["Seed:AdminPassword"], "Quản trị viên", "Admin");
        await AddUser(users, config["Seed:DemoEmail"], config["Seed:DemoPassword"], "Khách hàng demo", "Customer");
        if (!await db.Plans.AnyAsync()) {
            db.Plans.AddRange(
                new Plan { Name="VPS Starter", Type=PlanType.VPS, CPU=2,RAM=4,Storage=60,PricePerMonth=149000,Description="Máy chủ Linux mô phỏng dành cho học tập, website cá nhân và thử nghiệm backend." },
                new Plan { Name="VPS Developer", Type=PlanType.VPS, CPU=4,RAM=8,Storage=120,PricePerMonth=299000,Description="Không gian phát triển cho ứng dụng web, cơ sở dữ liệu và các dự án nhóm." },
                new Plan { Name="VPS Performance", Type=PlanType.VPS, CPU=8,RAM=16,Storage=240,PricePerMonth=599000,Description="Cấu hình mạnh cho các bài thực hành nhiều dịch vụ. Hạ tầng trong hệ thống chỉ là mô phỏng." },
                new Plan { Name="Desktop Study", Type=PlanType.VirtualDesktop, CPU=2,RAM=8,Storage=80,PricePerMonth=249000,Description="Máy tính Windows mô phỏng cho học tập và văn phòng. Không có kết nối Remote Desktop thật." },
                new Plan { Name="Desktop Pro", Type=PlanType.VirtualDesktop, CPU=4,RAM=16,Storage=160,PricePerMonth=449000,Description="Môi trường desktop mô phỏng cho lập trình, bài tập và làm việc nhóm." },
                new Plan { Name="Desktop Studio", Type=PlanType.VirtualDesktop, CPU=8,RAM=32,Storage=320,PricePerMonth=849000,Description="Gói desktop cấu hình cao để trình diễn nghiệp vụ thuê máy tính ảo trong đồ án." }
            );
            await db.SaveChangesAsync();
        }
    }
    static async Task AddUser(UserManager<ApplicationUser> manager, string? email, string? password, string name, string role) {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
        var user = await manager.FindByEmailAsync(email);
        if (user == null) {
            user = new ApplicationUser { UserName=email,Email=email,FullName=name,EmailConfirmed=true };
            var result = await manager.CreateAsync(user,password);
            if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ",result.Errors.Select(x=>x.Description)));
        }
        if (!await manager.IsInRoleAsync(user,role)) {
            var result = await manager.AddToRoleAsync(user,role);
            if (!result.Succeeded) throw new InvalidOperationException("Không thể gán quyền tài khoản seed.");
        }
    }
}
