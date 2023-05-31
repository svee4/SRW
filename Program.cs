using System;
using System.Runtime.InteropServices;


namespace SRW;

/// https://stackoverflow.com/questions/41516979
// https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-idesktopwallpaper


/// <summary>
/// IDesktopWallpaper COM interface<br/>
/// Stripped down version of https://stackoverflow.com/questions/41516979
/// </summary>
[ComImport]
[Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IDesktopWallpaper
{
	// null monitorID -> all monitors
	void SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string? monitorID, [MarshalAs(UnmanagedType.LPWStr)] string wallpaper);

	[return: MarshalAs(UnmanagedType.LPWStr)]
	string GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID);

	[return: MarshalAs(UnmanagedType.LPWStr)]
	string GetMonitorDevicePathAt(uint monitorIndex);

	// [return: MarshalAs(UnmanagedType.U4)]
	// uint GetMonitorDevicePathCount();

	// void SetBackgroundColor([MarshalAs(UnmanagedType.U4)] uint color);

	// void SetPosition([MarshalAs(UnmanagedType.I4)] DesktopWallpaperPosition position);

}

public enum DesktopWallpaperPosition
{
	Center = 0,
	Tile = 1,
	Stretch = 2,
	Fit = 3,
	Fill = 4,
	Span = 5,
}

[ComImport]
[Guid("C2CF3110-460E-4fc1-B9D0-8A1C0C9CC4BD")]
public class DesktopWallpaper { }

internal class Program
{

	static void Main(string[] args)
	{
        if (args.FirstOrDefault(x => x == "--help") is not null)
		{
			Console.WriteLine("""
Randomize your wallpaper :o

Arguments:
--directory=<string> | --directory="<string>" // (Required) Directory to pick a random wallpaper from
--sleep=<integer> // (Optional) Amount of milliseconds to sleep for, before checking if the wallpaper has updated correctly

""");
			return;
		}
		
		string? directory = args.FirstOrDefault(x => x.StartsWith("--directory="))?["--directory=".Length..]?.Trim('"');
		
		if (!int.TryParse(
				args.FirstOrDefault(x => x.StartsWith("--sleep="))?["--sleep=".Length..], 
				out int sleep)
			) sleep = -1;

#if DEBUG // got too lazy
		if (string.IsNullOrEmpty(directory))
		{
			Console.WriteLine("Directory: ");
			directory = Console.ReadLine();
		}
		if (sleep == -1)
		{
			Console.WriteLine("Sleep: ");
			if (!int.TryParse(Console.ReadLine(), out sleep)) sleep = -1;
		}
#endif

		if (string.IsNullOrEmpty(directory))
		{
			Log($"Missing required argument --directory", LogLevel.Fatal);
			return;
		}
		else if (!Directory.Exists(directory))
		{
			Log($"Directory does not exist: '{directory}'", LogLevel.Fatal);
			return;
		}

		string[] files = Directory.GetFiles(directory);
		if (files.Length == 0)
		{
			Log($"No files found in directory: '{directory}'", LogLevel.Fatal);
			return;
		}

		
		// magic happens here and we get an instance connected to the COM object or something along those lines
		IDesktopWallpaper p = (IDesktopWallpaper)new DesktopWallpaper();

		string monitor = p.GetMonitorDevicePathAt(0);
		string current = p.GetWallpaper(monitor);

		string file;
		do file = files[new Random().Next(files.Length)]; // dont pick the current wallpaper again
		while (file == current && files.Length > 1); // unless theres only one file

		p.SetWallpaper(null, file);
		
		
		if (sleep != -1) Thread.Sleep(sleep); // it takes a while to update
		string @new = p.GetWallpaper(monitor);
		
		if (string.IsNullOrEmpty(@new)) // happens... for exactly one file that i dont have anymore
			Log($"Failed to set file '{file}' as wallpaper (Result was empty)", LogLevel.Error);
		else if (@new != file)
			Log($"Failed to set file '{file}' as wallpaper (Result: '{@new}')", LogLevel.Error);
	}


	static void Log(string message, LogLevel level)
	{
#if DEBUG
		Console.WriteLine(message);
#endif
		File.AppendAllLines("log.txt", new[] { $"{DateTime.Now} [{level}] {message}" });
	}

	enum LogLevel
	{
		Info,
		Warning,
		Error,
		Fatal
	}

}
