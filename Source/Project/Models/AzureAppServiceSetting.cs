namespace Project.Models
{
	public class AzureAppServiceSetting
	{
		#region Properties

		public virtual string Name { get; set; }
		public virtual bool SlotSetting { get; set; }
		public virtual string Value { get; set; }

		#endregion
	}
}