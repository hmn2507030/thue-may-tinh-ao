using CloudRental.Data;
using CloudRental.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CloudRental.Controllers;
public class PlansController(ApplicationDbContext db) : Controller {
    public async Task<IActionResult> Index(CatalogVm input) {
        var q=db.Plans.AsNoTracking().Where(x=>x.IsActive);
        if (!string.IsNullOrWhiteSpace(input.Q)) q=q.Where(x=>x.Name.Contains(input.Q.Trim()));
        if (input.Type.HasValue) q=q.Where(x=>x.Type==input.Type);
        if (input.MaxPrice.HasValue) q=q.Where(x=>x.PricePerMonth<=input.MaxPrice);
        if (input.MinCpu.HasValue) q=q.Where(x=>x.CPU>=input.MinCpu);
        if (input.MinRam.HasValue) q=q.Where(x=>x.RAM>=input.MinRam);
        if (input.MinStorage.HasValue) q=q.Where(x=>x.Storage>=input.MinStorage);
        input.Plans=await q.OrderBy(x=>x.PricePerMonth).ToListAsync();
        return View(input);
    }
    public async Task<IActionResult> Details(int id) {
        var plan=await db.Plans.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id && x.IsActive);
        return plan==null ? NotFound() : View(plan);
    }
}
