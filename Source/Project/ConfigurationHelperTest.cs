using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Project
{
	[TestClass]
	public class ConfigurationHelperTest
	{
		#region Methods

		[TestMethod]
		public void ConvertToAzureAppServiceSettings_Test()
		{
			// ReSharper disable PossibleNullReferenceException
			var projectDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent;
			var appSettingsJsonFilePath = Path.Combine(projectDirectory.Parent.FullName, "AppSettings.json");
			// ReSharper restore PossibleNullReferenceException

			var azureAppServiceSettings = ConfigurationHelper.ConvertToAzureAppServiceSettings(appSettingsJsonFilePath);

			Assert.AreEqual(37814, azureAppServiceSettings.Length);
		}

		#endregion
	}
}