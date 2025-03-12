using Api_BarberShop.Context;
using Api_BarberShop.Model;
using Api_BarberShop.Servicios.IServices;
using Api_BarberShop.Servicios.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api_BarberShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentServices _appointmentService;
        //private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        //private readonly string _whatsAppToken;
        //private readonly string _phoneNumberId;

        public AppointmentController(IAppointmentServices appointmentService, IConfiguration configuration, AppDbContext dbContext)
        {
            _appointmentService = appointmentService;
            _context = dbContext;
            //_httpClient = new HttpClient();
            //_whatsAppToken = configuration["WhatsAppConfig:AccessToken"];
            //_phoneNumberId = configuration["WhatsAppConfig:PhoneNumberId"];
        }

        [HttpPost("CreateCita")]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentDto? appointmentDto)
        {
            if (appointmentDto == null)
                return BadRequest("El cuerpo de la solicitud no puede estar vacio");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _appointmentService.CreateAppointmentAsync(appointmentDto);
            if (!result)
                return StatusCode(500, "Error al crear la cita.");

            return Ok(new { message = "Cita creada exitosamente" });
        }

        [HttpGet("Count")]
        public async Task<IActionResult> GetAppointmentCount()
        {
            var count = await _appointmentService.GetAppointmentCountAsync();
            return Ok(new { totalAppointments = count });
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAllAppointments()
        {
            var appointments = await _appointmentService.GetAppointmentsAsync();
            return Ok(appointments);
        }

        [HttpPut("updateStatus/{id}")]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var success = await _appointmentService.UpdateAppointmentStatusAsync(id, request.Status);
            if (!success) return BadRequest("No se pudo actualizar el estado de la cita.");

            return Ok(new { message = "Estado actualizado correctamente." });
        }

        [HttpGet("appointment/{id}")]
        public async Task<IActionResult> GetByID(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                return NotFound("cita no encontrada");
            }
            return Ok(appointment);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] AppointmentDyTDtp appointment)
        {
            if (id != appointment.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            var updated = await _appointmentService.UpdateAppointmentAsync(appointment);
            if (!updated)
            {
                return NotFound("No se pudo actualizar la cita.");
            }

            return Ok(new { message = "Cita actualizada correctamente." });
        }

        // Enviar mensaje de WhatsApp
        //[HttpPost("sendWhatsAppMessage")]
        //public async Task<IActionResult> SendWhatsAppMessage([FromBody] WhatsAppMessageRequest request)
        //{
        //    var url = $"https://graph.facebook.com/v17.0/{_phoneNumberId}/messages";
        //    var payload = new
        //    {
        //        messaging_product = "whatsapp",
        //        to = request.Phone,
        //        type = "text",
        //        text = new { body = request.Message }
        //    };

        //    var jsonPayload = JsonSerializer.Serialize(payload);
        //    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        //    _httpClient.DefaultRequestHeaders.Clear();
        //    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _whatsAppToken);

        //    var response = await _httpClient.PostAsync(url, content);
        //    var responseContent = await response.Content.ReadAsStringAsync();

        //    if (response.IsSuccessStatusCode)
        //    {
        //        return Ok(new { message = "Mensaje enviado correctamente", response = responseContent });
        //    }

        //    return BadRequest(new { error = "Error al enviar el mensaje", details = responseContent });
        //}

        public class UpdateStatusRequest
        {
            public string Status { get; set; }
        }

        //public class WhatsAppMessageRequest
        //{
        //    public string Phone { get; set; } // Número del cliente, formato internacional: "521XXXXXXXXXX"
        //    public string Message { get; set; }
        //}
    }
}
