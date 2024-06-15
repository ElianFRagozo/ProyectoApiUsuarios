using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using ProyectoApiUsuarios.Models;
using ProyectoApiUsuarios.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;

namespace ProyectoApiUsuarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly MedicoService _medicoService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly PatientService _patientService;

        public UsersController(UserService userService, MedicoService medicoService, IConfiguration configuration, HttpClient httpClient, PatientService patientService)
        {
            _userService = userService;
            _medicoService = medicoService;
            _configuration = configuration;
            _httpClient = httpClient;
            _patientService = patientService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newId = ObjectId.GenerateNewId();
            userModel.Id = newId.ToString();

            await _userService.CreateUserAsync(userModel);

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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            if (loginModel == null || string.IsNullOrWhiteSpace(loginModel.Email) || string.IsNullOrWhiteSpace(loginModel.Password))
            {
                return BadRequest("Solicitud no válida");
            }

            var user = await _userService.ValidateUserCredentialsAsync(loginModel.Email, loginModel.Password);
            if (user == null)
            {
                return Unauthorized();
            }

            var patient = await _patientService.GetPacienteByEmailAsync(user.Email);
            if (patient == null)
            {
                return NotFound("No se encontró un paciente con el email proporcionado.");
            }

            // Asigna los roles del paciente al objeto PatientDto
            var patientDto = new PatientDto
            {
                Id = patient.Id,
                IdentificationType = patient.IdentificationType,
                IdentificationNumber = patient.IdentificationNumber,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateOfBirth = patient.DateOfBirth,
                Phone = patient.Phone,
                Email = patient.Email,
                Password = patient.Password,
                ConfirmEmail = patient.ConfirmEmail,
                ConfirmPassword = patient.ConfirmPassword,
                Roles = patient.Roles // Asegúrate de que se asignen los roles aquí
            };

            // Genera el token JWT con los roles asignados
            var token = GenerateJwtToken(user, patientDto, null, null);
            return Ok(new { token, patient = patientDto });
        }



        [HttpPost("medico-data")]
        public async Task<IActionResult> GetMedicoData([FromBody] MedicoLoginModel medicoLoginModel)
        {
            if (medicoLoginModel == null || string.IsNullOrWhiteSpace(medicoLoginModel.Email) || string.IsNullOrWhiteSpace(medicoLoginModel.Password))
            {
                return BadRequest("Solicitud no válida");
            }

            var user = await _userService.ValidateUserCredentialsAsync(medicoLoginModel.Email, medicoLoginModel.Password);
            if (user == null)
            {
                return Unauthorized();
            }

            var medico = await _medicoService.GetMedicoByEmailAsync(user.Email);
            if (medico == null)
            {
                return NotFound("No se encontró un médico con el email proporcionado.");
            }
            var medicoDto = new MedicoDto
            {
                Id = medico.Id,
                Nombre = medico.Nombre,
                Correo = medico.Email,
                Especialidad = string.Join(", ", medico.Especialidades),
                Rol = medico.Roles.Contains("admin") ? "admin" : "medico"
            };

            var token = GenerateJwtToken(user, null, medicoDto, null);
            return Ok(new { token, medico = medicoDto });
        }

        private string GenerateJwtToken(UserModel user, PatientDto patient, MedicoDto medico, Patient patient1)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim("UserId", user.Id)
    };

            if (patient != null)
            {
                claims.Add(new Claim("IdPatient", patient.Id));
                claims.Add(new Claim("IdentificationType", patient.IdentificationType));
                claims.Add(new Claim("IdentificationNumber", patient.IdentificationNumber));
                claims.Add(new Claim("FirstName", patient.FirstName));
                claims.Add(new Claim("LastName", patient.LastName));
                claims.Add(new Claim("DateOfBirth", patient.DateOfBirth.ToString("yyyy-MM-dd")));
                claims.Add(new Claim("Phone", patient.Phone));
                claims.Add(new Claim("Email", patient.Email));
                claims.Add(new Claim("ConfirmEmail", patient.ConfirmEmail));

                if (patient.Roles != null && patient.Roles.Any())
                {
                    claims.Add(new Claim("UserRole", patient.Roles.First()));
                }
                else
                {
                    claims.Add(new Claim("UserRole", "paciente")); // Asignar un rol por defecto
                }
            }

            if (medico != null)
            {
                claims.Add(new Claim("IdMedico", medico.Id));
                claims.Add(new Claim("MedicoNombre", medico.Nombre));
                claims.Add(new Claim("MedicoCorreo", medico.Correo));
                claims.Add(new Claim("MedicoEspecialidad", medico.Especialidad));
                claims.Add(new Claim("MedicoRol", medico.Rol));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        public class LoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class MedicoLoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }

    public class MedicoDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Especialidad { get; set; }
        public string Rol { get; set; }
    }
}
