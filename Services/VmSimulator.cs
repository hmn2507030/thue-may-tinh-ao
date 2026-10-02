using CloudRental.Data;
using CloudRental.Models;
using Microsoft.EntityFrameworkCore;
namespace CloudRental.Services;
public class VmSimulator(IServiceScopeFactory factory, ILogger<VmSimulator> logger) : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken token) {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
        try {
            while (await timer.WaitForNextTickAsync(token)) {
                try {
                    using var scope = factory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var now = DateTime.UtcNow;
                    var readyBefore = now.AddSeconds(-5);
                    var list = await db.VirtualMachines.Where(x =>
                        (x.Status == VmStatus.Creating && x.UpdatedAt <= readyBefore) ||
                        (x.ExpiresAt <= now && x.Status != VmStatus.Stopped)).ToListAsync(token);
                    foreach (var vm in list) {
                        vm.Status = vm.ExpiresAt <= now ? VmStatus.Stopped : VmStatus.Running;
                        vm.UpdatedAt = now;
                    }
                    if (list.Count > 0) await db.SaveChangesAsync(token);
                } catch (DbUpdateConcurrencyException) { /* Một thao tác mới hơn thắng; đọc lại ở chu kỳ kế tiếp. */ }
                  catch (OperationCanceledException) when (token.IsCancellationRequested) { break; }
                  catch (Exception ex) { logger.LogError(ex,"Không cập nhật được trạng thái mô phỏng."); }
            }
        } catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    }
}
