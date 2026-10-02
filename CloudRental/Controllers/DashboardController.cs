using System.Security.Claims;
using CloudRental.Data;
using CloudRental.Models;
using CloudRental.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CloudRental.Controllers;
[Authorize, ResponseCache(Duration=0, Location=ResponseCacheLocation.None, NoStore=true)]
public class DashboardController(ApplicationDbContext db, VmSecrets secrets) : Controller {
    string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index() => View(new DashboardVm {
        Machines=await db.VirtualMachines.AsNoTracking().Include(x=>x.Order).Where(x=>x.UserId==UserId).OrderByDescending(x=>x.CreatedAt).ToListAsync(),
        Orders=await db.Orders.AsNoTracking().Where(x=>x.UserId==UserId).OrderByDescending(x=>x.CreatedAt).Take(5).ToListAsync()
    });
    public async Task<IActionResult> Details(int id) {
        var vm=await db.VirtualMachines.AsNoTracking().Include(x=>x.Order).SingleOrDefaultAsync(x=>x.Id==id && x.UserId==UserId);
        if(vm==null) return NotFound();
        return View(new MachineDetailsVm { Machine=vm,Password=secrets.Reveal(vm.ProtectedPassword) });
    }
    [HttpPost] public async Task<IActionResult> Command(int id, string operation) {
        var vm=await db.VirtualMachines.SingleOrDefaultAsync(x=>x.Id==id && x.UserId==UserId);
        if(vm==null) return NotFound();
        if(vm.ExpiresAt<=DateTime.UtcNow) { TempData["Error"]="Gói thuê đã hết hạn, không thể điều khiển máy."; return RedirectToAction(nameof(Details),new{id}); }
        var next = (operation,vm.Status) switch {
            ("start",VmStatus.Stopped) => VmStatus.Creating,
            ("stop",VmStatus.Running) => VmStatus.Stopped,
            ("restart",VmStatus.Running) => VmStatus.Creating,
            _ => (VmStatus?)null
        };
        if (next==null) { TempData["Error"]="Thao tác không phù hợp với trạng thái hiện tại."; return RedirectToAction(nameof(Details),new{id}); }
        vm.Status=next.Value; vm.UpdatedAt=DateTime.UtcNow;
        try { await db.SaveChangesAsync(); TempData["Success"]="Đã gửi thao tác mô phỏng. Khởi động hoặc khởi động lại hoàn tất sau khoảng 5-8 giây khi ứng dụng đang chạy."; }
        catch(DbUpdateConcurrencyException) { TempData["Error"]="Trạng thái vừa thay đổi, vui lòng thao tác lại."; }
        return RedirectToAction(nameof(Details),new{id});
    }
}
