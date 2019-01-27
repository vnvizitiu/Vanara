﻿using Microsoft.Win32;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Vanara.Windows.Shell
{
	/// <summary>A class to register COM objects.</summary>
	public static class ComRegistrar
	{
		/// <summary>Registers the specified type as a COM Local Server.</summary>
		/// <typeparam name="TComObject">The type of the COM object.</typeparam>
		/// <param name="pszFriendlyName">The friendly name of the COM object.</param>
		/// <param name="cmdLineArgs">The command line arguments to supply to the executable on startup.</param>
		/// <param name="assembly">The assembly used to get the full path of the executable. If this value is <see langword="null"/>, then the assembly of <typeparamref name="TComObject"/> will be used.</param>
		/// <param name="systemWide">If set to <see langword="true" />, the COM object is registered system-wide in HKLM; otherwise it is registered for the user only in HKCU.</param>
		/// <param name="appId">The AppId to relate to this CLSID. If <see langword="null"/>, the CLSID value will be used.</param>
		public static void RegisterLocalServer<TComObject>(string pszFriendlyName, string cmdLineArgs = null, Assembly assembly = null, bool systemWide = false, Guid? appId = null) where TComObject : ComObject
		{
			var cmdLine = (assembly ?? typeof(TComObject).Assembly).Location;
			var clsid = GetClsid<TComObject>();
			var _appId = appId ?? clsid;
			if (!(cmdLineArgs is null)) cmdLine += " " + cmdLineArgs;

			using (var root = (systemWide ? Registry.LocalMachine : Registry.CurrentUser).CreateSubKey(@"Software\Classes"))
			using (root.CreateSubKey(@"AppID\" + _appId.GetString(), pszFriendlyName))
			using (var rClsid = root.CreateSubKey(@"CLSID\" + clsid.GetString(), pszFriendlyName))
			using (rClsid.CreateSubKey("LocalServer32", cmdLine))
			{
				rClsid.SetValue("AppId", _appId.GetString());
			}
		}

		/// <summary>Unregisters the COM Local Server.</summary>
		/// <typeparam name="TComObject">The type of the COM object.</typeparam>
		/// <param name="systemWide">If set to <see langword="true" />, the COM object is unregistered system-wide from HKLM; otherwise it is unregistered for the user only in HKCU.</param>
		/// <param name="appId">The AppId to relate to this CLSID. If <see langword="null"/>, the CLSID value will be used.</param>
		public static void UnregisterLocalServer<TComObject>(bool systemWide = false, Guid? appId = null) where TComObject : ComObject
		{
			var clsid = GetClsid<TComObject>();
			using (var root = (systemWide ? Registry.LocalMachine : Registry.CurrentUser).CreateSubKey(@"Software\Classes"))
			{
				root.DeleteSubKeyTree(@"AppID\" + (appId ?? clsid).GetString());
				root.DeleteSubKeyTree(@"CLSID\" + clsid.GetString());
			}
		}

		private static Guid GetClsid<TComObject>() => Marshal.GenerateGuidForType(typeof(TComObject));

		private static string GetString(this Guid g) => g.ToString("B").ToUpperInvariant();
	}
}