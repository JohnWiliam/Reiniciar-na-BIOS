using System.Diagnostics;

namespace BiosReboot.Services;

public interface ISystemPowerService
{
    Task RestartToFirmwareAsync();
}

public class SystemPowerService : ISystemPowerService
{
    public Task RestartToFirmwareAsync()
    {
        return Task.Run(() =>
        {
            var psi = new ProcessStartInfo("shutdown", "/r /fw /t 0")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using var _ = Process.Start(psi);
        });
    }
}
