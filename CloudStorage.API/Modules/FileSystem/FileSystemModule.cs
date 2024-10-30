using CloudStorage.API.Modules.FileSystem.Configuration;
using Microsoft.Extensions.Options;
using Minio;

namespace CloudStorage.API.Modules.FileSystem
{
    public static class FileSystemModule
    {
        public static WebApplicationBuilder AddFileSystemModule(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<S3StorageConfiguration>(
                builder.Configuration.GetRequiredSection(S3StorageConfiguration.Name));

            builder.Services.AddSingleton<IMinioClient>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<S3StorageConfiguration>>().Value;

                return new MinioClient()
                    .WithEndpoint(config.Endpoint)
                    .WithCredentials(config.AccessKey, config.SecretKey)
                    .Build();
            });

            return builder;
        }
    }
}
