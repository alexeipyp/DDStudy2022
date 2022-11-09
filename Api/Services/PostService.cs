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
        private Func<AttachModel, string?>? _linkContentGenerator;
        private Func<UserModel, string?>? _linkAvatarGenerator;
        public void SetLinkGenerator(Func<AttachModel, string?> linkContentGenerator, Func<UserModel, string?> linkAvatarGenerator)
        {
            _linkAvatarGenerator = linkAvatarGenerator;
            _linkContentGenerator = linkContentGenerator;
        }

        public PostService(IMapper mapper, DataContext context, AttachService attachService)
        {
            _mapper = mapper;
            _context = context;
            Console.WriteLine($"<< Post Service: {Guid.NewGuid()} >>");
        }

        public async Task CreatePost(CreatePostModel model)
        {
            var dbPost = _mapper.Map<DAL.Entities.Post>(model);
            await _context.Posts.AddAsync(dbPost);
            await _context.SaveChangesAsync();
        }

        public async Task CommentPost(CreateCommentPostModel model)
        {
            var dbComment = _mapper.Map<DAL.Entities.Comment>(model);
            var comment = await _context.Comments.AddAsync(dbComment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PostModel>> GetPosts(int skip, int take)
        {
            var posts = await _context.Posts
                .Include(x => x.Author)
                .ThenInclude(x => x.Avatar)
                .Include(x => x.PostAttaches)
                .Include(x => x.Comments)
                .AsNoTracking().Take(take).Skip(skip).ToListAsync();

            var res = posts.Select(post =>
                new PostModel
                {
                    Author = new UserAvatarModel(_mapper.Map<UserModel>(post.Author), post.Author.Avatar == null ? null : _linkAvatarGenerator),
                    Annotation = post.Annotation,
                    Id = post.Id,
                    UploadDate = post.UploadDate,
                    Attaches = post.PostAttaches?.Select(x =>
                    new AttachWithLinkModel(_mapper.Map<AttachModel>(x), _linkContentGenerator)).ToList(),
                    CommentsAmount = post.Comments is null ? 0 : post.Comments.Count 
                }).ToList();

            return res;
        }

        public async Task<List<CommentModel>> GetComments(Guid postId, int skip, int take)
        {
            var comments = await _context.Comments
                .Where(x => x.PostId == postId)
                .Include(x => x.Author)
                .ThenInclude(x => x.Avatar)
                .AsNoTracking().Take(take).Skip(skip).ToListAsync();

            var res = comments.Select(comment =>
                new CommentModel
                {
                    Author = new UserAvatarModel(_mapper.Map<UserModel>(comment.Author), comment.Author.Avatar == null ? null : _linkAvatarGenerator),
                    Text = comment.Text,
                    UploadDate = comment.UploadDate,
                }).ToList();

            return res;
        }

        public async Task<AttachModel> GetPostAttach(Guid postAttachId)
        {
            var res = await _context.PostAttaches.FirstOrDefaultAsync(x => x.Id == postAttachId);

            return _mapper.Map<AttachModel>(res);
        }
    }
}
