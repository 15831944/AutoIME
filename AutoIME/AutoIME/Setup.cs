﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AcadDocument = Autodesk.AutoCAD.ApplicationServices.Document;
using AcadWindows = Autodesk.AutoCAD.Windows;

namespace AutoIME
{
    static class Setup
    {
        private static Config _config = Config.GetConfigInstance();

        private static RegistryKey GetRootKey()
        {
            // Get Registry key
            var rootKey = HostApplicationServices.Current.MachineRegistryProductRootKey;
            var regAcadProdKey = Registry.CurrentUser.OpenSubKey(rootKey);
            return regAcadProdKey.OpenSubKey("Applications", true);
        }

        public static void Register(string appName)
        {
            // Get Registry key

            var regAcadAppKey = GetRootKey();

            // Check if `appName` exists
            var subKeys = regAcadAppKey.GetSubKeyNames();
            if (subKeys.Contains(appName))
            {
                regAcadAppKey.DeleteSubKeyTree(appName);
            }

            // Get dll location
            var sAssemblyPath = Assembly.GetExecutingAssembly().Location;
            // Register key
            var regAppAddInKey = regAcadAppKey.CreateSubKey(appName);
            regAppAddInKey.SetValue("DESCRIPTION", appName, Microsoft.Win32.RegistryValueKind.String);
            regAppAddInKey.SetValue("LOADCTRLS", 14, Microsoft.Win32.RegistryValueKind.DWord);
            regAppAddInKey.SetValue("LOADER", sAssemblyPath, Microsoft.Win32.RegistryValueKind.String);
            regAppAddInKey.SetValue("MANAGED", 1, Microsoft.Win32.RegistryValueKind.DWord);
            regAcadAppKey.Close();
        }

        public static void Unregister(string appName)
        {
            var regAcadProdKey = GetRootKey();
            var regAcadAppKey = regAcadProdKey.OpenSubKey("Applications", true);
            regAcadAppKey.DeleteSubKeyTree(appName);
            regAcadAppKey.Close();
        }

        public static void BindCommandToDoc(AcadDocument doc)
        {
            doc.CommandWillStart += new CommandEventHandler(CommandWillStart);
            doc.CommandEnded += new CommandEventHandler(CommandEnded);
            doc.CommandCancelled += new CommandEventHandler(CommandCancelled);
            doc.CommandFailed += new CommandEventHandler(CommandFailed);
        }

        public static void UnbindCommandToDoc(AcadDocument doc)
        {
            doc.CommandWillStart -= new CommandEventHandler(CommandWillStart);
            doc.CommandEnded -= new CommandEventHandler(CommandEnded);
            doc.CommandCancelled -= new CommandEventHandler(CommandCancelled);
            doc.CommandFailed -= new CommandEventHandler(CommandFailed);
        }

        private static void CommandFailed(object sender, CommandEventArgs e)
        {
            _config.Switch2CommandIME();
        }

        private static void CommandCancelled(object sender, CommandEventArgs e)
        {
            _config.Switch2CommandIME();
        }

        private static void CommandEnded(object sender, CommandEventArgs e)
        {
            _config.Switch2CommandIME();
        }

        private static void CommandWillStart(object sender, CommandEventArgs e)
        {
            var cmdName = e.GlobalCommandName;
            _config.Switch2IME(cmdName);
        }
    }
}
