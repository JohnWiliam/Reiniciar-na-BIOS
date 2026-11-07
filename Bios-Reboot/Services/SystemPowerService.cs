using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BiosReboot.Services;

public interface ISystemPowerService
{
    Task RestartToFirmwareAsync(CancellationToken cancellationToken = default);
}

public sealed class SystemPowerService : ISystemPowerService
{
    public async Task RestartToFirmwareAsync(CancellationToken cancellationToken = default)
    {
        string systemDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System);
        string shutdownExePath = Path.Combine(systemDirectory, "shutdown.exe");

        if (!File.Exists(shutdownExePath))
        {
            throw new FileNotFoundException(
                "O executável nativo de desligamento do sistema não foi encontrado.",
                shutdownExePath);
        }

        var psi = new ProcessStartInfo
        {
            FileName = shutdownExePath,
            CreateNoWindow = true,
            UseShellExecute = false,
            WorkingDirectory = systemDirectory
        };

        // Uso de ArgumentList evita injeção de argumentos e problemas de parsing
        psi.ArgumentList.Add("/r");
        psi.ArgumentList.Add("/fw");
        psi.ArgumentList.Add("/t");
        psi.ArgumentList.Add("0");

        using Process process = Process.Start(psi)
            ?? throw new InvalidOperationException("Não foi possível iniciar o processo de reinicialização.");

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        if (process.ExitCode != 0)
        {
            throw new Win32Exception(
                process.ExitCode,
                $"Falha ao solicitar reinicialização para o firmware UEFI (Código: {process.ExitCode}). " +
                "Verifique se o sistema está em modo UEFI e não em Legacy/CSM.");
        }
    }
}
