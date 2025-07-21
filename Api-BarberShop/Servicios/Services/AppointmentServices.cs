using Api_BarberShop.Context;
using Api_BarberShop.Model;
using Api_BarberShop.Servicios.IServices;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Api_BarberShop.Servicios.Services
{
    public class AppointmentServices : IAppointmentServices
    {
        private readonly AppDbContext _dbContext;
        private readonly string _whatsAppToken;
        private readonly string _phoneNumberId;
        private readonly HttpClient _httpClient;

        public AppointmentServices(AppDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _httpClient = new HttpClient();
            _whatsAppToken = configuration["WhatsAppConfig:AccessToken"];
            _phoneNumberId = configuration["WhatsAppConfig:PhoneNumberId"];
        }

        public async Task<bool> CreateAppointmentAsync(AppointmentDto appointmentDto)
        {
            var appointment = new Appointment()
            {
                Name = appointmentDto.Name,
                Phone = appointmentDto.Phone,
                Date = DateTime.SpecifyKind(appointmentDto.Date, DateTimeKind.Utc),
                Time = appointmentDto.Time,
                Status = appointmentDto.Status ?? "Pendiente",
                UserId = appointmentDto.UserId,
                DoctorId = appointmentDto.DoctorId,
            };

            _dbContext.Appointments.Add(appointment);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        public async Task<int> GetAppointmentCountAsync()
        {
            return await _dbContext.Appointments.CountAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsAsync()
        {
            var appointments = await _dbContext.Appointments
                .Select(a => new Appointment
                {
                    Id = a.Id,
                    Name = a.Name,
                    Date = a.Date,
                    Phone = a.Phone,
                    Time = a.Time,
                    Status = a.Status
                })
                .ToListAsync();

            return appointments;
        }

        public async Task<bool> UpdateAppointmentStatusAsync(int id, string status)
        {
            var appointment = await _dbContext.Appointments
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return false;

            appointment.Status = status;
            await _dbContext.SaveChangesAsync();

            // Mensaje personalizado
            string message = status switch
            {
                "Aceptado" => $"Hola {appointment.Name}, tu cita ha sido *aceptada* por el doctor.",
                "Rechazado" => $"Hola {appointment.Name}, lamentamos informarte que tu cita fue *rechazada*. Por favor agenda otra fecha.",
                _ => $"Hola {appointment.Name}, el estado de tu cita ha sido actualizado a: {status}"
            };

            await SendWhatsAppMessageAsync(appointment.Phone, message);

            return true;
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int id)
        {
            return await _dbContext.Appointments.FindAsync(id);
        }

        public async Task<bool> UpdateAppointmentAsync(AppointmentDyTDtp appointment)
        {
            var existingAppointment = await _dbContext.Appointments
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == appointment.Id);

            if (existingAppointment == null) return false;

            // Guardar vieja hora (opcional)
            var oldDate = existingAppointment.Date;
            var oldTime = existingAppointment.Time;

            // Actualiza campos
            existingAppointment.Date = appointment.Date;
            existingAppointment.Time = appointment.Time;

            _dbContext.Appointments.Update(existingAppointment);
            await _dbContext.SaveChangesAsync();

            // Enviar notificación
            var newDateTime = $"{appointment.Date:dd/MM/yyyy} a las {appointment.Time:hh\\:mm}";
            var message = $"Hola {existingAppointment.Name}, tu cita ha sido *reprogramada* para el día {newDateTime}. Por favor confirma tu disponibilidad.";
            await SendWhatsAppMessageAsync(existingAppointment.Phone, message);

            return true;
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            var appointment = await _dbContext.Appointments.FindAsync(id);
            if (appointment == null) return false;

            _dbContext.Appointments.Remove(appointment);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task SendWhatsAppMessageAsync(string phone, string message)
        {
            var url = $"https://graph.facebook.com/v17.0/{_phoneNumberId}/messages";
            var payload = new
            {
                messaging_product = "whatsapp",
                to = phone,
                type = "text",
                text = new { body = message }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _whatsAppToken);

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Error al enviar WhatsApp: {responseContent}");
            }
        }

    }
}
