# CloudRental — Website cho thuê máy tính ảo

Sourcecode tham chiếu cho đồ án cơ sở **"Xây dựng Hệ Thống Web Cho Thuê Máy Tính Ảo"** (Trường Đại học Nguyễn Tất Thành). Hệ thống cho phép khách hàng chọn gói VPS / Virtual Desktop, đặt thuê, theo dõi đơn và điều khiển máy ảo **mô phỏng**; quản trị viên duyệt đơn, quản lý gói và xem máy ảo của khách.

> **Phạm vi đồ án.** Không triển khai hạ tầng ảo hóa thật (không Hyper-V / Docker / KVM), không tích hợp cổng thanh toán thật và không hỗ trợ SSH / Remote Desktop thật. Thông tin kết nối (IP, username, password) là dữ liệu mẫu.

## Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Backend | ASP.NET Core MVC (.NET 8, C#) |
| Cơ sở dữ liệu | SQL Server (LocalDB / Express) + Entity Framework Core |
| Xác thực & phân quyền | ASP.NET Core Identity (role Admin / Customer) |
| Giao diện | Razor Views, Bootstrap 5, CSS3, JavaScript thuần |

## Chức năng chính

- Trang chủ giới thiệu gói dịch vụ nổi bật và banner quảng cáo.
- Danh sách gói dịch vụ có bộ lọc theo loại (VPS / Virtual Desktop), khoảng giá và cấu hình (CPU, RAM, SSD).
- Trang chi tiết gói: cấu hình kỹ thuật, giá thuê và chính sách hỗ trợ.
- Đặt hàng: chọn gói, chọn thời hạn 1 / 3 / 6 / 12 tháng và tính tổng tiền (giả lập).
- Đăng ký, đăng nhập, phân quyền Customer / Admin.
- Dashboard Khách hàng: danh sách máy ảo, trạng thái, thông tin kết nối mẫu, nút Start / Stop / Restart.
- Dashboard Admin: quản lý gói dịch vụ, duyệt hoặc từ chối đơn hàng, xem máy ảo toàn hệ thống.
- Mô phỏng trạng thái máy ảo: Creating → Running → Stopped, tự chuyển sang Running sau vài giây.

## Cấu trúc thư mục

```text
CloudRental/
├── Controllers/     Home, Account, Plans, Orders, Dashboard, Admin
├── Models/          ApplicationUser, Plan, Order, VirtualMachine + ViewModels
├── Data/            ApplicationDbContext (EF Core), SeedData (role, tài khoản, gói mẫu)
├── Services/        RentalPricing, VmSecrets, VmSimulator (worker mô phỏng)
├── Views/           Shared, Home, Account, Plans, Orders, Dashboard, Admin
└── wwwroot/         css/site.css, js/site.js
```

## Yêu cầu môi trường

- .NET SDK 8.0 trở lên.
- SQL Server Express, LocalDB hoặc SQL Server Developer.
- Visual Studio 2022 (hoặc VS Code + CLI) — tùy chọn.

## Cấu hình và chạy

1. Mở `CloudRental.csproj` bằng Visual Studio hoặc terminal tại thư mục dự án.
2. (Nên làm) Khai báo chuỗi kết nối và tài khoản seed bằng secrets để không lưu mật khẩu trong `appsettings.json`:

```bash
cd CloudRental
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=CloudRentalDemo;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Seed:AdminPassword" "Admin@123456"
dotnet user-secrets set "Seed:DemoPassword" "Demo@123456"
```

3. Khôi phục gói và chạy:

```bash
dotnet restore
dotnet run
```

4. Mở trình duyệt tại địa chỉ HTTPS hiển thị trong console (mặc định `https://localhost:7157`).

Ứng dụng tự tạo database `CloudRentalDemo`, các vai trò `Admin` / `Customer`, một gói mẫu cho mỗi loại dịch vụ và hai tài khoản seed.

Bảng dữ liệu: `AspNetUsers` (người dùng), `Plans` (gói dịch vụ), `Orders` (đơn hàng, có trường RowVersion), `VirtualMachines` (máy ảo mô phỏng).

## Cách chạy luồng demo

1. **Đăng nhập Admin** bằng `Seed:AdminEmail` (mặc định `admin@cloudrental.local`) và `Seed:AdminPassword`.
2. **Mở tab ẩn danh**, đăng nhập bằng tài khoản demo khách hàng hoặc **đăng ký tài khoản mới**.
3. Vào **Gói dịch vụ** → chọn một gói → chọn thời hạn → **Xác nhận đặt thuê**. Đơn có trạng thái *Chờ duyệt*.
4. Quay lại tab Admin → **Đơn hàng** → **Duyệt**. Hệ thống tạo máy ảo mô phỏng và cấp thông tin kết nối mẫu.
5. Về tab khách hàng → **Máy ảo của tôi** → **Chi tiết & điều khiển** → thử **Start / Stop / Restart** và quan sát trạng thái chuyển *Đang khởi tạo* → *Đang chạy*.

Mật khẩu kết nối máy ảo mô phỏng được bảo vệ bằng Data Protection API của ASP.NET Core; ứng dụng chỉ giải mã khi chính chủ sở hữu mở trang chi tiết (cần đăng nhập).

## Ghi chú khi mở rộng

- Schema được tạo bằng `EnsureCreated()` để thuận tiện cho demo. Khi phát triển tiếp, chuyển sang EF Core Migrations:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

- Mọi thao tác POST đều yêu cầu anti-forgery token; quyền truy cập được kiểm tra ở tầng controller và lọc theo chủ sở hữu dữ liệu.
- Truy vấn hiển thị dùng `AsNoTracking()`; thao tác cập nhật trạng thái máy ảo dùng RowVersion để tránh ghi đè khi có hai yêu cầu cùng lúc.
- Tổng tiền đơn hàng được tính lại ở server, không tin dữ liệu gửi từ trình duyệt.
- Có thể thay thế worker mô phỏng bằng tích hợp API cloud provider thật trong hướng phát triển của đồ án.
