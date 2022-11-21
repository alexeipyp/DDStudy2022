using Api.Models.Attachments;
using Common.CustomExceptions;
using Common.CustomExceptions.BadRequestExceptions;
using Common.CustomExceptions.ForbiddenExceptions;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Xml.Linq;

namespace Api.Services
{
    public class AttachService
    {
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

        public string MoveAttachFromTemp(Guid tempId)
        {
            var tempFileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), tempId.ToString()));
            if (!tempFileInfo.Exists)
            {
                throw new FileNotFoundException("file not found");
            }
            else
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "attaches", tempId.ToString());
                var destFileInfo = new FileInfo(path);
                if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                    destFileInfo.Directory.Create();

                System.IO.File.Copy(tempFileInfo.FullName, path, true);
                tempFileInfo.Delete();
                return path;
            }
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
                Size = file.Length,
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
