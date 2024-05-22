using System;
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
    public class MarkController : ControllerBase
    {

        private readonly APIDbContext _context = null;
        private readonly ILogger<MarkController> _logger;
        private readonly JwtTokenUtil jwtTokenUtil;

        public MarkController(APIDbContext context, ILogger<MarkController> logger, JwtTokenUtil jwtTokenUtil)
        {
            _context = context;
            _logger = logger;
            this.jwtTokenUtil = jwtTokenUtil;
        }

        [HttpGet]
        [Authorize]
        public JsonResult Index()
        {
            var marks = this._context.Marks;
            return new JsonResult(marks);
        }

        [Authorize]
        [HttpGet("{id}")]
        public JsonResult Get(int id)
        {

            var mark = this._context.Marks
                           .Where(r => r.Id == id)
                           .FirstOrDefault();

            if(mark != null)
            {
                Response.StatusCode = StatusCodes.Status200OK;
                return new JsonResult(mark);
            }
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return new JsonResult("Couldn't retrieve any user, pls authenticate");
        }

        [Authorize]
        [HttpPost]
        public JsonResult Post([FromBody] Mark mark)
        {
            if (ModelState.IsValid)
            {
                var user = GetUser();
                var shop = GetShop(mark.shop);
                if(shop == null)
                {
                    Response.StatusCode = StatusCodes.Status500InternalServerError;
                    return new JsonResult("Il semble y avoir une erreur interne...");
                }
                mark.user = user;
                mark.shop = shop;
                mark.CreationDate = DateTime.Now;
                mark.UpdatedDate = DateTime.Now;
                this._context.Marks.Add(mark);
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
            return new JsonResult(mark);
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

        private Shop GetShop(Shop shop)
        {
            var shops = this._context.Shops;
            foreach(var s in shops)
            {
                if (s == shop) return shop;
            }
            return null;
        }
    }




}
