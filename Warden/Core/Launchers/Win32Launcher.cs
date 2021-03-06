﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warden.Core.Exceptions;
using Warden.Properties;

namespace Warden.Core.Launchers
{
    internal class Win32Launcher : ILauncher
    {
        public async Task<WardenProcess> Launch(string path, string arguments)
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = string.IsNullOrWhiteSpace(arguments) ? string.Empty : arguments
                });
                if (process == null)
                {
                    throw new WardenLaunchException(Resources.Exception_Process_Not_Launched_Unknown);
                }
                await WaitForProcessStart(process, TimeSpan.FromSeconds(10));
                var warden = new WardenProcess(process.ProcessName, process.Id, path,
                    process.HasExited ? ProcessState.Dead : ProcessState.Alive, arguments, ProcessTypes.Win32);
                return warden;
            }
            catch (Exception ex)
            {
                throw new WardenLaunchException(string.Format(Resources.Exception_Process_Not_Launched, ex.Message), ex);
            }
        }

        public Task<WardenProcess> LaunchUri(string uri, string path, string arguments)
        {
            throw new NotImplementedException();
        }


        internal static async Task<bool> WaitForProcessStart(Process process, TimeSpan processTimeout)
        {
            var processStopwatch = Stopwatch.StartNew();
            var isProcessRunning = false;
            while (processStopwatch.ElapsedMilliseconds <= processTimeout.TotalMilliseconds && !isProcessRunning)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(2));
                try
                {
                    if (process != null)
                    {
                        isProcessRunning = process.Handle != IntPtr.Zero && process.Id > 0 && !process.HasExited;
                    }
                }
                catch
                {
                    isProcessRunning = false;
                }
            }
            return isProcessRunning;
        }
    }
}
