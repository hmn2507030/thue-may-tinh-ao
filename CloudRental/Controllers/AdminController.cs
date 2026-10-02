using CloudRental.Data;
using CloudRental.Models;
using CloudRental.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CloudRental.Controllers;
[Authorize(Roles="Admin"), ResponseCache(Duration=0, Location=ResponseCacheLocation.None, NoStore=true)]
public class AdminController(ApplicationDbContext db, VmSecrets secrets) : Controller {
    public async Task<IActionResult> Index() => View(new AdminOverviewVm {
        Users=await db.Users.CountAsync(), Plans=await db.Plans.CountAsync(),
        Pending=await db.Orders.CountAsync(x=>x.Status==OrderStatus.Pending), Machines=await db.VirtualMachines.CountAsync(),
        ApprovedTotal=await db.Orders.Where(x=>x.Status==OrderStatus.Approved).SumAsync(x=>(decimal?)x.Total) ?? 0,
        Recent=await db.Orders.AsNoTracking().Include(x=>x.User).OrderByDescending(x=>x.CreatedAt).Take(8).ToListAsync()
    });
    public async Task<IActionResult> Plans() => View(await db.Plans.AsNoTracking().OrderBy(x=>x.Type).ThenBy(x=>x.PricePerMonth).ToListAsync());
    [HttpGet] public async Task<IActionResult> EditPlan(int? id) {
        if(id==null) return View(new Plan {PricePerMonth=149000});
        var plan=await db.Plans.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id);
        return plan==null ? NotFound() : View(plan);
    }
    [HttpPost] public async Task<IActionResult> EditPlan([Bind("Id,Name,Type,CPU,RAM,Storage,PricePerMonth,Description,IsActive")] Plan input) {
        if (!ModelState.IsValid) return View(input);
        if(input.Id==0) db.Plans.Add(input);
        else {
            var plan=await db.Plans.FindAsync(input.Id); if(plan==null) return NotFound();
            plan.Name=input.Name; plan.Type=input.Type; plan.CPU=input.CPU; plan.RAM=input.RAM; plan.Storage=input.Storage;
            plan.PricePerMonth=input.PricePerMonth; plan.Description=input.Description; plan.IsActive=input.IsActive;
        }
        await db.SaveChangesAsync(); TempData["Success"]="Đã lưu gói dịch vụ. Các đơn cũ vẫn giữ giá và cấu hình tại thời điểm đặt.";
        return RedirectToAction(nameof(Plans));
    }
    [HttpPost] public async Task<IActionResult> TogglePlan(int id) {
        var plan=await db.Plans.FindAsync(id); if(plan==null) return NotFound();
        plan.IsActive=!plan.IsActive; await db.SaveChangesAsync(); return RedirectToAction(nameof(Plans));
    }
    [HttpPost] public async Task<IActionResult> DeletePlan(int id) {
        var plan=await db.Plans.FindAsync(id); if(plan==null) return NotFound();
        if(await db.Orders.AnyAsync(x=>x.PlanId==id)) { TempData["Error"]="Gói đã có đơn hàng. Hãy ẩn gói để bảo toàn lịch sử."; return RedirectToAction(nameof(Plans)); }
        db.Plans.Remove(plan);
        try { await db.SaveChangesAsync(); TempData["Success"]="Đã xóa gói dịch vụ."; }
        catch(DbUpdateException) { TempData["Error"]="Không thể xóa gói đã phát sinh dữ liệu liên quan. Hãy ẩn gói."; }
        return RedirectToAction(nameof(Plans));
    }
    public async Task<IActionResult> Orders(OrderStatus? status) {
        ViewBag.Status=status;
        var query=db.Orders.AsNoTracking().Include(x=>x.User).AsQueryable();
        if(status.HasValue) query=query.Where(x=>x.Status==status);
        return View(await query.OrderByDescending(x=>x.CreatedAt).ToListAsync());
    }
    [HttpPost] public async Task<IActionResult> Review(int id, string decision) {
        var order=await db.Orders.SingleOrDefaultAsync(x=>x.Id==id);
        if(order==null) return NotFound();
        if(order.Status!=OrderStatus.Pending) { TempData["Error"]="Đơn không còn chờ duyệt."; return RedirectToAction(nameof(Orders)); }
        if(decision=="approve") {
            order.Status=OrderStatus.Approved; order.ApprovedAt=DateTime.UtcNow;
            db.VirtualMachines.Add(new VirtualMachine { OrderId=order.Id,UserId=order.UserId,PlanId=order.PlanId,
                VmName="cloud-"+order.Id.ToString("D5"),IpAddress="192.0.2."+(1+order.Id%254),
                ProtectedPassword=secrets.CreateProtected(),ExpiresAt=DateTime.UtcNow.AddMonths(order.Months) });
        } else if(decision=="reject") order.Status=OrderStatus.Rejected;
        else return BadRequest();
        try { await db.SaveChangesAsync(); TempData["Success"]="Đã cập nhật đơn hàng. Đơn được duyệt sẽ có máy ảo mô phỏng."; }
        catch(DbUpdateConcurrencyException) { TempData["Error"]="Đơn vừa được xử lý bởi thao tác khác."; }
        return RedirectToAction(nameof(Orders));
    }
    public async Task<IActionResult> Machines() => View(await db.VirtualMachines.AsNoTracking().Include(x=>x.User).Include(x=>x.Order).OrderByDescending(x=>x.CreatedAt).ToListAsync());
}
