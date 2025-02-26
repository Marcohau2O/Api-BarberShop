using Api_BarberShop.Model;

namespace Api_BarberShop.Servicios.IServices
{
    public interface IUserServices
    {
        Task<string?> Authenticate(string name, string password);

        Task<bool> RegisterUser(User user);
        Task<bool> Logout(string token);
    }
}
