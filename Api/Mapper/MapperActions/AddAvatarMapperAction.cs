using Api.Models.Attachments;
using Api.Models.User;
using Api.Services;
using AutoMapper;

namespace Api.Mapper.MapperActions
{
    public class AddAvatarMapperAction : IMappingAction<AddAvatarRequest, MetaPathModel>
    {
        private readonly AttachService _attachService;

        public AddAvatarMapperAction(AttachService attachService)
        {
            _attachService = attachService;
        }

        public void Process(AddAvatarRequest source, MetaPathModel destination, ResolutionContext context)
        {
            destination.FilePath = _attachService.MoveAttachFromTemp(source.TempId);
        }
    }
}
