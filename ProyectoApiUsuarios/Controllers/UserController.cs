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

         public UsersController(UserService userService, MedicoService medicoService, IConfiguration configuration, HttpClient httpClient)
        {
            _userService = userService;
            _medicoService = medicoService;
            _configuration = configuration;
            _httpClient = httpClient;
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
            if (loginModel == null)
            {
                return BadRequest("Invalid client request");
            }

            var user = await _userService.ValidateUserCredentialsAsync(loginModel.Email, loginModel.Password);
            if (user == null)
            {
                return Unauthorized();
            }

            var patientServiceUrl = $"{Environment.GetEnvironmentVariable("PATIENT_SERVICE_BASE_URL")}?userId={user.Id}";

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(patientServiceUrl);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here as needed
                return StatusCode(500, "Error al enviar la solicitud al servicio de pacientes.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error al obtener el paciente asociado al usuario.");
            }

            var patientDataJson = await response.Content.ReadAsStringAsync();
            var patient = JsonConvert.DeserializeObject<List<PatientDto>>(patientDataJson);


            if (patient == null)
            {
                return StatusCode(500, "No se encontró un paciente asociado con el usuario.");
            }

            var patientUserId = patient.FirstOrDefault()?.UserId;
            if (patientUserId == null)
            {
                return StatusCode(500, "No se encontró el UserId del paciente asociado con el usuario.");
            }

            // Aquí deberías usar el patientUserId obtenido de la respuesta del servicio de pacientes
            var patientInfoServiceUrl = $"{Environment.GetEnvironmentVariable("PATIENT_SERVICE_BASE_URL")}?userId={patientUserId}";

            Console.WriteLine($"URL de la solicitud: {patientServiceUrl}");

            try
            {
                response = await _httpClient.GetAsync(patientServiceUrl);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here as needed
                return StatusCode(500, "Error al enviar la solicitud al servicio de pacientes.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error al obtener los datos del paciente.");
            }

            // Lee la respuesta del servicio de pacientes y registra el contenido
            var patientDataJsons = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Datos del paciente recibidos: {patientDataJson}");

            PatientDto[] patients = null;
            try
            {
                // Deserializa la respuesta del servicio de pacientes en un array de PatientDto
                patients = JsonConvert.DeserializeObject<PatientDto[]>(patientDataJson);
            }
            catch (JsonException ex)
            {
                // Maneja los errores de deserialización
                Console.WriteLine($"Error al deserializar los datos del paciente: {ex.Message}");
                return StatusCode(500, "Error al deserializar los datos del paciente.");
            }

            // Verifica si se encontraron pacientes en la respuesta
            if (patients == null || patients.Length == 0)
            {
                // Maneja el caso en el que no se encontraron pacientes
                return StatusCode(500, "No se encontraron pacientes en la respuesta.");
            }

            // Encuentra el paciente asociado al usuario actual
            var patientList = JsonConvert.DeserializeObject<List<PatientDto>>(patientDataJson);
            if (patientList == null)
            {
                // Maneja el caso en el que no se encontró un paciente asociado con el usuario
                return StatusCode(500, "No se encontró un paciente asociado con el usuario.");
            }

            var patientToUse = patientList.FirstOrDefault();

            // Genera el token JWT con el usuario y el paciente asociado
            var token = GenerateJwtToken(user, patientToUse, null);
            return Ok(new { token, patient = patientToUse });
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
                Nombre = medico.Nombre,
                Correo = medico.Email,
                Especialidad = string.Join(", ", medico.Especialidades),
                Rol = medico.Roles.Contains("admin") ? "admin" : "medico"
            };

            var token = GenerateJwtToken(user, null, medicoDto);
            return Ok(new { token, medico = medicoDto });
        }

        private string GenerateJwtToken(UserModel user, PatientDto patient, MedicoDto medico)
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
                claims.Add(new Claim("IdentificationType", patient.IdentificationType));
                claims.Add(new Claim("IdentificationNumber", patient.IdentificationNumber));
                claims.Add(new Claim("FirstName", patient.FirstName));
                claims.Add(new Claim("LastName", patient.LastName));
                claims.Add(new Claim("DateOfBirth", patient.DateOfBirth.ToString("yyyy-MM-dd")));
                claims.Add(new Claim("Phone", patient.Phone));
                claims.Add(new Claim("Email", patient.Email));
                claims.Add(new Claim("UserId", patient.UserId));
            }

            if (medico != null)
            {
                claims.Add(new Claim("MedicoNombre", medico.Nombre));
                claims.Add(new Claim("MedicoCorreo", medico.Correo));
                claims.Add(new Claim("MedicoEspecialidad", medico.Especialidad));
                claims.Add(new Claim("MedicoRol", medico.Rol));
            }

            var userRoles = _userService.GetUserRolesAsync(user.Id).Result;
            if (userRoles.Any())
            {
                string userRole = userRoles.First(); // Tomamos el primer rol de la lista
                claims.Add(new Claim("UserRole", userRole));
            }
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
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

    public class MedicoDto
    {
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Especialidad { get; set; }
        public string Rol { get; set; }
    }
}

