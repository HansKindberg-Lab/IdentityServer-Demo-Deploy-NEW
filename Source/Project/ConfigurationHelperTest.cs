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

			var azureAppServiceSettings = ConfigurationHelper.ConvertToAzureAppServiceSettings("my-identityserver-demo", appSettingsJsonFilePath, true, "Signing-certificate-thumbprint", "Validation-certificate-thumbprint-1,Validation-certificate-thumbprint-2            , Validation-certificate-thumbprint-3     ,  , ,");
			Assert.AreEqual(2192, azureAppServiceSettings.Length);
		}

		[TestMethod]
		public void ConvertToAzureAppServiceSettings_Test()
		{
			// ReSharper disable PossibleNullReferenceException
			var projectDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent;
			var appSettingsJsonFilePath = Path.Combine(projectDirectory.Parent.FullName, "AppSettings.json");
			// ReSharper restore PossibleNullReferenceException

			var azureAppServiceSettings = ConfigurationHelper.ConvertToAzureAppServiceSettings("my-identityserver-demo", appSettingsJsonFilePath, true);
			Assert.AreEqual(32581, azureAppServiceSettings.Length);
		}

		#endregion
	}
}