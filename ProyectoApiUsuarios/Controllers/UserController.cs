// UsersController.cs
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using ProyectoApiUsuarios.Models;
using ProyectoApiUsuarios.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoApiUsuarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Generar un nuevo ObjectId
            var newId = ObjectId.GenerateNewId();

            // Asignar el nuevo ObjectId al campo Id del UserModel
            userModel.Id = newId.ToString();

            // Crear el usuario en la base de datos
            await _userService.CreateUserAsync(userModel);

            // Retornar una respuesta con el nuevo usuario creado
            return CreatedAtAction(nameof(GetUserByEmail), new { email = userModel.Email }, userModel);
        }


        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }
    }
}
