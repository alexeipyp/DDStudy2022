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
        private readonly AccessAndFilterService _accessService;
        public PostService(IMapper mapper, DataContext context, AccessAndFilterService accessService)
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

        public async Task<IEnumerable<PostModel>> GetSearchFeed(Guid userId, int take, DateTimeOffset? upTo)
        {
            var filteredUsers = _accessService.GetSearchFeedFiltering(userId);
            var posts = await GetPostsAllowedToUser(userId, filteredUsers, take, upTo);

            return posts;
        }
        public async Task<IEnumerable<PostModel>> GetSubscriptionsFeed(Guid userId, int take, DateTimeOffset? upTo)
        {
            var accessPolicy = _accessService.GetSubscribeFeedFiltering(userId);
            var posts = await GetPostsAllowedToUser(userId, accessPolicy, take, upTo);

            return posts;
        }
        public async Task<IEnumerable<PostModel>> GetUserPosts(Guid userId, Guid userToVisitId, int take, DateTimeOffset? upTo)
        {
            var accessPolicy = _accessService.GetUserAccessPolicy(userId, userToVisitId);
            var posts = await GetPostsAllowedToUser(userId, accessPolicy, take, upTo);

            return posts;
        }

        public async Task<IEnumerable<PostModel>> GetFavoritePosts(Guid userId, int take, DateTimeOffset? upTo)
        {
            var permittedUsers = _accessService.GetReadAccessPolicy(userId);
            var postQuery = _context.Posts.AsQueryable()
                .Join(permittedUsers, x => x.AuthorId, y => y.Id, (x, y) => x)
                .Include(x => x.Author)
                .ThenInclude(x => x.Avatar)
                .Include(x => x.PostAttaches)
                .Join(_context.LikesToPosts.Where(x => x.UserId == userId), post => post.Id, like => like.PostId, (post, like) => new { post, like.Created })
                .AsNoTracking()
                ;

            if (upTo != null)
            {
                postQuery = postQuery.Where(x => x.Created < upTo);
            }

            postQuery = postQuery
                .OrderByDescending(x => x.Created)
                .Take(take);

            var posts = await postQuery.ToListAsync();

            var postStatsQuery = _context.PostsStats.Join(postQuery, x => x.Id, y => y.post.Id, (x, y) => x);
            var postsStats = (await postStatsQuery.ToListAsync()).Select(x => _mapper.Map<DAL.Entities.PostStatsPersonal>(x));

            var postsWithStats = posts.Join(postsStats, x => x.post.Id, y => y.Id,
                (x, y) => _mapper.Map<DAL.Entities.PostWithStats>(x.post, o => o.AfterMap((s, d) => 
                { 
                    d.Stats = y; 
                    d.Stats.WhenLiked = x.Created; 
                }))).Select(x => _mapper.Map<PostModel>(x));

            return postsWithStats;
        }


        public async Task<IEnumerable<CommentModel>> GetComments(Guid userId, Guid postId, int take, DateTimeOffset? upTo)
        {
            var permittedUsers = _accessService.GetReadAccessPolicy(userId);
            var commentsQuery = _context.Comments.AsQueryable();
            if (upTo != null)
            {
                commentsQuery = commentsQuery.Where(x => x.UploadDate > upTo);
            }
            commentsQuery = commentsQuery
                .Where(x => x.PostId == postId)
                .Include(x => x.Post)
                .Join(permittedUsers, x => x.Post.AuthorId, y => y.Id, (x, y) => x)
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .AsNoTracking().OrderBy(x => x.UploadDate).Take(take);

            var comments = await commentsQuery.ToListAsync();

            var commentsStatsQuery = from stats in _context.CommentsStats.Where(x => x.PostId == postId)
                                     join likes in _context.LikesToComments.Where(x => x.UserId == userId)
                                    on stats.Id equals likes.CommentId into grouping
                                 from likes in grouping.DefaultIfEmpty()
                                 select new { stats, likes };
            var commentsStats = (await commentsStatsQuery.ToListAsync()).Select(a => _mapper.Map<DAL.Entities.CommentStatsPersonal>(a.stats, o => o.AfterMap((s, d) => d.WhenLiked = a.likes?.Created)));

            var commentsWithStats = comments.Join(commentsStats, x => x.Id, y => y.Id,
                (x, y) => _mapper.Map<DAL.Entities.CommentWithStats>(x, o => o.AfterMap((s, d) => d.Stats = y)))
                .Select(x => _mapper.Map<CommentModel>(x));

            return commentsWithStats;
        }

        public async Task<AttachModel> GetPostAttach(Guid userId, Guid postAttachId)
        {
            var res = await _context.PostAttaches
                .Include(x => x.Post)
                .Join(_accessService.GetReadAccessPolicy(userId), x => x.Post.AuthorId, y => y.Id, (x, y) => x)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == postAttachId);
            if (res == null)
                throw new PostAttachNotFoundException();

            return _mapper.Map<AttachModel>(res);
        }

        public async Task<PostStatsModel> GetPostStatsByUserIdAndPostId(Guid userId, Guid postId)
        {
            var postStats = await _context.PostsStats.FirstOrDefaultAsync(x => x.Id == postId);
            var like = await _context.LikesToPosts.FirstOrDefaultAsync(x => x.UserId == userId && x.PostId == postId);
            DateTimeOffset? whenLiked = like != null ? like.Created : null;
            var res = _mapper.Map<PostStatsModel>(postStats, o => o.AfterMap((s, d) => d.WhenLiked = whenLiked));
            return res;
        }

        private async Task<CommentStatsModel> GetCommentStatsByUserIdAndCommentId(Guid userId, Guid commentId)
        {
            var commentStats = await _context.CommentsStats.FirstOrDefaultAsync(x => x.Id == commentId);
            var like = await _context.LikesToComments.FirstOrDefaultAsync(x => x.UserId == userId && x.CommentId == commentId);
            DateTimeOffset? whenLiked = like != null ? like.Created : null;
            var res = _mapper.Map<CommentStatsModel>(commentStats, o => o.AfterMap((s, d) => d.WhenLiked = whenLiked));
            return res;
        }

        private async Task<DAL.Entities.Post> GetPostById(Guid postId)
        {
            var post = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null)
                throw new PostNotFoundException();

            return post;
        }

        private async Task<IEnumerable<PostModel>> GetPostsAllowedToUser(Guid userId, IQueryable<DAL.Entities.User> permittedUsers, int take, DateTimeOffset? upTo)
        {
            var postQuery = _context.Posts.AsQueryable();
            if (upTo != null)
            {
                postQuery = postQuery.Where(x => x.UploadDate < upTo);
            }
            postQuery = postQuery
                .Join(permittedUsers, x => x.AuthorId, y => y.Id, (x, y) => x)
                .Include(x => x.Author)
                .ThenInclude(x => x.Avatar)
                .Include(x => x.PostAttaches)
                .AsNoTracking()
                .OrderByDescending(x => x.UploadDate)
                .Take(take);

            var posts = await postQuery.ToListAsync();

            var postStatsQuery = _context.PostsStats.Join(postQuery, x => x.Id, y => y.Id, (x, y) => x);
            var postStatsJoinLikesQuery = from stats in postStatsQuery
                             join likes in _context.LikesToPosts.Where(x => x.UserId == userId)
                                on stats.Id equals likes.PostId into grouping
                             from likes in grouping.DefaultIfEmpty()
                             select new { stats, likes};
            var postsStats = (await postStatsJoinLikesQuery.ToListAsync()).Select(a => _mapper.Map<DAL.Entities.PostStatsPersonal>(a.stats, o => o.AfterMap((s, d) => d.WhenLiked = a.likes?.Created)));
            
            var postsWithStats = posts.Join(postsStats, x => x.Id, y => y.Id,
                (x, y) => _mapper.Map<DAL.Entities.PostWithStats>(x, o => o.AfterMap((s, d) => d.Stats = y))).Select(x => _mapper.Map<PostModel>(x));

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

    }
}
