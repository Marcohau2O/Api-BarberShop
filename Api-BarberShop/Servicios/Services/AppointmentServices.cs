using Api_BarberShop.Context;
using Api_BarberShop.Model;
using Api_BarberShop.Servicios.IServices;
using Microsoft.EntityFrameworkCore;

namespace Api_BarberShop.Servicios.Services
{
    public class AppointmentServices : IAppointmentServices
    {
        private readonly AppDbContext _dbContext;

        public AppointmentServices(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> CreateAppointmentAsync(AppointmentDto appointmentDto)
        {
            var appointment = new Appointment()
            {
                Name = appointmentDto.Name,
                Phone = appointmentDto.Phone,
                Date = appointmentDto.Date,
                Time = appointmentDto.Time,
                Status = appointmentDto.Status ?? "Pendiente",
                UserId = appointmentDto.UserId,
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
            var appointment = await _dbContext.Appointments.FindAsync(id);
            if (appointment == null) return false;

            appointment.Status = status;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int id)
        {
            return await _dbContext.Appointments.FindAsync(id);
        }

        public async Task<bool> UpdateAppointmentAsync(AppointmentDyTDtp appointment)
        {
            var existingAppointment = await _dbContext.Appointments.FindAsync(appointment.Id);
            if (existingAppointment == null)
            {
                return false;
            }

            // Actualiza los campos
            existingAppointment.Date = appointment.Date;
            existingAppointment.Time = appointment.Time;

            _dbContext.Appointments.Update(existingAppointment);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
