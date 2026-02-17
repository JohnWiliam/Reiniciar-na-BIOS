using System.Diagnostics;

namespace BiosReboot.Services;

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
