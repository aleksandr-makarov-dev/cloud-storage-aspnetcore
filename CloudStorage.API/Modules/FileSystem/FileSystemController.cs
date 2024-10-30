using CloudStorage.API.Modules.FileSystem.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using System.Linq;
using System.Net;
using CloudStorage.API.Modules.FileSystem.Models;
using Minio.DataModel;

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

        [HttpPost("Upload")]
        public async Task<IActionResult> UploadObject([FromQuery] string? prefix, IFormFile? file)
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

        [HttpGet("Download")]
        public async Task<IActionResult> DownloadObject([FromQuery] string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest("Prefix is required");
            }

            var stream = new MemoryStream();

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(key)
                .WithCallbackStream((st) => st.CopyTo(stream));

            var obj = await _minio.GetObjectAsync(getObjectArgs);
            stream.Seek(0, SeekOrigin.Begin);


            if (obj is null)
            {
                return NotFound("File not found");
            }

            return File(stream, obj.ContentType, obj.ObjectName);
        }

        [HttpPut("Move")]
        public async Task<IActionResult> MoveObject([FromBody] MoveObjectRequest moveObjectRequest)
        {
            var copyObjectArgs = new CopyObjectArgs()
                .WithCopyObjectSource(new CopySourceObjectArgs()
                    .WithBucket(_config.BucketName)
                    .WithObject(moveObjectRequest.Key))
                .WithBucket(_config.BucketName)
                .WithObject(moveObjectRequest.NewKey);

            await _minio.CopyObjectAsync(copyObjectArgs);

            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(moveObjectRequest.Key);

            await _minio.RemoveObjectAsync(removeObjectArgs);

            return NoContent();
        }

        [HttpDelete("{key}")]
        public async Task<IActionResult> RemoveObject(string key)
        {
            var decodedKey = WebUtility.UrlDecode(key);

            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(decodedKey);

            await _minio.RemoveObjectAsync(removeObjectArgs);

            return NoContent();
        }
    }
}
