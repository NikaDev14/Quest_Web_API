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
    [Route("address_shops")]
    public class ShopAddressController : ControllerBase
    {
        private readonly APIDbContext _context = null;
        private readonly ILogger<ShopAddressController> _logger;
        private readonly JwtTokenUtil jwtTokenUtil;

        public ShopAddressController(APIDbContext context, ILogger<ShopAddressController> logger, JwtTokenUtil jwtTokenUtil)
        {
            _context = context;
            _logger = logger;
            this.jwtTokenUtil = jwtTokenUtil;
        }

        [HttpGet]
        public JsonResult Index()
        {
            var shopAddresses = this._context.ShopAddresses;
            return new JsonResult(shopAddresses);
        }

        [HttpGet("{id}")]
        public JsonResult Get(int id)
        {
            var shopAddress = this._context.ShopAddresses
                .Where(s => s.Id == id)
                .FirstOrDefault();
            if (shopAddress == null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return new JsonResult("Didn't find any shop wiht this id");
            }
            Response.StatusCode = StatusCodes.Status200OK;
            return new JsonResult(shopAddress);
        }

        [Authorize]
        [HttpPost]
        public JsonResult Post([FromBody] ShopAddress shopAddress)
        {
            if (ModelState.IsValid)
            {

                if (this.ShopAddressExists(shopAddress))
                {
                    Response.StatusCode = StatusCodes.Status409Conflict;
                    return new JsonResult("Tentative de duplication : erreur 409");
                }
                var shop = GetShop();
                if(shop == null)
                {
                    var user = GetUser();
                    if(user == null || user.Role == UserRole.ROLE_USER)
                    {
                        Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return new JsonResult("U do not have the right access");
                    }
                }
                shopAddress.CreationDate = DateTime.Now;
                shopAddress.UpdatedDate = DateTime.Now;
                if (shop != null) shopAddress.shop = shop;
                this._context.ShopAddresses.Add(shopAddress);
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
            return new JsonResult(shopAddress);
        }

        [Authorize]
        [HttpPut("{id}")]
        public JsonResult UpdateShopAdress([FromRoute] int id, [FromBody] ShopAddress shopAddress)
        {
            var shop = GetShop();
            if (shop == null)
            {
                // fixme => if null we should request the user db to check if its an admin
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return new JsonResult("U have to login to acces this route");
            }
            var shopAddrToUpdate = GetShopAddress(id);
            if (shopAddrToUpdate == null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return new JsonResult("Address not found");
            }
            if (shopAddrToUpdate.shop != shop)
            {
                Response.StatusCode = StatusCodes.Status403Forbidden;
                return new JsonResult("U cannot have access to an address which is not yours");
            }
            if (shopAddress.City != null) shopAddrToUpdate.City = shopAddress.City;
            if (shopAddress.Country != null) shopAddrToUpdate.Country = shopAddress.Country;
            if (shopAddress.PostalCode != null) shopAddrToUpdate.PostalCode = shopAddress.PostalCode;
            if (shopAddress.Road != null) shopAddrToUpdate.Road = shopAddress.Road;
            shopAddrToUpdate.UpdatedDate = DateTime.Now;
            this._context.ShopAddresses.Update(shopAddrToUpdate);
            _context.SaveChanges();
            return new JsonResult(shopAddrToUpdate);
        }

        // DELETE address/5
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<JsonResult> DeleteShopAddress(int id)
        {
            try
            {
                var shopAddrToDelete = await _context.ShopAddresses
               .FirstOrDefaultAsync(e => e.Id == id);

                if (shopAddrToDelete == null)
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
                // fixme : add a control to auhtorize admin user
                if (shopAddrToDelete.shop != shop)
                {
                    Response.StatusCode = StatusCodes.Status403Forbidden;
                    return new JsonResult(new DeleteStatus(false));
                }
                var deletedShopAddress = await Del(id);
                // if deleted addresqs not null return cle value json
                return new JsonResult(new DeleteStatus(true));
            }
            catch (Exception)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return new JsonResult(new DeleteStatus(false));
            }
        }

        private bool ShopAddressExists(ShopAddress shopAddress)
        {
            var shopAddresses = this._context.ShopAddresses;

            foreach (var a in shopAddresses)
            {
                if (a.Road == shopAddress.Road && a.PostalCode == shopAddress.PostalCode && a.City == shopAddress.City && a.Country == shopAddress.Country)
                {
                    return true;
                }
            }
            return false;
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

        private ShopAddress GetShopAddress(int id)
        {
            var shopAddresses = _context.ShopAddresses;
            foreach (var a in shopAddresses)
            {
                if (a.Id == id) return a;
            }
            return null;
        }

        private async Task<ShopAddress> Del(int id)
        {
            var result = await _context.ShopAddresses
                .FirstOrDefaultAsync(e => e.Id == id);
            if (result != null)
            {
                _context.ShopAddresses.Remove(result);
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
