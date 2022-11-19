using Api.Configs;
using Api.Models.Attachments;
using Api.Models.Post;
using Api.Models.User;
using AutoMapper;
using Common.CustomExceptions;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    public class PostService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;
        public PostService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task CreatePost(Guid userId, CreatePostRequest request)
        {
            var authorizedRequest = _mapper.Map<CreatePostAuthorizedRequest>(request, o => o.AfterMap((s, d) => d.AuthorId = userId));
            var model = _mapper.Map<CreatePostAuthorizedRequest, CreatePostModel>(authorizedRequest);

            var dbPost = _mapper.Map<DAL.Entities.Post>(model);
            await _context.Posts.AddAsync(dbPost);
            await _context.SaveChangesAsync();
        }

        public async Task CommentPost(Guid userId, CreateCommentPostRequest request)
        {

            var model = _mapper.Map<CreateCommentPostModel>(request, o => o.AfterMap((s, d) => d.AuthorId = userId));
            var dbComment = _mapper.Map<DAL.Entities.Comment>(model);
            await _context.Comments.AddAsync(dbComment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PostModel>> GetPosts(int skip, int take)
        {
            var posts = await _context.Posts
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .Include(x => x.PostAttaches)
                .Include(x => x.Comments)
                .AsNoTracking().OrderByDescending(x => x.UploadDate).Skip(skip).Take(take)
                .Select(x => _mapper.Map<PostModel>(x))
                .ToListAsync();

            return posts;
        }

        public async Task<IEnumerable<CommentModel>> GetComments(Guid postId, int skip, int take)
        {
            var comments = await _context.Comments
                .Where(x => x.PostId == postId)
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .AsNoTracking().OrderByDescending(x => x.UploadDate).Skip(skip).Take(take)
                .Select(x => _mapper.Map<CommentModel>(x))
                .ToListAsync();

            return comments;
        }

        public async Task<AttachModel> GetPostAttach(Guid postAttachId)
        {
            var res = await _context.PostAttaches.FirstOrDefaultAsync(x => x.Id == postAttachId);

            return _mapper.Map<AttachModel>(res);
        }

    }
}
