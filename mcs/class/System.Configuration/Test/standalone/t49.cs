using System;
using System.Reflection;
using System.Configuration;
using System.IO;
using System.Xml;

[assembly:AssemblyProductAttribute("Product-Name.")]

namespace TestUserConfig.Path
{
	class Program
	{
		const string expCompanyDirName = "TestUserConfig"; /* Company directory stops at the first period in the entry point's namespace. */
		const string expVersionDirName = "0.0.0.0"; /* Without an assembly attribute, defaults to 0.0.0.0 */
		const string expProductDirNamePart1 = "t49.exe"; /* Taken from AppDomain.CurrentDomain.FriendlyName. Product name attribute is ignored. */
		const string expProductDirNamePart2 = "Url";

		public static int Main (string[] args)
		{
			DirectoryInfo appDataCompanyDir = null;
			DirectoryInfo appDataProductDir = null;
			DirectoryInfo appDataVersionDir = null;

			try
			{
				Settings.Default.SettingBool1 = false;
				Settings.Default.Save();

				/* Get company directory. */
				appDataCompanyDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/" + expCompanyDirName);
				if (!appDataCompanyDir.Exists) {
					Console.WriteLine("Failed to find company name directory");
					return 1;
				}

				/* Get product directory. */
				DirectoryInfo[] appDataProductDirs = appDataCompanyDir.GetDirectories(expProductDirNamePart1 + "_" + expProductDirNamePart2 + "*");
				foreach (DirectoryInfo dir in appDataProductDirs) {
					appDataProductDir = dir;
					break;
				}

				if (appDataProductDir == null || !appDataProductDir.Exists) {
					Console.WriteLine("Failed to find product name directory");
					return 1;
				}

				/* Finally, get version directory. */
				appDataVersionDir = new DirectoryInfo(appDataProductDir.FullName + "/" + expVersionDirName);
				if (!appDataVersionDir.Exists) {
					Console.WriteLine("Failed to find version directory");
					return 1;
				}

				Console.WriteLine("Tests successful!");
				return 0;
			}
			catch (Exception e)
			{
				// Error.
				Console.WriteLine(e.ToString());
				return 1;
			}
			finally
			{
				if (appDataCompanyDir != null && appDataCompanyDir.Exists)
					appDataCompanyDir.Delete(true);
			}
		}

		class Settings : ApplicationSettingsBase
		{
			private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

			public static Settings Default => defaultInstance;

			[UserScopedSetting]
			public bool SettingBool1
			{
				get
				{
					return (bool)this["SettingBool1"];
				}
				set
				{
					this["SettingBool1"] = value;
				}
			}
		}
	}
}
