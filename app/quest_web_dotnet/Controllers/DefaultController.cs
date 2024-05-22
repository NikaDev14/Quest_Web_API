using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace quest_web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DefaultController : Controller
    {

        private readonly ILogger<DefaultController> _logger;

        public DefaultController(ILogger<DefaultController> logger)
        {
            _logger = logger;
        }

        [HttpGet("/testSuccess")]
        public string testSuccess()
        {
            string message = "success";

            Response.StatusCode = 200;
            return message;
        }

        [HttpGet("/testNotFound")]
        public string PageNotFound()
        {
            string message = "not found";

            Response.StatusCode = 404;
            return message;
        }


        [HttpGet("/testError")]
        public string testError(int id)
        {
            string message = "error";

            Response.StatusCode = 500;
            return message;
        }

    }
}
