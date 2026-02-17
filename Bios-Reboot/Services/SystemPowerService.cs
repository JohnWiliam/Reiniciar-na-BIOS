using System.Diagnostics;

namespace Bios Reboot.Services;

public interface ISystemPowerService
{
    void RestartToFirmware();
}

public class SystemPowerService : ISystemPowerService
{
    public void RestartToFirmware()
    {
        var psi = new ProcessStartInfo("shutdown", "/r /fw /t 0")
        {
            CreateNoWindow = true,
            UseShellExecute = false
        };
        Process.Start(psi);
    }
}
