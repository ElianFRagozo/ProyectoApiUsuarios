using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProyectoApiUsuarios.Models;
using ProyectoApiUsuarios.Services;
using MongoDB.Bson;


namespace ProyectoApiUsuarios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly PatientService _patientService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;

        private readonly List<string> _validIdentificationTypes = new List<string>
        {
            "Cédula de Ciudadanía",
            "Tarjeta de Identidad",
            "Cédula de Extranjería",
            "Pasaporte"
        };

        public PatientsController(PatientService patientService, HttpClient httpClient, IConfiguration configuration, UserService userService)
        {
            _patientService = patientService;
            _httpClient = httpClient;
            _configuration = configuration;
            _userService = userService;
        }

        private static List<Patient> patients = new List<Patient>();

        [HttpPost("patients")]
        public async Task<IActionResult> CreatePatientAndUser([FromBody] PatientDto patientDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newId = ObjectId.GenerateNewId();
            var patientRole = new List<string> { "paciente" }; // Definir el rol de "paciente"

            // Crear el UserModel utilizando los datos del PatientDto
            var newUser = new UserModel
            {
                Id = newId.ToString(),
                Email = patientDto.Email,
                Password = patientDto.Password,
                Roles = patientRole
            };

            var newPatient = new Patient
            {
                Id = newId.ToString(),
                IdentificationType = patientDto.IdentificationType,
                IdentificationNumber = patientDto.IdentificationNumber,
                FirstName = patientDto.FirstName,
                LastName = patientDto.LastName,
                DateOfBirth = patientDto.DateOfBirth,
                Phone = patientDto.Phone,
                Email = patientDto.Email,
                Password = patientDto.Password,
                ConfirmEmail = patientDto.ConfirmEmail,
                ConfirmPassword = patientDto.ConfirmPassword,
                Roles = patientRole
            };

            // Guardar el nuevo usuario y el nuevo paciente en la base de datos
            await _userService.CreateUserAsync(newUser);
            await _patientService.CreatePatientAsync(newPatient);

            // Devolver una respuesta adecuada
            return CreatedAtAction(nameof(GetPatientById), new { id = newPatient.Id }, newPatient);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(string id, [FromBody] UpdatePatientDto updatePatientDto)
        {
            var patientToUpdate = await _patientService.GetPatientAsync(id.ToString());
            if (patientToUpdate == null)
            {
                return NotFound();
            }

            // Validaciones (por ejemplo, verificar el tipo de identificación)
            if (!_validIdentificationTypes.Contains(updatePatientDto.IdentificationType))
            {
                return BadRequest("Tipo de identificación no válido.");
            }

            // Actualizar las propiedades del paciente con los valores del DTO recibido
            patientToUpdate.IdentificationType = updatePatientDto.IdentificationType;
            patientToUpdate.IdentificationNumber = updatePatientDto.IdentificationNumber;
            patientToUpdate.FirstName = updatePatientDto.FirstName;
            patientToUpdate.LastName = updatePatientDto.LastName;
            patientToUpdate.DateOfBirth = updatePatientDto.DateOfBirth;
            patientToUpdate.Phone = updatePatientDto.Phone;

            // Llamar al servicio para actualizar el paciente en la base de datos
            await _patientService.UpdatePatientAsync(id.ToString(), patientToUpdate);

            return NoContent();
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientById(string id)
        {
            // Validar que el ID es un ObjectId válido
            if (!ObjectId.TryParse(id, out _))
            {
                return BadRequest("Invalid ID format.");
            }

            var patient = await _patientService.GetPatientAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            return Ok(patient);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _patientService.GetPatientsAsync();
            return Ok(patients);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPatientByUserId(string userId)
        {
            var patient = await _patientService.GetPatientByUserIdAsync(userId);
            if (patient == null)
            {
                return NotFound();
            }

            return Ok(patient);
        }
    }
}