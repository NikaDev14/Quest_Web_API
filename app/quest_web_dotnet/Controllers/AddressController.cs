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
    [Route("address")]
    public class AddressController : ControllerBase
    {
        private readonly APIDbContext _context = null;
        private readonly ILogger<AddressController> _logger;
        private readonly JwtTokenUtil jwtTokenUtil;

        public AddressController(APIDbContext context, ILogger<AddressController> logger, JwtTokenUtil jwtTokenUtil)
        {
            _context = context;
            _logger = logger;
            this.jwtTokenUtil = jwtTokenUtil;
        }

        // GET: address
        [Authorize]
        [HttpGet]
        public JsonResult Get()
        {
            //facultative with the authorize attribute
            var user = GetUser();
            if(user != null)
            {
                var addresses = this._context.Addresses;
                if (user.Role == UserRole.ROLE_ADMIN)
                {
                    Response.StatusCode = StatusCodes.Status200OK;
                    return new JsonResult(addresses);
                }
                else
                {
                    var customerAddresses = new List<Address>();
                    foreach (var a in addresses)
                    {
                        if (a.user == user)
                        {
                            customerAddresses.Add(a);
                        }
                    }
                    Response.StatusCode = StatusCodes.Status200OK;
                    return new JsonResult(customerAddresses);
                }
            }
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return new JsonResult("Couldn't retrieve any user, pls authenticate");
        }

        // GET addres/5
        [Authorize]
        [HttpGet("{id}")]
        public JsonResult Get(int id)
        {
            var user = GetUser();
            if (user != null)
            {
                var address = this._context.Addresses
                              .Where(a => a.Id == id)
                              .FirstOrDefault();
                if(address != null)
                {
                    if(user.Role == UserRole.ROLE_USER)
                    {
                        if (address.user == user)
                        {
                            Response.StatusCode = StatusCodes.Status200OK;
                            return new JsonResult(address);
                        } 
                        else
                        {
                            Response.StatusCode = StatusCodes.Status403Forbidden;
                            return new JsonResult("U don't have permissions to access this address, please contact admin");
                        } 
                    }
                    if (user.Role == UserRole.ROLE_ADMIN)
                    {
                        Response.StatusCode = StatusCodes.Status200OK;
                        return new JsonResult(address);
                    } 
                }
                else
                {
                    // here is my testing now
                    Response.StatusCode = StatusCodes.Status404NotFound;
                    return new JsonResult("Invalid request : didn't found, maybe doesn't exist");
                }
            }
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return new JsonResult("Couldn't catch any valid user, pls authenticate");
        }

        // POST address
        [Authorize]
        [HttpPost]
        public JsonResult Post([FromBody] Address address)
        {
            if (ModelState.IsValid)
            {
                
                if (this.AddressExists(address))
                {
                    Response.StatusCode = StatusCodes.Status409Conflict;
                    return new JsonResult("Tentative de duplication : erreur 409");
                }
                var user = GetUser();
                address.CreationDate = DateTime.Now;
                address.UpdatedDate = DateTime.Now;
                if (user != null) address.user = user;
                this._context.Addresses.Add(address);
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
            return new JsonResult(address);
        }
        
        // PUT address/5
        [Authorize]
        [HttpPut("{id}")]
        public  JsonResult UpdateAdress([FromRoute]int id, [FromBody] Address address)
        {
            var user = GetUser();
            if(user == null)
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return new JsonResult("U have to login to acces this route");
            }
            var addrToUpdate = GetAddress(id);
            if (addrToUpdate == null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return new JsonResult("Address not found");
            }
            if (user.Role == UserRole.ROLE_USER)
            {
                if (addrToUpdate.user != user)
                {
                    Response.StatusCode = StatusCodes.Status403Forbidden;
                    return new JsonResult("U cannot have access to an address which is not yours");
                }     
            }
            if (address.City != null) addrToUpdate.City = address.City;
            if (address.Country != null) addrToUpdate.Country = address.Country;
            if (address.PostalCode != null) addrToUpdate.PostalCode = address.PostalCode;
            if (address.Road != null) addrToUpdate.Road = address.Road;
            addrToUpdate.UpdatedDate = DateTime.Now;
            this._context.Addresses.Update(addrToUpdate);
            _context.SaveChanges();
            return new JsonResult(addrToUpdate);
        }

        // DELETE address/5
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<JsonResult> DeleteAddress(int id)
        {
            try
            {
             var addrToDelete = await _context.Addresses
            .FirstOrDefaultAsync(e => e.Id == id); 

                if (addrToDelete == null)
                {
                    Response.StatusCode = StatusCodes.Status404NotFound;
                    return new JsonResult(new DeleteStatus(false));
                }
                var user = GetUser();
                if(user == null)
                {
                    Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return new JsonResult(new DeleteStatus(false));
                }
                if(addrToDelete.user != user && user.Role != UserRole.ROLE_ADMIN)
                {
                    Response.StatusCode = StatusCodes.Status403Forbidden;
                    return new JsonResult(new DeleteStatus(false));
                }
                var deletedAddress = await Del(id);
                // if deleted addresqs not null return cle value json
                return new JsonResult(new DeleteStatus(true));
            }
            catch (Exception)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return new JsonResult(new DeleteStatus(false));
            }
        }

        /*
         * Private methods => maybe refacto in repositories to avoid duplication
         */

 
        private bool AddressExists(Address address)
        {
            var addresses = this._context.Addresses;

            foreach (var a in addresses)
            {
                if (a.Road == address.Road && a.PostalCode == address.PostalCode && a.City == address.City && a.Country == address.Country)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<Address> Del(int id)
        {
            var result = await _context.Addresses
                .FirstOrDefaultAsync(e => e.Id == id);
            if (result != null)
            {
                _context.Addresses.Remove(result);
                await _context.SaveChangesAsync();
                return result;
            }

            return null;
        }

        private Address GetAddress(int id)
        {
            var addresses = _context.Addresses;
            foreach(var a in addresses)
            {
                if (a.Id == id) return a;
            }
            return null;
        }

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
    }
}
