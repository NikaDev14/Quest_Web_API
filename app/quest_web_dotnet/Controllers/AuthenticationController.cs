using System;
using Microsoft.AspNetCore.Mvc;
using quest_web.Contexts;
using Microsoft.Extensions.Logging;
using quest_web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using quest_web.Utils;
using BCryptNet = BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;

namespace quest_web.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly APIDbContext _context=null;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly JwtTokenUtil jwtTokenUtil;
        public AuthenticationController(APIDbContext context, ILogger<AuthenticationController> logger, JwtTokenUtil jwtTokenUtil)
        {
            this._context = context;
            this.jwtTokenUtil = jwtTokenUtil;
            _logger = logger;
        }


        [HttpPost("/register")]
        public JsonResult Create(User user)
        {
            var detailedUser = new UserDetails();
            if (ModelState.IsValid)
            {
                if(this.UserExists(user))
                {
                    Response.StatusCode = StatusCodes.Status409Conflict;

                return new JsonResult("Tentative de duplication : erreur 409");
                }
                user.CreationDate = DateTime.Now;
                user.Password = BCryptNet.HashPassword(user.Password);
                this._context.Users.Add(user);
                try
                {
                    this._context.SaveChanges();

                    detailedUser.Username = user.Username;
                    detailedUser.Role = user.Role;
                }
                catch(DbUpdateConcurrencyException e)
                {
                    return new JsonResult(e);
                }
                catch(DbUpdateException e)
                {
                    Response.StatusCode = StatusCodes.Status409Conflict;
                    return new JsonResult(e.Message);
                }
            }
            else
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return new JsonResult("invalid request");
            }
            Response.StatusCode = StatusCodes.Status201Created;
            return new JsonResult(detailedUser);
        }
        [HttpPost("/register_admin")]
        public JsonResult CreateAdmin(User user)
        {
            var detailedUser = new UserDetails();
            if (ModelState.IsValid)
            {
                if (this.UserExists(user))
                {
                    Response.StatusCode = StatusCodes.Status409Conflict;

                    return new JsonResult("Tentative de duplication : erreur 409");
                }
                user.CreationDate = DateTime.Now;
                user.Password = BCryptNet.HashPassword(user.Password);
                user.Role = UserRole.ROLE_ADMIN;
                this._context.Users.Add(user);
                try
                {
                    this._context.SaveChanges();

                    detailedUser.Username = user.Username;
                    detailedUser.Role = user.Role;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    return new JsonResult(e);
                }
                catch (DbUpdateException e)
                {
                    Response.StatusCode = StatusCodes.Status409Conflict;
                    return new JsonResult(e.Message);
                }
            }
            else
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return new JsonResult("invalid request");
            }
            Response.StatusCode = StatusCodes.Status201Created;
            return new JsonResult(detailedUser);
        }

        [Authorize]
        [HttpGet("/getall")]
        public JsonResult Index()
        {
            var users = this._context.Users;
            return new JsonResult(users);
        }

        private bool UserExists(User user)
        {
            var users = this._context.Users;
            foreach(var u in users)
            {
                if(u.Username.Trim().ToLower() == user.Username.Trim().ToLower())
                {
                    return true;
                }
            }
            return false;
        }
        
        [HttpPost("/authenticate")]
        public JsonResult Login([FromBody] User user)
        {
            string token;
            if (ModelState.IsValid)
            {
                var users = this._context.Users;
                
                foreach (var u in users)
                {
                    if (u.Username == user.Username && BCryptNet.Verify(user.Password, u.Password))
                    {
                        token = jwtTokenUtil.GenerateToken(new UserDetails(u.Username, u.Role));
                        return new JsonResult(token);
                    }
                }
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return new JsonResult("Nom d'utilisateur ou mot de passe incorrect");
            }
            else
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;

                return new JsonResult("Erreur dans l'envoi : erreur 400");
            }
        }

        [Authorize]
        [HttpGet("/me")]
        public JsonResult Me()
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
                            return new JsonResult(new UserDetails(u.Username, u.Role));
                        }
                    }
                    Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return new JsonResult("Json invalide ou incorrect");

                }
            }
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult("Requête invalide");
        }

    }

}
