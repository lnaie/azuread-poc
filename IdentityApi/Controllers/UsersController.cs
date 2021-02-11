using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace TodoListService.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        // GET: api/values
        [Authorize]
        [HttpGet("identity")]
        public IActionResult GetIdentity()
        {
            return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
        }
    }
}
