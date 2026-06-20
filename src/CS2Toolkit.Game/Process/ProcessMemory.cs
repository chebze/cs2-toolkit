using System.Runtime.InteropServices;
using System.Text;

namespace CS2Toolkit.Game.Process;

public sealed class ProcessMemory : IDisposable
{
    private const int ProcessVmRead = 0x0010;
    private const int ProcessQueryInformation = 0x0400;

    private nint _processHandle;
    private nint _clientBase;
    private readonly Dictionary<string, nint> _moduleBases = new(StringComparer.OrdinalIgnoreCase);

    public bool IsAttached => _processHandle != nint.Zero && _clientBase != nint.Zero;
    public nint ClientBase => _clientBase;

    public nint GetModuleBase(string moduleName) =>
        _moduleBases.TryGetValue(moduleName, out var moduleBase) ? moduleBase : nint.Zero;

    public bool AttachToProcess(string processName)
    {
        Detach();

        var processes = System.Diagnostics.Process.GetProcessesByName(processName);
        if (processes.Length == 0)
            return false;

        var process = processes[0];
        _processHandle = OpenProcess(ProcessVmRead | ProcessQueryInformation, false, process.Id);
        if (_processHandle == nint.Zero)
            return false;

        CacheModuleBases(process);
        _clientBase = GetModuleBase("client.dll");
        return _clientBase != nint.Zero;
    }

    public void Detach()
    {
        if (_processHandle != nint.Zero)
        {
            CloseHandle(_processHandle);
            _processHandle = nint.Zero;
        }

        _clientBase = nint.Zero;
        _moduleBases.Clear();
    }

    public T Read<T>(nint address) where T : unmanaged
    {
        if (_processHandle == nint.Zero)
            return default;

        var size = Marshal.SizeOf<T>();
        var buffer = new byte[size];
        if (!ReadProcessMemory(_processHandle, address, buffer, size, out _))
            return default;

        return MemoryMarshal.Read<T>(buffer);
    }

    public nint ReadPtr(nint address) => Read<nint>(address);

    public string ReadString(nint address, int maxLength = 64)
    {
        if (_processHandle == nint.Zero || address == nint.Zero)
            return string.Empty;

        var buffer = new byte[maxLength];
        if (!ReadProcessMemory(_processHandle, address, buffer, buffer.Length, out _))
            return string.Empty;

        var nullIndex = Array.IndexOf(buffer, (byte)0);
        if (nullIndex >= 0)
            return Encoding.UTF8.GetString(buffer, 0, nullIndex);

        return Encoding.UTF8.GetString(buffer);
    }

    public void Dispose() => Detach();

    private void CacheModuleBases(System.Diagnostics.Process process)
    {
        _moduleBases.Clear();
        foreach (System.Diagnostics.ProcessModule module in process.Modules)
            _moduleBases[module.ModuleName] = module.BaseAddress;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint OpenProcess(int access, bool inheritHandle, int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(nint process, nint address, byte[] buffer, int size, out int bytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(nint handle);
}
