using Api.Models.Post;
using AutoMapper;

namespace Api.Mapper.MapperActions
{
    public class CreatePostMapperAction : IMappingAction<CreatePostAuthorizedRequest, CreatePostModel>
    {
        public void Process(CreatePostAuthorizedRequest source, CreatePostModel destination, ResolutionContext context)
        {
            foreach (var model in destination.Attaches)
            {
                model.AuthorId = source.AuthorId;
            }
        }
    }
}
