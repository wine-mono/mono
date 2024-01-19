//
// CustomizableFileSettingsProvider.cs
//
// Authors:
//	Noriaki Okimoto  <seara@ojk.sppd.ne.jp>
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2007 Noriaki Okimoto
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if CONFIGURATION_DEP

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace System.Configuration
{
	internal class CustomizableFileSettingsProvider : SettingsProvider, IApplicationSettingsProvider
	{
		// KLUDGE WARNING.
		//
		// This is used from within System.Web to allow mapping of the ExeConfigFilename to
		// the correct Web.config for the current request. Otherwise web applications will
		// not be able to access settings from Web.config. The type assigned to this
		// variable must descend from the ConfigurationFileMap class and its
		// MachineConfigFilename will be used to set the ExeConfigFilename.
		//
		// This is necessary to fix bug #491531
#pragma warning disable 649
		private static Type webConfigurationFileMapType;
#pragma warning restore 649
		
		private static string userRoamingPath = "";
		private static string userLocalPath = "";
		private static string userRoamingPathPrevVersion = "";
		private static string userLocalPathPrevVersion = "";
		private static string userRoamingName = "user.config";
		private static string userLocalName = "user.config";

		public override void Initialize (string name, NameValueCollection config)
		{
			base.Initialize (name, config);
		}

		// full path to roaming user.config
		internal static string UserRoamingFullPath {
			get { return Path.Combine (userRoamingPath, userRoamingName); }
		}

		// full path to local user.config
		internal static string UserLocalFullPath {
			get { return Path.Combine (userLocalPath, userLocalName); }
		}

		// previous full path to roaming user.config
		public static string PrevUserRoamingFullPath {
			get { return Path.Combine (userRoamingPathPrevVersion, userRoamingName); }
		}

		// previous full path to local user.config
		public static string PrevUserLocalFullPath {
			get { return Path.Combine (userLocalPathPrevVersion, userLocalName); }
		}

		private static void CreateUserConfigPath ()
		{
			/* Get one directory above the version directory. */
			userRoamingPath = Path.GetDirectoryName(ClientConfigPaths.Current.RoamingConfigDirectory);
			userLocalPath = Path.GetDirectoryName(ClientConfigPaths.Current.LocalConfigDirectory);

			string versionName = ClientConfigPaths.Current.ProductVersion;
			string prevVersionRoaming = PrevVersionPath (userRoamingPath, versionName);
			string prevVersionLocal = PrevVersionPath (userLocalPath, versionName);
			
			userRoamingPath = Path.Combine (userRoamingPath, versionName);
			userLocalPath = Path.Combine (userLocalPath, versionName);
			if (prevVersionRoaming != "")
				userRoamingPathPrevVersion = Path.Combine(userRoamingPath, prevVersionRoaming);
			if (prevVersionLocal != "")
				userLocalPathPrevVersion = Path.Combine(userLocalPath, prevVersionLocal);
		}

		// string for the previous version. It ignores newer ones.
		private static string PrevVersionPath (string dirName, string currentVersion)
		{
			string prevVersionString = "";

			if (!Directory.Exists(dirName))
				return prevVersionString;
			DirectoryInfo currentDir = new DirectoryInfo (dirName);
			foreach (DirectoryInfo dirInfo in currentDir.GetDirectories ())
				if (String.Compare (currentVersion, dirInfo.Name, StringComparison.Ordinal) > 0)
					if (String.Compare (prevVersionString, dirInfo.Name, StringComparison.Ordinal) < 0)
						prevVersionString = dirInfo.Name;

			return prevVersionString;
		}

		public override string Name {
			get { return base.Name; }
		}

		string app_name = String.Empty;//"OJK.CustomSetting.CustomizableLocalFileSettingsProvider";
		public override string ApplicationName {
			get { return app_name; }
			set { app_name = value; }
		}

		private ExeConfigurationFileMap exeMapCurrent = null;
		private ExeConfigurationFileMap exeMapPrev = null;
		private SettingsPropertyValueCollection values = null;

		/// <remarks>
		/// Hack to remove the XmlDeclaration that the XmlSerializer adds.
		/// <br />
		/// see <a href="https://github.com/mono/mono/pull/2273">Issue 2273</a> for details
		/// </remarks>
		private string StripXmlHeader (string serializedValue)
		{
			if (serializedValue == null)
			{
				return string.Empty;
			}

			XmlDocument doc = new XmlDocument ();
			XmlElement valueXml = doc.CreateElement ("value");
			valueXml.InnerXml = serializedValue;

			foreach (XmlNode child in valueXml.ChildNodes) {
				if (child.NodeType == XmlNodeType.XmlDeclaration) {
					valueXml.RemoveChild (child);
					break;
				}
			}

			// InnerXml will give you well-formed XML that you could save as a separate document, and 
			// InnerText will immediately give you a pure-text representation of this inner XML.
			return valueXml.InnerXml;
		}

		private void SaveProperties (ExeConfigurationFileMap exeMap, SettingsPropertyValueCollection collection, ConfigurationUserLevel level, SettingsContext context, bool checkUserLevel)
		{
			Configuration config = ConfigurationManager.OpenMappedExeConfiguration (exeMap, level);
			
			UserSettingsGroup userGroup = config.GetSectionGroup ("userSettings") as UserSettingsGroup;
			bool isRoaming = (level == ConfigurationUserLevel.PerUserRoaming);

			if (userGroup == null) {
				userGroup = new UserSettingsGroup ();
				config.SectionGroups.Add ("userSettings", userGroup);
			}
			ApplicationSettingsBase asb = context.CurrentSettings;
			string class_name = NormalizeInvalidXmlChars ((asb != null ? asb.GetType () : typeof (ApplicationSettingsBase)).FullName);
			ClientSettingsSection userSection = null;
			ConfigurationSection cnf = userGroup.Sections.Get (class_name);
			userSection = cnf as ClientSettingsSection;
			if (userSection == null) {
				userSection = new ClientSettingsSection ();
				userGroup.Sections.Add (class_name, userSection);
			}

			bool hasChanges = false;

			if (userSection == null)
				return;

			foreach (SettingsPropertyValue value in collection) {
				if (checkUserLevel && value.Property.Attributes.Contains (typeof (SettingsManageabilityAttribute)) != isRoaming)
					continue;
				// The default impl does not save the ApplicationScopedSetting properties
				if (value.Property.Attributes.Contains (typeof (ApplicationScopedSettingAttribute)))
					continue;
				/* Unless the value has been changed, don't attempt to save it. */
				if (!value.IsDirty)
					continue;

				hasChanges = true;
				SettingElement element = userSection.Settings.Get (value.Name);
				if (element == null) {
					element = new SettingElement (value.Name, value.Property.SerializeAs);
					userSection.Settings.Add (element);
				}
				if (element.Value.ValueXml == null)
					element.Value.ValueXml = new XmlDocument ().CreateElement ("value");
				switch (value.Property.SerializeAs) {
				case SettingsSerializeAs.Xml:
					element.Value.ValueXml.InnerXml = StripXmlHeader (value.SerializedValue as string);
					break;
				case SettingsSerializeAs.String:
					element.Value.ValueXml.InnerText = value.SerializedValue as string;
					break;
				case SettingsSerializeAs.Binary:
					element.Value.ValueXml.InnerText = value.SerializedValue != null ? Convert.ToBase64String (value.SerializedValue as byte []) : string.Empty;
					break;
				default:
					throw new NotImplementedException ();
				}
			}
			if (hasChanges)
				config.Save (ConfigurationSaveMode.Minimal, true);
		}

		// NOTE: We should add here all the chars that are valid in a name of a class (Ecma-wise),
		// but invalid in an xml element name, and provide a better impl if we get too many of them.
		string NormalizeInvalidXmlChars (string str)
		{
			char [] invalid_chars = new char [] { '+' };

			if (str == null || str.IndexOfAny (invalid_chars) == -1)
				return str;

			// Replace with its hexadecimal values.
			str = str.Replace ("+", "_x002B_");
			return str;
		}

		private void LoadPropertyValue (SettingsPropertyCollection collection, SettingElement element, bool allowOverwrite)
		{
			SettingsProperty prop = collection [element.Name];
			if (prop == null) { // see bug #343459
				prop = new SettingsProperty (element.Name);
				collection.Add (prop);
			}

			SettingsPropertyValue value = new SettingsPropertyValue (prop);
			value.IsDirty = false;
			if (element.Value.ValueXml != null) {
				switch (value.Property.SerializeAs) {
				case SettingsSerializeAs.Xml:
					value.SerializedValue = element.Value.ValueXml.InnerXml;
					break;
				case SettingsSerializeAs.String:
					value.SerializedValue = element.Value.ValueXml.InnerText.Trim ();
					break;
				case SettingsSerializeAs.Binary:
					value.SerializedValue = Convert.FromBase64String (element.Value.ValueXml.InnerText);
					break;
				}
			}
			else
				value.SerializedValue = prop.DefaultValue;
			try
			{
				if (allowOverwrite)
					values.Remove (element.Name);
				values.Add (value);
			} catch (ArgumentException ex) {
				throw new ConfigurationErrorsException (string.Format (
					CultureInfo.InvariantCulture,
					"Failed to load value for '{0}'.",
					element.Name), ex);
			}
		}

		private void LoadProperties (ExeConfigurationFileMap exeMap, SettingsPropertyCollection collection, ConfigurationUserLevel level, string sectionGroupName, bool allowOverwrite, string groupName)
		{
			Configuration config = ConfigurationManager.OpenMappedExeConfiguration (exeMap,level);
			
			ConfigurationSectionGroup sectionGroup = config.GetSectionGroup (sectionGroupName);
			if (sectionGroup != null) {
				foreach (ConfigurationSection configSection in sectionGroup.Sections) {
					if (configSection.SectionInformation.Name != groupName)
						continue;

					ClientSettingsSection clientSection = configSection as ClientSettingsSection;
					if (clientSection == null)
						continue;

					foreach (SettingElement element in clientSection.Settings) {
						LoadPropertyValue(collection, element, allowOverwrite);
					}
					// Only the first one seems to be processed by MS
					break;
				}
			}

		}

		public override void SetPropertyValues (SettingsContext context, SettingsPropertyValueCollection collection)
		{
			CreateExeMap ();

			if (UserLocalFullPath == UserRoamingFullPath)
			{
				SaveProperties (exeMapCurrent, collection, ConfigurationUserLevel.PerUserRoaming, context, false);
			} else {
				SaveProperties (exeMapCurrent, collection, ConfigurationUserLevel.PerUserRoaming, context, true);
				SaveProperties (exeMapCurrent, collection, ConfigurationUserLevel.PerUserRoamingAndLocal, context, true);
			}
		}

		public override SettingsPropertyValueCollection GetPropertyValues (SettingsContext context, SettingsPropertyCollection collection)
		{
			CreateExeMap ();

			values = new SettingsPropertyValueCollection ();
			string groupName = context ["GroupName"] as string;
			groupName = NormalizeInvalidXmlChars (groupName); // we likely saved the element removing the non valid xml chars.
			LoadProperties (exeMapCurrent, collection, ConfigurationUserLevel.None, "applicationSettings", false, groupName);
			LoadProperties (exeMapCurrent, collection, ConfigurationUserLevel.None, "userSettings", false, groupName);

			LoadProperties (exeMapCurrent, collection, ConfigurationUserLevel.PerUserRoaming, "userSettings", true, groupName);
			LoadProperties (exeMapCurrent, collection, ConfigurationUserLevel.PerUserRoamingAndLocal, "userSettings", true, groupName);

			// create default values if not exist
			foreach (SettingsProperty p in collection)
				if (values [p.Name] == null)
					values.Add (new SettingsPropertyValue (p));
			return values;
		}

		/// creates an ExeConfigurationFileMap
		private void CreateExeMap ()
		{
			if (exeMapCurrent == null) {
				CreateUserConfigPath ();

				// current version
				exeMapCurrent = new ExeConfigurationFileMap ();
				
				// exeMapCurrent.ExeConfigFilename = System.Windows.Forms.Application.ExecutablePath + ".config";
				Assembly entry = Assembly.GetEntryAssembly () ?? Assembly.GetExecutingAssembly ();
				exeMapCurrent.ExeConfigFilename = entry.Location + ".config";
				exeMapCurrent.LocalUserConfigFilename = UserLocalFullPath;
				exeMapCurrent.RoamingUserConfigFilename = UserRoamingFullPath;

				if (webConfigurationFileMapType != null && typeof (ConfigurationFileMap).IsAssignableFrom (webConfigurationFileMapType)) {
					try {
						ConfigurationFileMap cfgFileMap = Activator.CreateInstance (webConfigurationFileMapType) as ConfigurationFileMap;
						if (cfgFileMap != null) {
							string fpath = cfgFileMap.MachineConfigFilename;
							if (!String.IsNullOrEmpty (fpath))
								exeMapCurrent.ExeConfigFilename = fpath;
						}
					} catch {
						// ignore
					}
				}
				
				// previous version
				if ((PrevUserLocalFullPath != "") && (PrevUserRoamingFullPath != ""))
				{
					exeMapPrev = new ExeConfigurationFileMap();
 					// exeMapPrev.ExeConfigFilename = System.Windows.Forms.Application.ExecutablePath + ".config";
					exeMapPrev.ExeConfigFilename = entry.Location + ".config";
					exeMapPrev.LocalUserConfigFilename = PrevUserLocalFullPath;
					exeMapPrev.RoamingUserConfigFilename = PrevUserRoamingFullPath;
				}
			}
		}

		// FIXME: implement
		public SettingsPropertyValue GetPreviousVersion (SettingsContext context, SettingsProperty property)
		{
			return null;
		}

		public void Reset (SettingsContext context)
		{
			if (values == null) {
				SettingsPropertyCollection coll = new SettingsPropertyCollection ();
				GetPropertyValues (context, coll);
			}

			if (values != null) {
				foreach (SettingsPropertyValue propertyValue in values) {
					// Can't use propertyValue.Property.DefaultValue
					// as it may cause InvalidCastException (see bug# 532180)
					values[propertyValue.Name].PropertyValue = propertyValue.Reset ();
				}
			}
		}

		// FIXME: implement
		public void Upgrade (SettingsContext context, SettingsPropertyCollection properties)
		{
		}

		public static void setCreate ()
		{
			CreateUserConfigPath();
		}
	}
}

#endif
