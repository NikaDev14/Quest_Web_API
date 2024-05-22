using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using quest_web.Models;
namespace quest_web.Utils {
    public class JwtTokenUtil
    {
        public static readonly long JwtTokenValidity = 5 * 360;
        private static readonly string Secret = "etna_quest_jwt_secret_key";

        public static readonly TokenValidationParameters TokenValidationParameters = new TokenValidationParameters {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret)),
            ValidateLifetime = true,
        };

        public string GenerateToken(UserDetails user)
        { var claims = new[] {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            };
            return GenerateToken(claims);
        }

        public string GenerateToken(Claim[] claims)
        {
            var jwtToken = new JwtSecurityToken(claims: claims, expires: DateTime.Now.AddSeconds(JwtTokenValidity), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret)), SecurityAlgorithms.HmacSha512Signature));
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        public (ClaimsPrincipal, JwtSecurityToken) DecodeToken(string token)
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, TokenValidationParameters, out var validatedToken);
            return (principal, validatedToken as JwtSecurityToken);
        }

        public string  GenerateShopToken(Shop shop)
        {
            var claims = new[] {
            new Claim(ClaimTypes.Name, shop.Qabis),
            new Claim(ClaimTypes.Role, shop.Role.ToString()),
            };
            return GenerateToken(claims);
        }


        public DateTime GetExpirationDateFromToken(string token)
        {
            var (_, jwt) = DecodeToken(token);
            return jwt.ValidTo;
        }

        public string GetUsernameFromToken(string token)
        {
            var (_, jwt) = DecodeToken(token);
            return jwt.Claims.First(claim => claim.Type == ClaimTypes.Name).Value;
        }

        public bool IsTokenExpired(string token)
        {
            return DateTime.Now < GetExpirationDateFromToken(token);
        }

        public bool ValidateToken(string token, UserDetails details)
        {
            return GetUsernameFromToken(token).Equals(details.Username);
        }
    }
}