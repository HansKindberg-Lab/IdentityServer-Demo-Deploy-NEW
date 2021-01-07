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
			var environmentVariablesJsonFilePath = Path.Combine(projectDirectory.Parent.FullName, "EnvironmentVariables.json");
			// ReSharper restore PossibleNullReferenceException

			var azureAppServiceSettings = ConfigurationHelper.ConvertToAzureAppServiceSettings("my-identityserver-demo", appSettingsJsonFilePath, environmentVariablesJsonFilePath, true, "Signing-certificate-thumbprint", "Validation-certificate-thumbprint-1,Validation-certificate-thumbprint-2            , Validation-certificate-thumbprint-3     ,  , ,");
			Assert.AreEqual(2282, azureAppServiceSettings.Length);
		}

		[TestMethod]
		public void ConvertToAzureAppServiceSettings_Test()
		{
			// ReSharper disable PossibleNullReferenceException
			var projectDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent;
			var appSettingsJsonFilePath = Path.Combine(projectDirectory.Parent.FullName, "AppSettings.json");
			var environmentVariablesJsonFilePath = Path.Combine(projectDirectory.Parent.FullName, "EnvironmentVariables.json");
			// ReSharper restore PossibleNullReferenceException

			var azureAppServiceSettings = ConfigurationHelper.ConvertToAzureAppServiceSettings("my-identityserver-demo", appSettingsJsonFilePath, environmentVariablesJsonFilePath, true);
			Assert.AreEqual(12984, azureAppServiceSettings.Length);
		}

		#endregion
	}
}