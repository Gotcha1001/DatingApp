

using API.Dtos;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRespository _likesRespository;

        public LikesController(IUserRepository userRepository, ILikesRespository likesRespository)
        {
            _userRepository = userRepository;
            _likesRespository = likesRespository;
        }

        [HttpPost("{username}")]

        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _userRepository.GetUserByUsernameAsync(username);
            var sourceUser = await _likesRespository.GetUserWithLikes(sourceUserId);


            if (likedUser == null) return NotFound();
            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");
            var userLike = await _likesRespository.GetUserLike(sourceUserId, likedUser.Id);
            if (userLike != null) return BadRequest("You already like this user");

            userLike = new Entities.UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };

            sourceUser.LikeUsers.Add(userLike);

            if (await _userRepository.SaveAllAsync()) return Ok();
            return BadRequest("Failed to like user");

        }

        [HttpGet]

        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _likesRespository.GetUserLikes(likesParams);
            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage,
             users.PageSize, users.TotalCount, users.TotalPages));
            return Ok(users);
        }
        
    }
}