using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using quest_web.Contexts;
using quest_web.Models;
using quest_web.Utils;
using BCryptNet = BCrypt.Net.BCrypt;

namespace quest_web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShopAuthenticationController : ControllerBase
    {
        private readonly APIDbContext _context = null;
        private readonly ILogger<ShopAuthenticationController> _logger;
        private readonly JwtTokenUtil jwtTokenUtil;

        public ShopAuthenticationController(APIDbContext context, ILogger<ShopAuthenticationController> logger, JwtTokenUtil jwtTokenUtil)
        {
            this._context = context;
            this.jwtTokenUtil = jwtTokenUtil;
            _logger = logger;
        }

        [HttpPost("/register_shop")]
        public JsonResult Create(Shop shop)
        {
            if (ModelState.IsValid)
            {
                if (this.ShopExists(shop))
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;

                    return new JsonResult("Tentative de duplication");
                }
                if(shop.Name == null)
                {
                    Response.StatusCode = StatusCodes.Status409Conflict;

                    return new JsonResult("the field name is required");
                }
                shop.CreationDate = DateTime.Now;
                shop.Role = UserRole.ROLE_SHOP;
                shop.Password = BCryptNet.HashPassword(shop.Password);
                this._context.Shops.Add(shop);
                try
                {
                    this._context.SaveChanges();

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
            Response.StatusCode = StatusCodes.Status201Created;
            return new JsonResult("shop's registration success");
        }

        private bool ShopExists(Shop shop)
        {
            var shops = this._context.Shops;
            foreach (var s in shops)
            {
                if (s.Qabis.Trim().ToLower() == shop.Qabis.Trim().ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        [Authorize]
        [HttpGet("/me_shop")]
        public JsonResult Me()
        {
            var authorization = Request.Headers[HeaderNames.Authorization];
            if (AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
            {
                var bearer = headerValue.Parameter;

                string name = jwtTokenUtil.GetUsernameFromToken(bearer);
                //return new JsonResult(name);
                if (name.Length > 0)
                {
                    var shops = this._context.Shops;
                    foreach (var s in shops)
                    {
                        if (s.Name == name)
                        {

                            return new JsonResult(s);
                        }
                    }
                    Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return new JsonResult("Json invalide ou incorrect");

                }
            }
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult("Requête invalide");
        }

        [HttpPost("/authenticate_shop")]
        public JsonResult Login([FromBody] Shop shop)
        {
            string token;
            if (ModelState.IsValid)
            {
                var shops = this._context.Shops;

                foreach (var s in shops)
                {
                    if (s.Qabis == shop.Qabis && BCryptNet.Verify(shop.Password, s.Password))
                    {
                        token = jwtTokenUtil.GenerateShopToken(new Shop(s.Name, s.Qabis));
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


    }
}
