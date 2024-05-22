using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using quest_web.Contexts;
using quest_web.Models;
using quest_web.Utils;
namespace quest_web.Controllers
{
    [Route("reviews")]
    public class ReviewController : ControllerBase
    {
        private readonly APIDbContext _context = null;
        private readonly ILogger<ReviewController> _logger;
        private readonly JwtTokenUtil jwtTokenUtil;

        public ReviewController(APIDbContext context, ILogger<ReviewController> logger, JwtTokenUtil jwtTokenUtil)
        {
            _context = context;
            _logger = logger;
            this.jwtTokenUtil = jwtTokenUtil;
        }


        [HttpGet]
        public JsonResult Index()
        {
            var reviews = this._context.Reviews;
            return new JsonResult(reviews);
        }

        // GET addres/5
        [Authorize]
        [HttpGet("{id}")]
        public JsonResult Get(int id)
        {

            var shop = GetShop();
            if (shop != null)
            {
                var review = this._context.Reviews
                              .Where(r => r.Id == id)
                              .FirstOrDefault();
                if (review != null)
                {
                    if (review.shop == shop)
                    {
                       Response.StatusCode = StatusCodes.Status200OK;
                       return new JsonResult(review);
                    }
                    else
                    {
                        Response.StatusCode = StatusCodes.Status403Forbidden;
                        return new JsonResult("U don't have permissions to access this address, please contact admin");
                    }
                }
            }
            else
            {
                 Response.StatusCode = StatusCodes.Status404NotFound;
                 return new JsonResult("Invalid request : didn't found, maybe doesn't exist");
            }
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return new JsonResult("Couldn't retrieve any user, pls authenticate");
        }


        [Authorize]
        [HttpPost]
        public JsonResult Post([FromBody] Review review)
        {
            if (ModelState.IsValid)
            {

                if (this.ReviewExists(review))
                {
                    Response.StatusCode = StatusCodes.Status409Conflict;
                    return new JsonResult("Tentative de duplication : erreur 409 => vous avez déjà soumis une review, supprimer la avant");
                }
                var shop = GetShop();
                if(shop == null)
                {
                    Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return new JsonResult("U dont have the right access");
                }
                review.CreationDate = DateTime.Now;
                review.UpdatedDate = DateTime.Now;
                review.shop = shop;
                this._context.Reviews.Add(review);
                try
                {
                    this._context.SaveChanges();

                }
                catch (DbUpdateConcurrencyException e)
                {
                    Response.StatusCode = StatusCodes.Status409Conflict;
                    return new JsonResult(e);
                }
                catch (DbUpdateException e)
                {
                    Response.StatusCode = StatusCodes.Status409Conflict;
                    return new JsonResult(e.Message);
                }
            }
            Response.StatusCode = StatusCodes.Status201Created;
            return new JsonResult(review);
        }

        [Authorize]
        [HttpPut("{id}")]
        public JsonResult UpdateReview([FromRoute] int id, [FromBody] Review review)
        {
            var shop = GetShop();
            var user = GetUser();
            if (shop == null)
            {
                if(user == null || user.Role != UserRole.ROLE_ADMIN)
                {
                    Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return new JsonResult("U have to login to acces this route");
                }
            }
            var reviewToUpdate = this._context.Reviews
                              .Where(r => r.Id == id)
                              .FirstOrDefault();
            //return new JsonResult(reviewToUpdate);
            if (reviewToUpdate == null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return new JsonResult("Address not found");
            }
            if (reviewToUpdate.shop != shop)
            {
                if(user.Role != UserRole.ROLE_ADMIN)
                {
                    Response.StatusCode = StatusCodes.Status403Forbidden;
                    return new JsonResult("U do not have access");
                }
            }
            if (review.Comments != null) reviewToUpdate.Comments = review.Comments;
            if(review.IsApproved == true)
            {
                reviewToUpdate.IsApproved = true;
            }
            reviewToUpdate.UpdatedDate = DateTime.Now;
            reviewToUpdate.shop.IsPublished = true;
            this._context.Shops.Update(reviewToUpdate.shop);
            this._context.Reviews.Update(reviewToUpdate);
            _context.SaveChanges();
            return new JsonResult(reviewToUpdate);
        }

        // DELETE address/5
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<JsonResult> DeleteReview(int id)
        {
            try
            {
                var reviewToDelete = await _context.Reviews
               .FirstOrDefaultAsync(e => e.Id == id);

                if (reviewToDelete == null)
                {
                    Response.StatusCode = StatusCodes.Status404NotFound;
                    return new JsonResult(new DeleteStatus(false));
                }
                var shop = GetShop();
                if (shop == null)
                {
                    Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return new JsonResult(new DeleteStatus(false));
                }
                // fixme : add a control to autorize admin user
                if (reviewToDelete.shop != shop)
                {
                    Response.StatusCode = StatusCodes.Status403Forbidden;
                    return new JsonResult(new DeleteStatus(false));
                }
                var deletedReview = await Del(id);
                // if deleted addresqs not null return cle value json
                return new JsonResult(new DeleteStatus(true));
            }
            catch (Exception)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return new JsonResult(new DeleteStatus(false));
            }
        }

        private Shop GetShop()
        {
            var authorization = Request.Headers[HeaderNames.Authorization];
            if (AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
            {
                var bearer = headerValue.Parameter;

                string name = jwtTokenUtil.GetUsernameFromToken(bearer);
                if (name.Length > 0)
                {
                    var shops = this._context.Shops;
                    foreach (var s in shops)
                    {
                        if (s.Name == name)
                        {
                            return s;
                        }
                    }
                }
                return null;
            }
            return null;
        }


        private bool ReviewExists(Review review)
        {
            var reviews = this._context.Reviews;

            foreach (var r in reviews)
            {
                if (r.shop == review.shop)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<Review> Del(int id)
        {
            var result = await _context.Reviews
                .FirstOrDefaultAsync(e => e.Id == id);
            if (result != null)
            {
                _context.Reviews.Remove(result);
                await _context.SaveChangesAsync();
                return result;
            }

            return null;
        }

        private User GetUser()
        {
            var authorization = Request.Headers[HeaderNames.Authorization];
            if (AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
            {
                var bearer = headerValue.Parameter;

                string name = jwtTokenUtil.GetUsernameFromToken(bearer);
                if (name.Length > 0)
                {
                    var users = this._context.Users;
                    foreach (var u in users)
                    {
                        if (u.Username == name)
                        {
                            return u;
                        }
                    }
                }
                return null;
            }
            return null;
        }
    }
}
