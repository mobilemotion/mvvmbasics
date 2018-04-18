using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace TemplatesInstaller
{
	[RunInstaller(true)]
	public partial class Installer : System.Configuration.Install.Installer
	{
		public Installer()
		{
			InitializeComponent();
		}

		public override void Install(IDictionary stateSaver)
		{
			base.Install(stateSaver);

			var templatesFolder = GetTemplatesFolder();
			var sourceFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var sourceFile = Path.Combine(sourceFolder, "Templates.zip");
			var targetFile = Path.Combine(templatesFolder, "MVVMbasicsProjectTemplates.zip");

			if (File.Exists(targetFile))
				File.Delete(targetFile);
			File.Copy(sourceFile, targetFile);
		}

		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall(savedState);

			var templatesFolder = GetTemplatesFolder();
			var targetFile = Path.Combine(templatesFolder, "MVVMbasicsProjectTemplates.zip");

			File.Delete(targetFile);
		}

		private string GetTemplatesFolder()
		{
			string documentsFolder = @"%userprofile%\documents";
			try
			{
				documentsFolder = Registry.GetValue(
					@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", "Personal",
					null).ToString();
			}
			catch (Exception) { /* Fallback already defined as default above */ }

			var templatesFolder = $@"{documentsFolder}\Visual Studio 2017\Templates\ProjectTemplates\Visual C#\";

			if (!Directory.Exists(templatesFolder))
				throw new Exception("The Visual Studio project template folder could not be found.\r\nMake sure Visual Studio 2017 is installed for the current user!");

			return templatesFolder;
		}
	}
}
