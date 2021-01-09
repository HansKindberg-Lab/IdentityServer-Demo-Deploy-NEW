using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Project.Models;

namespace Project
{
	public static class ConfigurationHelper
	{
		#region Fields

		private const string _azureWebSitesDomain = "azurewebsites.net";
		private const string _identityServerSettingPrefix = "IdentityServer";
		private const string _privateCertificatePathFormat = "/var/ssl/private/{0}.p12";
		private const string _publicCertificatePathFormat = "/var/ssl/certs/{0}.der";
		private const string _separator = "__";

		#endregion

		#region Methods

		public static string ConvertToAzureAppServiceSettings(string appServiceName, string appSettingsJsonPath, string environmentVariablesJsonPath, bool indent = false, string signingCertificateThumbprint = null, string validationCertificateThumbprints = null)
		{
			if(appServiceName == null)
				throw new ArgumentNullException(nameof(appServiceName));

			if(appSettingsJsonPath == null)
				throw new ArgumentNullException(nameof(appSettingsJsonPath));

			if(!File.Exists(appSettingsJsonPath))
				throw new FileNotFoundException($"The app-settings-json-file \"{appSettingsJsonPath}\" does not exist.");

			if(environmentVariablesJsonPath == null)
				throw new ArgumentNullException(nameof(environmentVariablesJsonPath));

			if(!File.Exists(environmentVariablesJsonPath))
				throw new FileNotFoundException($"The environment-variables-json-file \"{environmentVariablesJsonPath}\" does not exist.");

			// ReSharper disable ConvertToUsingDeclaration
			using(var stream = File.OpenRead(environmentVariablesJsonPath))
			{
				var configurationBuilder = new ConfigurationBuilder();
				configurationBuilder.AddJsonStream(stream);
				var configuration = configurationBuilder.Build();

				var settings = new List<AzureAppServiceSetting>
				{
					new AzureAppServiceSetting
					{
						Name = "AppSettings_json",
						Value = File.ReadAllText(appSettingsJsonPath).Replace(Environment.NewLine, string.Empty, StringComparison.Ordinal).Replace("\t", string.Empty, StringComparison.Ordinal)
					}
				};

				PopulateIdentityServerSettings(appServiceName, settings);

				PopulateCertificateSettings(settings, signingCertificateThumbprint, validationCertificateThumbprints);

				PopulateSettings(configuration, settings);

				return JsonSerializer.Serialize(settings, new JsonSerializerOptions
				{
					WriteIndented = indent
				});
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		private static void PopulateCertificateSetting(string settingPrefix, IList<AzureAppServiceSetting> settings, string thumbprint, ISet<string> thumbprints)
		{
			if(settingPrefix == null)
				throw new ArgumentNullException(nameof(settingPrefix));

			if(settings == null)
				throw new ArgumentNullException(nameof(settings));

			if(thumbprint == null)
				throw new ArgumentNullException(nameof(thumbprint));

			if(thumbprints == null)
				throw new ArgumentNullException(nameof(thumbprints));

			thumbprints.Add(thumbprint);

			settings.Add(new AzureAppServiceSetting
			{
				Name = $"{settingPrefix}{_separator}Options{_separator}Path",
				Value = string.Format(CultureInfo.InvariantCulture, _privateCertificatePathFormat, thumbprint)
			});

			settings.Add(new AzureAppServiceSetting
			{
				Name = $"{settingPrefix}{_separator}Type",
				Value = "RegionOrebroLan.Security.Cryptography.Configuration.FileResolverOptions, RegionOrebroLan"
			});
		}

		private static void PopulateCertificateSettings(IList<AzureAppServiceSetting> settings, string signingCertificateThumbprint, string validationCertificateThumbprints)
		{
			if(settings == null)
				throw new ArgumentNullException(nameof(settings));

			var thumbprints = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			if(signingCertificateThumbprint != null)
				PopulateCertificateSetting($"{_identityServerSettingPrefix}{_separator}SigningCertificate", settings, signingCertificateThumbprint, thumbprints);

			var validationCertificateThumbprintsArray = (validationCertificateThumbprints ?? string.Empty).Split(',').Select(item => item.Trim()).Where(item => !string.IsNullOrEmpty(item)).ToArray();

			for(var i = 0; i < validationCertificateThumbprintsArray.Length; i++)
			{
				var validationCertificateThumbprint = validationCertificateThumbprintsArray[i];

				PopulateCertificateSetting($"{_identityServerSettingPrefix}{_separator}ValidationCertificates{_separator}{i}", settings, validationCertificateThumbprint, thumbprints);
			}

			if(thumbprints.Any())
			{
				settings.Add(new AzureAppServiceSetting
				{
					Name = "WEBSITE_LOAD_CERTIFICATES",
					Value = string.Join(",", thumbprints)
				});
			}
		}

		private static void PopulateIdentityServerSettings(string appServiceName, IList<AzureAppServiceSetting> settings)
		{
			if(appServiceName == null)
				throw new ArgumentNullException(nameof(appServiceName));

			if(settings == null)
				throw new ArgumentNullException(nameof(settings));

			settings.Add(new AzureAppServiceSetting
			{
				Name = $"{_identityServerSettingPrefix}{_separator}InteractiveMutualTlsDomain",
				Value = $"{appServiceName}-interactive-mtls.{_azureWebSitesDomain}"
			});

			settings.Add(new AzureAppServiceSetting
			{
				Name = $"{_identityServerSettingPrefix}{_separator}IssuerUri",
				Value = $"https://{appServiceName}.{_azureWebSitesDomain}"
			});

			settings.Add(new AzureAppServiceSetting
			{
				Name = $"{_identityServerSettingPrefix}{_separator}MutualTls{_separator}DomainName",
				Value = $"{appServiceName}-mtls.{_azureWebSitesDomain}"
			});

			settings.Add(new AzureAppServiceSetting
			{
				Name = $"{_identityServerSettingPrefix}{_separator}MutualTls{_separator}Enabled",
				Value = "true"
			});
		}

		private static void PopulateSettings(IConfiguration configuration, IList<AzureAppServiceSetting> settings)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			if(settings == null)
				throw new ArgumentNullException(nameof(settings));

			// ReSharper disable MergeSequentialPatterns
			if(configuration is IConfigurationSection configurationSection && configurationSection.Value != null)
			{
				settings.Add(new AzureAppServiceSetting
				{
					Name = configurationSection.Path.Replace(":", _separator, StringComparison.Ordinal),
					Value = ResolveValue(configurationSection.Value)
				});
			}
			else
			{
				foreach(var child in configuration.GetChildren())
				{
					PopulateSettings(child, settings);
				}
			}
			// ReSharper restore MergeSequentialPatterns
		}

		private static string ResolveValue(string value)
		{
			const StringComparison stringComparison = StringComparison.OrdinalIgnoreCase;

			// Boolean values need to bee lower-case in Azure App Settings, "True" / "False" does not work.
			if(value != null && (value.Equals(bool.TrueString, stringComparison) || value.Equals(bool.FalseString, stringComparison)))
				return value.ToLowerInvariant();

			return value;
		}

		#endregion
	}
}