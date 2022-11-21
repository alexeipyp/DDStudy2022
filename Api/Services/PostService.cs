using Api.Configs;
using Api.Models.Attachments;
using Api.Models.Likes;
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
        private readonly SubscribeService _subscribeService;
        public PostService(IMapper mapper, DataContext context, SubscribeService subscribeService)
        {
            _mapper = mapper;
            _context = context;
            _subscribeService = subscribeService;
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

        public async Task LikePost(Guid userId, LikePostRequest request)
        {
            if (!(await CheckPostExist(request.PostId)))
            {
                throw new Exception("post not found");
            }
            else
            {
                var dbLike = _mapper.Map<DAL.Entities.LikeToPost>(request, o => o.AfterMap((s, d) => d.UserId = userId));
                await _context.LikesToPosts.AddAsync(dbLike);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UndoLikeToPost(Guid userId, LikeToPostUndoRequest request)
        {
            var like = await GetLikeToPostById(userId, request.PostId);
            _context.LikesToPosts.Remove(like);
            await _context.SaveChangesAsync();
        }

        public async Task LikeComment(Guid userId, LikeCommentRequest request)
        {
            if (!(await CheckCommentExist(request.CommentId)))
            {
                throw new Exception("comment not found");
            }
            else
            {
                var dbLike = _mapper.Map<DAL.Entities.LikeToComment>(request, o => o.AfterMap((s, d) => d.UserId = userId));
                await _context.LikesToComments.AddAsync(dbLike);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UndoLikeToComment(Guid userId, LikeToCommentUndoRequest request)
        {
            var like = await GetLikeToCommentById(userId, request.CommentId);
            _context.LikesToComments.Remove(like);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PostModel>> GetPosts(Guid userId, int skip, int take)
        {
            var subs = await _subscribeService.GetAuthorsIdsByFollowerId(userId);

            var posts = await _context.Posts
                .Where(x => subs.Contains(x.AuthorId))
                .Include(x => x.Author)
                .ThenInclude(x => x.Avatar) 
                .Include(x => x.PostAttaches)
                .Include(x => x.Comments)
                .Include(x => x.Likes)
                .AsNoTracking().OrderByDescending(x => x.UploadDate).Skip(skip).Take(take)
                .ToListAsync();
            
            var res = posts
                .Select(x => _mapper.Map<DAL.Entities.Post, PostModel>(x, o => o.AfterMap((s, d) => d.IsLiked = s.Likes!.Any(x => x.UserId == userId))))
                .ToList();

            return res;
        }

        public async Task<IEnumerable<CommentModel>> GetComments(Guid userId, Guid postId, int skip, int take)
        {
            var comments = await _context.Comments
                .Where(x => x.PostId == postId)
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .Include(x => x.Likes)
                .AsNoTracking().OrderByDescending(x => x.UploadDate).Skip(skip).Take(take)
                .ToListAsync();

            var res = comments
                .Select(x => _mapper.Map<DAL.Entities.Comment, CommentModel>(x, o => o.AfterMap((s, d) => d.IsLiked = s.Likes!.Any(x => x.UserId == userId))))
                .ToList();

            return res;
        }

        public async Task<AttachModel> GetPostAttach(Guid userId, Guid postAttachId)
        {
            var subs = await _subscribeService.GetAuthorsIdsByFollowerId(userId);
            var res = await _context.PostAttaches
                .Join(_context.Posts, x => x.PostId, y => y.Id, (Attach, y) => new {Attach, y.AuthorId})
                .FirstOrDefaultAsync(x => x.Attach.Id == postAttachId && subs.Contains(x.AuthorId));
            if (res == null)
                throw new Exception("post attach not found");

            return _mapper.Map<AttachModel>(res.Attach);
        }

        private async Task<bool> CheckPostExist(Guid postId) 
            => await _context.Posts.AnyAsync(x => x.Id == postId);

        private async Task<bool> CheckCommentExist(Guid commentId)
            => await _context.Comments.AnyAsync(x => x.Id == commentId);

        private async Task<DAL.Entities.LikeToPost> GetLikeToPostById(Guid userId, Guid postId)
        {
            var like = await _context.LikesToPosts.FirstOrDefaultAsync(x => x.UserId == userId && x.PostId == postId);
            if (like == null)
            {
                throw new Exception("user wasn't liked this post");
            }
            return like;
        }

        private async Task<DAL.Entities.LikeToComment> GetLikeToCommentById(Guid userId, Guid commentId)
        {
            var like = await _context.LikesToComments.FirstOrDefaultAsync(x => x.UserId == userId && x.CommentId == commentId);
            if (like == null)
            {
                throw new Exception("user wasn't liked this comment");
            }
            return like;
        }
    }
}
