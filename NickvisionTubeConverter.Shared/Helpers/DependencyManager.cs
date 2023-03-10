﻿using NickvisionTubeConverter.Shared.Controllers;
using NickvisionTubeConverter.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xabe.FFmpeg.Downloader;

namespace NickvisionTubeConverter.Shared.Helpers;

internal static class DependencyManager
{
    /// <summary>
    /// The path for ffmpeg
    /// </summary>
    public static string Ffmpeg
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Path.DirectorySeparatorChar}Nickvision{Path.DirectorySeparatorChar}{AppInfo.Current.Name}{Path.DirectorySeparatorChar}ffmpeg.exe";
            }
            var prefixes = new List<string>() {
                Directory.GetParent(Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!))!.FullName)!.FullName,
                Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!))!.FullName,
                "/usr"
            };
            foreach (var prefix in prefixes)
            {
                var path = $"{prefix}/bin/ffmpeg";
                if (File.Exists(path))
                {
                    return path;
                }
            }
            return "";
        }
    }

    /// <summary>
    /// Setups dependencies for the application
    /// </summary>
    /// <returns>True if successful, else false.</returns>
    public static async Task<bool> SetupDependenciesAsync()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                await PythonExtensions.DeployEmbeddedAsync(new Version("3.11.2"));
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, Ffmpeg.Remove(Ffmpeg.IndexOf("ffmpeg.exe")));
            }
            else
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "python3",
                        Arguments = "-c \"import sysconfig; print('/'.join(sysconfig.get_config_vars('LIBDIR', 'INSTSONAME')))\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };
                process.Start();
                Python.Runtime.Runtime.PythonDLL = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
            }
            return true;
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}
