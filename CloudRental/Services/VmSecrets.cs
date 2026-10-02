using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
namespace CloudRental.Services;
public class VmSecrets {
    private readonly IDataProtector protector;
    public VmSecrets(IDataProtectionProvider provider) { protector = provider.CreateProtector("VmDemoPassword.v1"); }
    public string CreateProtected() => protector.Protect("Vm!" + Convert.ToHexString(RandomNumberGenerator.GetBytes(12)) + "a");
    public string Reveal(string value) => protector.Unprotect(value);
}
