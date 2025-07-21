using Api_BarberShop.Model;

namespace Api_BarberShop.Servicios.IServices
{
    public interface IUserServices
    {
        Task<string?> Authenticate(string email, string password);

        Task<bool> RegisterUser(User user);
        Task<bool> Logout(string token);
        Task<bool> UpdateUser(int id, UpdateUserDto updatedUser);

        Task<IEnumerable<User>> GetUsers();
    }
}
