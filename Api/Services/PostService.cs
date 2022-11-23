using Api.Configs;
using Api.Models.Attachments;
using Api.Models.Likes;
using Api.Models.Post;
using Api.Models.User;
using AutoMapper;
using Common.CustomExceptions;
using Common.CustomExceptions.ForbiddenExceptions;
using Common.CustomExceptions.NotFoundExceptions;
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
        private readonly AccessManagementService _accessService;
        public PostService(IMapper mapper, DataContext context, AccessManagementService accessService)
        {
            _mapper = mapper;
            _context = context;
            _accessService = accessService;
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
            var post = await GetPostById(request.PostId);
            if (!(await _accessService.GetWriteAccessPermission(userId, post.AuthorId)))
                throw new ForbiddenException("no permission");

            var model = _mapper.Map<CreateCommentPostModel>(request, o => o.AfterMap((s, d) => d.AuthorId = userId));
            var dbComment = _mapper.Map<DAL.Entities.Comment>(model);
            await _context.Comments.AddAsync(dbComment);
            await _context.SaveChangesAsync();
        }

        public async Task LikePost(Guid userId, LikePostRequest request)
        {
            var post = await GetPostById(request.PostId);
            if (!(await _accessService.GetWriteAccessPermission(userId, post.AuthorId)))
                throw new ForbiddenException("no permission");
            if (await CheckLikeToPostExist(userId, request.PostId))
                throw new ForbiddenException("like already exists");

            var dbLike = _mapper.Map<DAL.Entities.LikeToPost>(request, o => o.AfterMap((s, d) => d.UserId = userId));
            await _context.LikesToPosts.AddAsync(dbLike);
            await _context.SaveChangesAsync();
        }

        public async Task UndoLikeToPost(Guid userId, LikeToPostUndoRequest request)
        {
            var post = await GetPostById(request.PostId);
            if (!(await _accessService.GetWriteAccessPermission(userId, post.AuthorId)))
                throw new ForbiddenException("no permission");

            var like = await GetLikeToPostById(userId, request.PostId);
            _context.LikesToPosts.Remove(like);
            await _context.SaveChangesAsync();
        }

        public async Task LikeComment(Guid userId, LikeCommentRequest request)
        {
            var comment = await GetCommentWithPostById(request.CommentId);
            if (!(await _accessService.GetWriteAccessPermission(userId, comment.Post.AuthorId)))
                throw new ForbiddenException("no permission");
            if (await CheckLikeToCommentExist(userId, request.CommentId))
                throw new ForbiddenException("like already exists");

            var dbLike = _mapper.Map<DAL.Entities.LikeToComment>(request, o => o.AfterMap((s, d) => d.UserId = userId));
            await _context.LikesToComments.AddAsync(dbLike);
            await _context.SaveChangesAsync();
        }

        public async Task UndoLikeToComment(Guid userId, LikeToCommentUndoRequest request)
        {
            var comment = await GetCommentWithPostById(request.CommentId);
            if (!(await _accessService.GetWriteAccessPermission(userId, comment.Post.AuthorId)))
                throw new ForbiddenException("no permission");

            var like = await GetLikeToCommentById(userId, request.CommentId);
            _context.LikesToComments.Remove(like);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PostModel>> GetFeed(Guid userId, int skip, int take)
        {
            var posts = await _context.Posts
                .Join(_accessService.GetReadAccessPolicy(userId), x => x.AuthorId, y => y, (x, y) => x)
                .Where(x => x.AuthorId != userId)
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

        public async Task<IEnumerable<PostModel>> GetSubscriptionsFeed(Guid userId, int skip, int take)
        {
            var posts = await _context.Posts
                .Join(_accessService.GetFollowerAccessPolicy(userId), x => x.AuthorId, y => y, (x, y) => x)
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

        public async Task<IEnumerable<PostModel>> GetUserPosts(Guid userId, Guid userToVisitId, int skip, int take)
        {
            var posts = await _context.Posts
                .Join(_accessService.GetReadAccessPolicy(userId), x => x.AuthorId, y => y, (x, y) => x)
                .Where(x => x.AuthorId == userToVisitId)
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
                .Include(x => x.Post)
                .Join(_accessService.GetReadAccessPolicy(userId), x => x.Post.AuthorId, y => y, (x, y) => x)
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
            var res = await _context.PostAttaches
                .Include(x => x.Post)
                .Join(_accessService.GetReadAccessPolicy(userId), x => x.Post.AuthorId, y => y, (x, y) => x)
                .FirstOrDefaultAsync(x => x.Id == postAttachId);
            if (res == null)
                throw new PostAttachNotFoundException("post attach not found");

            return _mapper.Map<AttachModel>(res);
        }

        private async Task<DAL.Entities.Post> GetPostById(Guid postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null)
                throw new PostNotFoundException("post not found");

            return post;
        }

        private async Task<DAL.Entities.Comment> GetCommentWithPostById(Guid commentId)
        {
            var comment = await _context.Comments
                .Include(x => x.Post)
                .FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null)
                throw new LikeNotFoundException("comment not found");

            return comment;
        }

        private async Task<DAL.Entities.LikeToPost> GetLikeToPostById(Guid userId, Guid postId)
        {
            var like = await _context.LikesToPosts.FirstOrDefaultAsync(x => x.UserId == userId && x.PostId == postId);
            if (like == null)
                throw new LikeNotFoundException("user wasn't liked this post");

            return like;
        }

        private async Task<DAL.Entities.LikeToComment> GetLikeToCommentById(Guid userId, Guid commentId)
        {
            var like = await _context.LikesToComments.FirstOrDefaultAsync(x => x.UserId == userId && x.CommentId == commentId);
            if (like == null)
                throw new LikeNotFoundException("user wasn't liked this comment");

            return like;
        }

        private async Task<bool> CheckLikeToPostExist(Guid userId, Guid postId)
        {
            return await _context.LikesToPosts.AnyAsync(x => x.UserId == userId && x.PostId == postId);
        }

        private async Task<bool> CheckLikeToCommentExist(Guid userId, Guid commentId)
        {
            return await _context.LikesToComments.AnyAsync(x => x.UserId == userId && x.CommentId == commentId);
        }
    }
}
