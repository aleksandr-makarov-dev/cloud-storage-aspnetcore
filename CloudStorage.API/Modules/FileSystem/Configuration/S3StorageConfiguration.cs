using CloudStorage.API.Common.Configuration;

namespace CloudStorage.API.Modules.FileSystem.Configuration
{
    public class S3StorageConfiguration:IConfigurationBase
    {
        public string Endpoint { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public static string Name => "S3Storage";
    }
}
