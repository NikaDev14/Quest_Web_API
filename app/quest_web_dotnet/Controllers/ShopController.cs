using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using quest_web.Contexts;
using quest_web.Utils;
using System.Collections.Generic;

using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;

using quest_web.Models;
using BCryptNet = BCrypt.Net.BCrypt;

using Microsoft.EntityFrameworkCore;
namespace quest_web.Controllers
{
    [Route("shop")]
    public class ShopController : ControllerBase
    {

        private readonly APIDbContext _context = null;
        private readonly ILogger<ShopController> _logger;
        private readonly JwtTokenUtil jwtTokenUtil;
        public ShopController(APIDbContext context, ILogger<ShopController> logger, JwtTokenUtil jwtTokenUtil)
        {
            _context = context;
            _logger = logger;
            this.jwtTokenUtil = jwtTokenUtil;
        }


        [HttpGet]
        public JsonResult Index()
        {
            var shops = this._context.Shops;
            return new JsonResult(shops);
        }

        [HttpGet("{id}")]
        public JsonResult Get(int id)
        {
            var shop = this._context.Shops
                .Where(s => s.Id == id)
                .FirstOrDefault();
            if (shop == null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return new JsonResult("Didn't find any shop wiht this id");
            }
            Response.StatusCode = StatusCodes.Status200OK;
            return new JsonResult(shop);
        }

        [Authorize]
        [HttpPut("{id}")]
        public JsonResult Put(int id, [FromBody] Shop bodyShop)
        {
            var shop = GetShop();
            if (shop == null)
            {
                var userAdmin = GetUser();
                if(userAdmin != null)
                {
                    if(userAdmin.Role != UserRole.ROLE_ADMIN)
                    {
                        Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return new JsonResult("U have to login to acces this route");
                    }
                }

            }
            var shopToUpdate = GetShop(id);
            if (shopToUpdate == null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return new JsonResult("shop not found");
            }
            // coudl have directly compare with id but prefer comparing objects 
            if (shop.Role != UserRole.ROLE_SHOP && shopToUpdate.Id != shop.Id)
            {
                Response.StatusCode = StatusCodes.Status403Forbidden;
                return new JsonResult("You do not have permission to edit this data, pls contact admin");
            }

            if (bodyShop.Name != null) shopToUpdate.Name = bodyShop.Name;
            if (bodyShop.Qabis != null) shopToUpdate.Qabis = bodyShop.Qabis;
            if (bodyShop.Password != null) shopToUpdate.Password = BCryptNet.HashPassword(bodyShop.Password);
            shopToUpdate.UpdatedDate = DateTime.Now;
            this._context.Shops.Update(shopToUpdate);
            _context.SaveChanges();
            Response.StatusCode = StatusCodes.Status200OK;
            return new JsonResult(shopToUpdate);
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public JsonResult Delete(int id)
        {
            var shopToDelete = _context.Shops
                .FirstOrDefault(e => e.Id == id);
            if (shopToDelete == null)
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
            if (shopToDelete != shop && shop.Role != UserRole.ROLE_ADMIN)
            {
                Response.StatusCode = StatusCodes.Status403Forbidden;
                return new JsonResult(new DeleteStatus(false));
            }
            var toRemoveAddresses = DeleteUserToDeleteShopAddressses(shopToDelete);
            foreach (var a in toRemoveAddresses)
            {
                _context.ShopAddresses.Update(a);
            }
            _context.Shops.Remove(shopToDelete);
            _context.SaveChanges();
            Response.StatusCode = StatusCodes.Status200OK;
            return new JsonResult(new DeleteStatus(true));
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
        private Shop GetShop(int id)
        {
            var shops = _context.Shops;
            foreach (var s in shops)
            {
                if (s.Id == id) return s;
            }
            return null;
        }

        private List<ShopAddress> DeleteUserToDeleteShopAddressses(Shop shop)
        {
            var shopAddresses = _context.ShopAddresses;
            var updatedShopAddresses = new List<ShopAddress>();
            foreach (var s in shopAddresses)
            {
                if (s.shop == shop)
                {
                    s.shop = null;
                    updatedShopAddresses.Add(s);
                }
            }
            return updatedShopAddresses;
        }

    }
}
