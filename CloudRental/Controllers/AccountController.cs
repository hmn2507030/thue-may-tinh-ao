using CloudRental.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace CloudRental.Controllers;
public class AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : Controller {
    [HttpGet] public IActionResult Register() => User.Identity?.IsAuthenticated == true ? RedirectToAction("Index","Dashboard") : View(new RegisterVm());
    [HttpPost] public async Task<IActionResult> Register(RegisterVm input) {
        if (!ModelState.IsValid) return View(input);
        var email=input.Email.Trim();
        var user = new ApplicationUser {UserName=email,Email=email,FullName=input.FullName.Trim()};
        var result = await users.CreateAsync(user,input.Password);
        if (!result.Succeeded) { foreach(var error in result.Errors) ModelState.AddModelError("",error.Description); return View(input); }
        var roleResult = await users.AddToRoleAsync(user,"Customer");
        if (!roleResult.Succeeded) {
            await users.DeleteAsync(user); ModelState.AddModelError("","Chưa tạo được tài khoản, vui lòng thử lại."); return View(input);
        }
        await signIn.SignInAsync(user,isPersistent:false);
        TempData["Success"]="Chào mừng bạn! Tài khoản đã sẵn sàng.";
        return RedirectToAction("Index","Dashboard");
    }
    [HttpGet] public IActionResult Login(string? returnUrl=null) => View(new LoginVm { ReturnUrl=returnUrl });
    [HttpPost] public async Task<IActionResult> Login(LoginVm input) {
        if (!ModelState.IsValid) return View(input);
        var result = await signIn.PasswordSignInAsync(input.Email.Trim(),input.Password,input.RememberMe,lockoutOnFailure:true);
        if (result.Succeeded) return Url.IsLocalUrl(input.ReturnUrl) ? LocalRedirect(input.ReturnUrl!) : RedirectToAction("Index","Dashboard");
        ModelState.AddModelError("",result.IsLockedOut ? "Tài khoản tạm khóa do đăng nhập sai nhiều lần. Thử lại sau 10 phút." : "Email hoặc mật khẩu không đúng.");
        return View(input);
    }
    [Authorize, HttpPost] public async Task<IActionResult> Logout() { await signIn.SignOutAsync(); return RedirectToAction("Index","Home"); }
    public IActionResult Denied() { Response.StatusCode=403; return View(); }
}
