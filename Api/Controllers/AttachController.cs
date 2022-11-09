using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Common.CustomExceptions;
using Api.Services;
using Api.Models.Attachments;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AttachController : ControllerBase
    {
        private readonly AttachService _attachService;

        public AttachController(AttachService attachService)
        {
            _attachService = attachService;
        }

        [HttpPost]
        [Authorize]
        public async Task<List<MetadataModel>> UploadFiles([FromForm] List<IFormFile> files) => await _attachService.UploadFiles(files);    
               
    }
}
