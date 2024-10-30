using CloudStorage.API.Modules.FileSystem.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using System.Linq;

namespace CloudStorage.API.Modules.FileSystem
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class FileSystemController : ControllerBase
    {
        private readonly IMinioClient _minio;
        private readonly S3StorageConfiguration _config;

        public FileSystemController(IMinioClient minio, IOptions<S3StorageConfiguration> config)
        {
            _minio = minio;
            _config = config.Value;
        }

        [HttpGet]
        public async Task<IActionResult> ListObjects([FromQuery] string? prefix)
        {
            var listObjectsArgs = new ListObjectsArgs()
                .WithBucket(_config.BucketName)
                .WithPrefix(prefix);

            var objects = await _minio.ListObjectsEnumAsync(listObjectsArgs).ToListAsync();

            return Ok(objects);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Upload([FromQuery] string? prefix, IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(string.Join("/",prefix,file.FileName))
                .WithObjectSize(file.Length)
                .WithContentType(file.ContentType)
                .WithStreamData(file.OpenReadStream());
            
            // TODO: check if object is uploaded to the bucket
            await _minio.PutObjectAsync(putObjectArgs);

            return Ok(new {message = "File uploaded to the bucket"});
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Download([FromQuery] string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return BadRequest("Prefix is required");
            }

            var stream = new MemoryStream();

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(prefix)
                .WithCallbackStream((st) => st.CopyTo(stream));

            var obj = await _minio.GetObjectAsync(getObjectArgs);
            stream.Seek(0, SeekOrigin.Begin);


            if (obj is null)
            {
                return NotFound("File not found");
            }

            return File(stream, obj.ContentType, obj.ObjectName);
        }
    }
}
