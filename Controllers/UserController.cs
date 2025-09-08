using Microsoft.AspNetCore.Mvc;
using UserAuthApi.Models;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    // InMemory база даних
    private static List<User> users = new List<User>();

    // POST: api/User
    [HttpPost]
    public IActionResult AddUser([FromBody] User newUser)
    {
        if (users.Any(u => u.UserName == newUser.UserName))
            return BadRequest("Користувач вже існує");

        users.Add(newUser);
        return Ok("Користувача додано");
    }

    // GET: api/User?username=xxx&password=yyy
    [HttpGet]
    public IActionResult CheckUser([FromQuery] string username, [FromQuery] string password)
    {
        var user = users.FirstOrDefault(u => u.UserName == username);

        if (user == null)
            return NotFound("Користувач не знайдений");

        if (user.Password == password)
            return Ok("Пароль правильний");
        else
            return Unauthorized("Невірний пароль");
    }
}
