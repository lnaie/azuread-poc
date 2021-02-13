
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using TodoListService.Models;

namespace TodoListService.Controllers
{
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        static readonly ConcurrentBag<TodoItem> TodoStore = new ConcurrentBag<TodoItem>()
        {
            new TodoItem
            {
                Title = "Buy pizza ingredients from supermarket",
                Owner = "UserA"
            },
            new TodoItem
            {
                Title = "Watch American Gods TV show",
                Owner = "UserB"
            }
        };

        // GET: api/values
        [Authorize (Policy = "AzureADUser", Roles = "admin")]
        [Authorize(Policy = "IDSUser", Roles = "admin")]
        [HttpGet("all")]
        public IEnumerable<TodoItem> GetAll()
        {
            return TodoStore;
        }

        // POST api/values
        [Authorize (Policy = "AzureADUser")]
        [Authorize(Policy = "IDSUser")]
        [HttpPost]
        public void Post([FromBody]TodoItem todo)
        {
            string owner = User.FindFirst(ClaimTypes.Name)?.Value;
            TodoStore.Add(new TodoItem { Owner = owner, Title = todo.Title });
        }
    }
}
