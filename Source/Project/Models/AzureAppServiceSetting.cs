using System.Text.Json.Serialization;

namespace Project.Models
{
	public class AzureAppServiceSetting
	{
		#region Properties

		[JsonPropertyName("name")]
		public virtual string Name { get; set; }

		[JsonPropertyName("slotSetting")]
		public virtual bool SlotSetting { get; set; }

		[JsonPropertyName("value")]
		public virtual string Value { get; set; }

		#endregion
	}
}