namespace Wirehome.Extensions.MessagesModel
{

    public class Discoveredappliance
    {
        public string[] actions { get; set; }
        public Additionalappliancedetails additionalApplianceDetails { get; set; }
        public string applianceId { get; set; }
        public string friendlyDescription { get; set; }
        public string friendlyName { get; set; }
        public bool isReachable { get; set; }
        public string manufacturerName { get; set; }
        public string modelName { get; set; }
        public string version { get; set; }
    }

}
