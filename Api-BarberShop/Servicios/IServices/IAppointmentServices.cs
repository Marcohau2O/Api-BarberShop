using Api_BarberShop.Model;

namespace Api_BarberShop.Servicios.IServices
{
    public interface IAppointmentServices
    {
        Task<bool> CreateAppointmentAsync(AppointmentDto appointmentDto);
        Task<int> GetAppointmentCountAsync();
        Task<bool> UpdateAppointmentStatusAsync(int id, string status);
        Task<List<Appointment>> GetAppointmentsAsync();
        Task<Appointment> GetAppointmentByIdAsync(int id);
        Task<bool> UpdateAppointmentAsync(AppointmentDyTDtp appointment);
    }
}
