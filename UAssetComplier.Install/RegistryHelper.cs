using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAssetComplier.Install
{
    public class RegistryHelper
    {
        /// <summary>
        /// 计算机\HKEY_CURRENT_USER\Software\Classes\*\shell
        /// </summary>
        private static RegistryKey RegistryUserSoftwareClassesAllShell
        {
            get
            {
                return Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes").OpenSubKey("*").OpenSubKey("shell", true);
            }
        }
        /// <summary>
        /// 生成右键菜单按钮（当前用户）
        /// </summary>
        /// <param name="keyName">右键菜单名</param>
        /// <param name="programPath">点击按钮后打开的程序路径</param>
        public static void CreateUserContextMenu(string keyName, string programPath)
        {
            if (!RegistryUserSoftwareClassesAllShell.GetSubKeyNames().Contains(keyName))
            {
                var contextMenuRegistryKey = RegistryUserSoftwareClassesAllShell.CreateSubKey(keyName);
                contextMenuRegistryKey.SetValue("", keyName);
                var command = contextMenuRegistryKey.CreateSubKey("command");
                command.SetValue("", $"\"{programPath}\" \"%1\"");
            }
        }
        /// <summary>
        /// 删除右键菜单按钮（当前用户）
        /// </summary>
        /// <param name="keyName">右键菜单名</param>
        public static void DeleteUserContextMenu(string keyName)
        {
            if (RegistryUserSoftwareClassesAllShell.GetSubKeyNames().Contains(keyName))
            {
                RegistryUserSoftwareClassesAllShell.DeleteSubKeyTree(keyName);
            }
        }
    }
}
