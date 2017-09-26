namespace Wirehome.Contracts.Api.Cloud
{
    public class CloudResponseMessage
    {
        public CloudMessageHeader Header { get; } = new CloudMessageHeader();

        public ApiResponse Response { get; } = new ApiResponse();
    }
}
