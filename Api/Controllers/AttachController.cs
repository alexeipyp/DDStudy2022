using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Common.CustomExceptions;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AttachController : ControllerBase
    {
        [HttpPost]
        [Authorize]
        public async Task<List<MetadataModel>> UploadFiles([FromForm] List<IFormFile> files)
        {
            if (files.Count == 0)
            {
                throw new FileIsNullException("uploaded a null value");
            }
            var res = new List<MetadataModel>();
            foreach (var file in files)
            {
                res.Add(await UploadFile(file));
            }
            return res;
        }

        private async Task<MetadataModel> UploadFile(IFormFile file)
        {
            if (file.Length == 0)
            {
                throw new FileIsNullException("uploaded a null value");
            }
            var tempPath = Path.GetTempPath();
            var meta = new MetadataModel
            {
                TempId = Guid.NewGuid(),
                Name = file.FileName,
                MimeType = file.ContentType,
                Size = file.Length
            };

            var newPath = Path.Combine(tempPath, meta.TempId.ToString());

            var fileinfo = new FileInfo(newPath);
            if (fileinfo.Exists)
            {
                throw new FileAlreadyExistsException("file exists");                
            }
            else
            {
                if (fileinfo.Directory == null)
                {
                    throw new DirectoryNotFoundException("temp is null");
                }

                if (!fileinfo.Directory.Exists)
                {
                    fileinfo.Directory?.Create();
                }

                using (var stream = System.IO.File.Create(newPath))
                {
                    await file.CopyToAsync(stream);
                }

                return meta;
            }
            
            

        }
    }
}
