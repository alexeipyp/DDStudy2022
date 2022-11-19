using Api.Models.Attachments;
using Api.Services;
using AutoMapper;

namespace Api.Mapper.MapperActions
{
    public class MetaPathMapperAction : IMappingAction<MetadataModel, MetaPathModel>
    {
        private readonly AttachService _attachService;

        public MetaPathMapperAction(AttachService attachService)
        {
            _attachService = attachService;
        }

        public void Process(MetadataModel source, MetaPathModel destination, ResolutionContext context)
        {
            destination.FilePath = _attachService.MoveAttachFromTemp(source.TempId);
        }
    }
}
