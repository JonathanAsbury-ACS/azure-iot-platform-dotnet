
namespace Mmm.Platform.IoT.TenantManager.Services.Models
{
    public class StreamAnalyticsJobModel
    {
        public string TenantId { get; set; }
        public string StreamAnalyticsJobName { get; set; }
        public string JobState { get; set; }
        public bool IsActive { get; set; }
    }
}
