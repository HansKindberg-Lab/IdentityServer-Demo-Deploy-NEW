using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Project.Models;

namespace Project
{
	public static class ConfigurationHelper
	{
		#region Methods

		public static string ConvertToAzureAppServiceSettings(string appSettingsJsonPath, bool indent = false)
		{
			if(appSettingsJsonPath == null)
				throw new ArgumentNullException(nameof(appSettingsJsonPath));

			if(!File.Exists(appSettingsJsonPath))
				throw new FileNotFoundException($"The app-settings-json-file \"{appSettingsJsonPath}\" does not exist.");

			// ReSharper disable ConvertToUsingDeclaration
			using(var stream = File.OpenRead(appSettingsJsonPath))
			{
				var configurationBuilder = new ConfigurationBuilder();
				configurationBuilder.AddJsonStream(stream);
				var configuration = configurationBuilder.Build();
				var settings = new List<AzureAppServiceSetting>();
				PopulateSettings(configuration, settings);

				return JsonSerializer.Serialize(settings, new JsonSerializerOptions
				{
					WriteIndented = indent
				});
			}
			// ReSharper restore ConvertToUsingDeclaration
		}

		private static void PopulateSettings(IConfiguration configuration, IList<AzureAppServiceSetting> settings)
		{
			if(configuration == null)
				throw new ArgumentNullException(nameof(configuration));

			if(settings == null)
				throw new ArgumentNullException(nameof(settings));

			if(configuration is IConfigurationSection configurationSection && configurationSection.Value != null)
			{
				settings.Add(new AzureAppServiceSetting
				{
					Name = configurationSection.Path.Replace(":", "__", StringComparison.Ordinal),
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
		}

		[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
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