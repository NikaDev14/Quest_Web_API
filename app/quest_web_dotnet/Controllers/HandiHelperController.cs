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
    public class HandiHelperController
    {
        private readonly APIDbContext _context = null;
        private readonly ILogger<HandiHelperController> _logger;
        private readonly JwtTokenUtil jwtTokenUtil;

        public HandiHelperController(APIDbContext context, ILogger<HandiHelperController> logger, JwtTokenUtil jwtTokenUtil)
        {
            _context = context;
            _logger = logger;
            this.jwtTokenUtil = jwtTokenUtil;
        }


    }
}
