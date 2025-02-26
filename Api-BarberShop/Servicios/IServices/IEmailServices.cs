namespace Api_BarberShop.Servicios.IServices
{
    public interface IEmailServices
    {
        Task SendPasswordResetEmail(string email, string token);
    }
}
