using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SsmConsole {

    /// <summary>
    /// 注册表操作类
    /// </summary>
    public static class Registry {

        /// <summary>
        /// 注册文件关联
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="name"></param>
        /// <param name="decription"></param>
        /// <param name="command"></param>
        /// <param name="iconPath"></param>
        /// <exception cref="Exception"></exception>
        public static void RegisterFileAssociations(string ext, string name, string decription, string command, string iconPath) {
            // 注册软件路径
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                // Windows
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                var isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
                if (isElevated) {
                    // 建立关联产品
                    var keyProduct = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(name, true);
                    if (eggs.Object.IsNull(keyProduct)) {
                        keyProduct = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(name);
                    }
                    keyProduct.SetValue("", decription);
                    // 建立关联图标
                    var keyProductDefaultIcon = keyProduct.OpenSubKey("DefaultIcon", true);
                    if (eggs.Object.IsNull(keyProductDefaultIcon)) {
                        keyProductDefaultIcon = keyProduct.CreateSubKey("DefaultIcon");
                    }
                    keyProductDefaultIcon.SetValue("", $"\"{iconPath}\"");
                    // 建立shell
                    var keyProductShell = keyProduct.OpenSubKey("shell", true);
                    if (eggs.Object.IsNull(keyProductShell)) {
                        keyProductShell = keyProduct.CreateSubKey("shell");
                    }

                    // 建立open
                    var keyProductShellOpen = keyProductShell.OpenSubKey("open", true);
                    if (eggs.Object.IsNull(keyProductShellOpen)) {
                        keyProductShellOpen = keyProductShell.CreateSubKey("open");
                    }
                    // 建立command
                    var keyProductShellOpenCommand = keyProductShellOpen.OpenSubKey("command", true);
                    if (eggs.Object.IsNull(keyProductShellOpenCommand)) {
                        keyProductShellOpenCommand = keyProductShellOpen.CreateSubKey("command");
                    }
                    keyProductShellOpenCommand.SetValue("", command);
                    //keyProductShellOpenCommand.SetValue("", $"\"{exePath}\" \"%1\"");
                    // 建立关联扩展名
                    var keyFile = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext, true);
                    if (eggs.Object.IsNull(keyFile)) {
                        keyFile = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(ext);
                    }
                    // 建立关联扩展名打开方式
                    var keyFileOpenWithProgids = keyFile.OpenSubKey("OpenWithProgids", true);
                    if (eggs.Object.IsNull(keyFileOpenWithProgids)) {
                        keyFileOpenWithProgids = keyFile.CreateSubKey("OpenWithProgids");
                    }
                    keyFileOpenWithProgids.SetValue(name, "");
                    Console.WriteLine("注册成功!");
                } else {
                    throw new Exception("权限不足，请使用管理员权限运行");
                }
            } else {
                throw new Exception($"不支持的操作系统'{RuntimeInformation.OSDescription}'");
            }
        }

        /// <summary>
        /// 注册文件关联
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cmdName"></param>
        /// <param name="cmdContent"></param>
        /// <exception cref="Exception"></exception>
        public static void RegisterFileAssociations(string name, string cmdName, string cmdContent) {
            // 注册软件路径
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                // Windows
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                var isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
                if (isElevated) {
                    // 建立关联产品
                    var keyProduct = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(name, true);
                    if (eggs.Object.IsNull(keyProduct)) {
                        keyProduct = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(name);
                    }
                    // 建立关联图标
                    var keyProductDefaultIcon = keyProduct.OpenSubKey("DefaultIcon", true);
                    if (eggs.Object.IsNull(keyProductDefaultIcon)) {
                        keyProductDefaultIcon = keyProduct.CreateSubKey("DefaultIcon");
                    }
                    // 建立shell
                    var keyProductShell = keyProduct.OpenSubKey("shell", true);
                    if (eggs.Object.IsNull(keyProductShell)) {
                        keyProductShell = keyProduct.CreateSubKey("shell");
                    }
                    // 建立open
                    var keyProductShellOpen = keyProductShell.OpenSubKey("open", true);
                    if (eggs.Object.IsNull(keyProductShellOpen)) {
                        keyProductShellOpen = keyProductShell.CreateSubKey("open");
                    }

                    // 建立open
                    var keyProductShellDebug = keyProductShell.OpenSubKey(cmdName, true);
                    if (eggs.Object.IsNull(keyProductShellDebug)) {
                        keyProductShellDebug = keyProductShell.CreateSubKey(cmdName);
                    }
                    // 建立command
                    var keyProductShellDebugCommand = keyProductShellDebug.OpenSubKey("command", true);
                    if (eggs.Object.IsNull(keyProductShellDebugCommand)) {
                        keyProductShellDebugCommand = keyProductShellDebug.CreateSubKey("command");
                    }
                    keyProductShellDebugCommand.SetValue("", cmdContent);
                    Console.WriteLine("注册成功!");
                } else {
                    throw new Exception("权限不足，请使用管理员权限运行");
                }
            } else {
                throw new Exception($"不支持的操作系统'{RuntimeInformation.OSDescription}'");
            }
        }

    }
}
