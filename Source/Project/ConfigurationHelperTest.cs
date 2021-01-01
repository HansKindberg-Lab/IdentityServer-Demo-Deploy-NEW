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
		public void ConvertToAzureAppServiceSettings_Certificate_Test()
		{
			// ReSharper disable PossibleNullReferenceException
			var projectDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent;
			var appSettingsJsonFilePath = Path.Combine(projectDirectory.Parent.FullName, "AppSettings.json");
			// ReSharper restore PossibleNullReferenceException

			var azureAppServiceSettings = ConfigurationHelper.ConvertToAzureAppServiceSettings(appSettingsJsonFilePath, true, "Signing-certificate-thumbprint", new[] {"Validation-certificate-thumbprint-1", "Validation-certificate-thumbprint-2", "Validation-certificate-thumbprint-3"});
			Assert.AreEqual(1799, azureAppServiceSettings.Length);
		}

		[TestMethod]
		public void ConvertToAzureAppServiceSettings_Test()
		{
			// ReSharper disable PossibleNullReferenceException
			var projectDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent;
			var appSettingsJsonFilePath = Path.Combine(projectDirectory.Parent.FullName, "AppSettings.json");
			// ReSharper restore PossibleNullReferenceException

			var azureAppServiceSettings = ConfigurationHelper.ConvertToAzureAppServiceSettings(appSettingsJsonFilePath, true);
			Assert.AreEqual(33905, azureAppServiceSettings.Length);
		}

		#endregion
	}
}