using Api.Configs;
using Api.Models.Attachments;
using Api.Models.Likes;
using Api.Models.Post;
using Api.Models.User;
using AutoMapper;
using Common.CustomExceptions;
using Common.CustomExceptions.ForbiddenExceptions;
using Common.CustomExceptions.NotFoundExceptions;
using Common.Enums;
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
            if (!(await _accessService.GetWritePermission(userId, post.AuthorId)))
                throw new ForbiddenException("no permission");

            var model = _mapper.Map<CreateCommentPostModel>(request, o => o.AfterMap((s, d) => d.AuthorId = userId));
            var dbComment = _mapper.Map<DAL.Entities.Comment>(model);
            await _context.Comments.AddAsync(dbComment);
            await _context.SaveChangesAsync();
        }

        public async Task<PostStatsModel> LikePost(Guid userId, LikePostRequest request)
        {
            var post = await GetPostById(request.PostId);
            if (!(await _accessService.GetReadPermission(userId, post.AuthorId)))
                throw new ForbiddenException("no permission");

            var like = await GetLikeToPostById(userId, request.PostId);

            if (like == null)
            {
                var dbLike = _mapper.Map<DAL.Entities.LikeToPost>(request, o => o.AfterMap((s, d) => d.UserId = userId));
                await _context.LikesToPosts.AddAsync(dbLike);
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.LikesToPosts.Remove(like);
                await _context.SaveChangesAsync();
            }     

            return await GetPostStatsByUserIdAndPostId(userId, request.PostId);
        }

        public async Task<CommentStatsModel> LikeComment(Guid userId, LikeCommentRequest request)
        {
            var comment = await GetCommentWithPostById(request.CommentId);
            if (!(await _accessService.GetReadPermission(userId, comment.Post.AuthorId)))
                throw new ForbiddenException("no permission");

            var like = await GetLikeToCommentById(userId, request.CommentId);

            if (like == null)
            {
                var dbLike = _mapper.Map<DAL.Entities.LikeToComment>(request, o => o.AfterMap((s, d) => d.UserId = userId));
                await _context.LikesToComments.AddAsync(dbLike);
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.LikesToComments.Remove(like);
                await _context.SaveChangesAsync();
            }

            return await GetCommentStatsByUserIdAndCommentId(userId, request.CommentId);
        }

        public async Task<IEnumerable<PostModel>> GetFeed(Guid userId, int take, DateTimeOffset? upTo)
        {
            var accessPolicy = _accessService.GetFeedAccessPolicy(userId);
            var posts = await GetPostsAllowedToUser(userId, accessPolicy, take, upTo);

            return posts;
        }
        public async Task<IEnumerable<PostModel>> GetSubscriptionsFeed(Guid userId, int take, DateTimeOffset? upTo)
        {
            var accessPolicy = _accessService.GetFollowerAccessPolicy(userId);
            var posts = await GetPostsAllowedToUser(userId, accessPolicy, take, upTo);

            return posts;
        }
        public async Task<IEnumerable<PostModel>> GetUserPosts(Guid userId, Guid userToVisitId, int take, DateTimeOffset? upTo)
        {
            var accessPolicy = _accessService.GetUserAccessPolicy(userId, userToVisitId);
            var posts = await GetPostsAllowedToUser(userId, accessPolicy, take, upTo);

            return posts;
        }

        public async Task<IEnumerable<CommentModel>> GetComments(Guid userId, Guid postId, int skip, int take)
        {
            var accessPolicy = _accessService.GetReadAccessPolicy(userId);
            var comments = await _context.Comments
                .Where(x => x.PostId == postId)
                .Include(x => x.Post)
                .Join(accessPolicy, x => x.Post.AuthorId, y => y, (x, y) => x)
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .AsNoTracking().OrderByDescending(x => x.UploadDate).Skip(skip).Take(take)
                .ToListAsync();

            var commentsStats = await _context.CommentsStats.AsNoTracking().Where(x => x.PostId == postId).ToListAsync();
            var likedCommentIds = await GetLikedCommentIdsByUserId(userId, postId);
            var commentsWithStats = comments.Join(commentsStats, x => x.Id, y => y.Id,
                (x, y) => _mapper.Map(y, _mapper.Map<DAL.Entities.CommentWithStats>(x)))
                .Select(x => _mapper.Map<CommentModel>(x, o => o.AfterMap((s, d) =>
                {
                    d.Stats.IsLiked = likedCommentIds.Contains(d.Id);
                })));

            return commentsWithStats;
        }

        public async Task<AttachModel> GetPostAttach(Guid userId, Guid postAttachId)
        {
            var res = await _context.PostAttaches
                .Include(x => x.Post)
                .Join(_accessService.GetReadAccessPolicy(userId), x => x.Post.AuthorId, y => y, (x, y) => x)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == postAttachId);
            if (res == null)
                throw new PostAttachNotFoundException();

            return _mapper.Map<AttachModel>(res);
        }

        private async Task<PostStatsModel> GetPostStatsByUserIdAndPostId(Guid userId, Guid postId)
        {
            var postStats = await _context.PostsStats.FirstOrDefaultAsync(x => x.Id == postId);
            var isLiked = await _context.LikesToPosts.AnyAsync(x => x.UserId == userId && x.PostId == postId);
            var res = _mapper.Map<PostStatsModel>(postStats, o => o.AfterMap((s, d) => d.IsLiked = isLiked));
            return res;
        }

        private async Task<CommentStatsModel> GetCommentStatsByUserIdAndCommentId(Guid userId, Guid commentId)
        {
            var commentStats = await _context.CommentsStats.FirstOrDefaultAsync(x => x.Id == commentId);
            var isLiked = await _context.LikesToComments.AnyAsync(x => x.UserId == userId && x.CommentId == commentId);
            var res = _mapper.Map<CommentStatsModel>(commentStats, o => o.AfterMap((s, d) => d.IsLiked = isLiked));
            return res;
        }

        private async Task<DAL.Entities.Post> GetPostById(Guid postId)
        {
            var post = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null)
                throw new PostNotFoundException();

            return post;
        }

        private async Task<IEnumerable<PostModel>> GetPostsAllowedToUser(Guid userId, IQueryable<Guid> accessPolicy, int take, DateTimeOffset? upTo)
        {
            var postQuery = _context.Posts.AsQueryable();
            if (upTo != null)
            {
                postQuery = postQuery.Where(x => x.UploadDate < upTo);
            }
            postQuery = postQuery.Join(accessPolicy, x => x.AuthorId, y => y, (x, y) => x)
                .Include(x => x.Author)
                .ThenInclude(x => x.Avatar)
                .Include(x => x.PostAttaches)
                .AsNoTracking()
                .OrderByDescending(x => x.UploadDate)
                .Take(take);
            
            var posts = await postQuery.ToListAsync();

            var postsStats = await _context.PostsStats.AsNoTracking().ToListAsync();
            var likedPostIds = await GetLikedPostIdsByUserId(userId);
            var postsWithStats = posts.Join(postsStats, x => x.Id, y => y.Id,
                (x, y) => _mapper.Map(y, _mapper.Map<DAL.Entities.PostWithStats>(x)))
                .Select(x => _mapper.Map<PostModel>(x, o => o.AfterMap((s, d) =>
                {
                    d.Stats.IsLiked = likedPostIds.Contains(d.Id);
                })));

            return postsWithStats;
        }

        private async Task<DAL.Entities.Comment> GetCommentWithPostById(Guid commentId)
        {
            var comment = await _context.Comments
                .Include(x => x.Post)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null)
                throw new LikeNotFoundException();

            return comment;
        }

        private async Task<DAL.Entities.LikeToPost?> GetLikeToPostById(Guid userId, Guid postId)
        {
            var like = await _context.LikesToPosts.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId && x.PostId == postId);

            return like;
        }

        private async Task<DAL.Entities.LikeToComment?> GetLikeToCommentById(Guid userId, Guid commentId)
        {
            var like = await _context.LikesToComments.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId && x.CommentId == commentId);

            return like;
        }

        private async Task<List<Guid>> GetLikedPostIdsByUserId(Guid userId)
        {
            var likes = await _context.LikesToPosts
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => x.PostId)
                .ToListAsync();

            return likes;
        }

        private async Task<List<Guid>> GetLikedCommentIdsByUserId(Guid userId, Guid postId)
        {
            var likes = await _context.LikesToComments
                .Include(x => x.Comment)
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Comment.PostId == postId)
                .Select(x => x.CommentId)
                .ToListAsync();

            return likes;
        }
    }
}
