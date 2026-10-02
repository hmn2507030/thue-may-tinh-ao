using CloudRental.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CloudRental.Controllers;
public class HomeController(ApplicationDbContext db) : Controller {
    public async Task<IActionResult> Index() => View(await db.Plans.AsNoTracking().Where(x=>x.IsActive).OrderBy(x=>x.PricePerMonth).Take(3).ToListAsync());
    [ResponseCache(Duration=0, Location=ResponseCacheLocation.None, NoStore=true)]
    public IActionResult Error() { Response.StatusCode=500; return View(); }
}
