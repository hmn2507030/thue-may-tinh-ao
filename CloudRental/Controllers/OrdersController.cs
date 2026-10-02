using System.Security.Claims;
using CloudRental.Data;
using CloudRental.Models;
using CloudRental.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CloudRental.Controllers;
[Authorize]
public class OrdersController(ApplicationDbContext db) : Controller {
    string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index() => View(await db.Orders.AsNoTracking().Where(x=>x.UserId==UserId).OrderByDescending(x=>x.CreatedAt).ToListAsync());
    [HttpGet] public async Task<IActionResult> Checkout(int id) {
        var plan=await db.Plans.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id && x.IsActive);
        return plan==null ? NotFound() : View(new CheckoutVm { PlanId=plan.Id,Plan=plan });
    }
    [HttpPost] public async Task<IActionResult> Checkout(CheckoutVm input) {
        var plan=await db.Plans.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==input.PlanId && x.IsActive);
        if (plan==null) return NotFound();
        input.Plan=plan;
        if (!RentalPricing.AllowedMonths.Contains(input.Months)) ModelState.AddModelError(nameof(input.Months),"Chọn 1, 3, 6 hoặc 12 tháng.");
        if (!ModelState.IsValid) return View(input);
        db.Orders.Add(new Order { UserId=UserId,PlanId=plan.Id,PlanName=plan.Name,CPU=plan.CPU,RAM=plan.RAM,Storage=plan.Storage,Type=plan.Type,
            Months=input.Months,UnitPrice=plan.PricePerMonth,Total=RentalPricing.Total(plan.PricePerMonth,input.Months) });
        await db.SaveChangesAsync();
        TempData["Success"]="Đã tạo đơn hàng chờ quản trị viên duyệt. Đây là giao dịch mô phỏng, bạn không bị thu tiền.";
        return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> Cancel(int id) {
        var order=await db.Orders.SingleOrDefaultAsync(x=>x.Id==id && x.UserId==UserId);
        if (order==null) return NotFound();
        if (order.Status!=OrderStatus.Pending) { TempData["Error"]="Chỉ có thể hủy đơn đang chờ duyệt."; return RedirectToAction(nameof(Index)); }
        order.Status=OrderStatus.Cancelled;
        try { await db.SaveChangesAsync(); TempData["Success"]="Đã hủy đơn hàng."; }
        catch(DbUpdateConcurrencyException) { TempData["Error"]="Đơn hàng vừa thay đổi. Vui lòng kiểm tra lại."; }
        return RedirectToAction(nameof(Index));
    }
}
