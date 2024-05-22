using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using quest_web.Contexts;
using quest_web.Models;
using BCryptNet = BCrypt.Net.BCrypt;
using quest_web.Utils;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace quest_web.Controllers
{
    [Route("user")]
    public class UserController : ControllerBase
    {

        private readonly APIDbContext _context = null;
        private readonly ILogger<UserController> _logger;
        private readonly JwtTokenUtil jwtTokenUtil;


        public UserController(APIDbContext context, ILogger<UserController> logger, JwtTokenUtil jwtTokenUtil)
        {
            _context = context;
            _logger = logger;
            this.jwtTokenUtil = jwtTokenUtil;
        }

        // GET: api/values
        [Authorize]
        [HttpGet]
        public JsonResult Get()
        {
            var users = this._context.Users;
            var formattedUsers = new List<UserDetails>();
            foreach(var user in users)
            {
                formattedUsers.Add(new UserDetails(user.Username, user.Role));
            }
            Response.StatusCode = StatusCodes.Status200OK;
            return new JsonResult(formattedUsers);
        }

        // GET api/values/5
        [Authorize]
        [HttpGet("{id}")]
        public JsonResult Get(int id)
        {
            var user = this._context.Users
              .Where(a => a.Id == id)
              .FirstOrDefault();
            if(user == null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return new JsonResult("Didn't find any user wiht this id");
            }
            Response.StatusCode = StatusCodes.Status200OK;
            return new JsonResult(new UserDetails(user.Username, user.Role));
        }

        // PUT api/values/5
        [Authorize]
        [HttpPut("{id}")]
        public JsonResult Put([FromRoute] int id, [FromBody] User bodyUser)
        {
            var user = GetUser();
            if (user == null)
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return new JsonResult("U have to login to acces this route");
            }
            var userToUpdate = GetUser(id);
            if (userToUpdate == null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return new JsonResult("user not found");
            }
            // coudl have directly compare with id but prefer comparing objects 
            if (user.Role != UserRole.ROLE_ADMIN && userToUpdate.Id != user.Id)
            {
                Response.StatusCode = StatusCodes.Status403Forbidden;
                return new JsonResult("You do not have permission to edit this data, pls contct admin");
            }

            if (bodyUser.Username != null) userToUpdate.Username = bodyUser.Username;
            if (bodyUser.Password != null) userToUpdate.Password = BCryptNet.HashPassword(bodyUser.Password);
            //if (bodyUser.Password != null) userToUpdate.Password = BCryptNet.HashPassword(bodyUser.Password);
            // An admin can edit an user role, as follows :
            //if(user.Role == UserRole.ROLE_ADMIN)
            //{
            if (bodyUser.Role != userToUpdate.Role) userToUpdate.Role = bodyUser.Role;
            //}
            userToUpdate.UpdatedDate = DateTime.Now;
            this._context.Users.Update(userToUpdate);
            _context.SaveChanges();
            Response.StatusCode = StatusCodes.Status200OK;
            return new JsonResult(userToUpdate);
        }

        // DELETE api/values/5
        [Authorize]
        [HttpDelete("{id:int}")]
        public JsonResult DeleteUser(int id)
        {
                var userToDelete =  _context.Users
               .FirstOrDefault(e => e.Id == id);
                //return new JsonResult(userToDelete);
                if (userToDelete == null)
                {
                    Response.StatusCode = StatusCodes.Status404NotFound;
                    return new JsonResult(new DeleteStatus(false));
                }
            //return new JsonResult("ok");
                var user = GetUser();
                if (user == null)
                {
                    Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return new JsonResult(new DeleteStatus(false));
                }
                if (userToDelete != user && user.Role != UserRole.ROLE_ADMIN)
                {
                    Response.StatusCode = StatusCodes.Status403Forbidden;
                    return new JsonResult(new DeleteStatus(false));
                }
            var toRemoveAddresses = DeleteUserToDeleteAddressses(userToDelete);
                foreach(var a in toRemoveAddresses)
                {
                _context.Addresses.Update(a);
                }
                _context.Users.Remove(userToDelete);
                _context.SaveChanges();
                Response.StatusCode = StatusCodes.Status200OK;
                return new JsonResult(new DeleteStatus(true));
            }
        /*
         * Private methods
         */
        // got a user method here => not opti
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

        private User GetUser(int id)
        {
            var users = _context.Users;
            foreach (var u in users)
            {
                if (u.Id == id) return u;
            }
            return null;
        }
        private List<Address> DeleteUserToDeleteAddressses(User user)
        {
            var addresses = _context.Addresses;
            var updatedAddresses = new List<Address>();
            foreach(var a in addresses)
            {
                if(a.user == user)
                {
                    a.user = null;
                    updatedAddresses.Add(a);
                }
            }
            return updatedAddresses;
        }
    }
}
