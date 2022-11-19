using Api.Models.Attachments;
using Api.Services;
using AutoMapper;
using DAL.Entities;

namespace Api.Mapper.MapperActions
{
    public class PostAttachMapperAction : IMappingAction<PostAttach, AttachWithLinkModel>
    {
        private LinkGeneratorService _links;
        public PostAttachMapperAction(LinkGeneratorService linkGeneratorService)
        {
            _links = linkGeneratorService;
        }
        public void Process(PostAttach source, AttachWithLinkModel destination, ResolutionContext context)
        {
            _links.FixAttach(source, destination);
        }
        
    }
}
