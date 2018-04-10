namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class OperatingSystem
    {
        public int OperatingSystemId { get; set; }
        public string OperatingSystemName { get; set; }
        public Version1 OperatingSystemVersion { get; set; }
        public Version1 OperatingSystemVersionMinor { get; set; }
    }
}
