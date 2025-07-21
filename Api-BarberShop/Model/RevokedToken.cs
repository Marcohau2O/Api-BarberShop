using System.ComponentModel.DataAnnotations;

namespace Api_BarberShop.Model
{
    public class UserToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int UserId { get; set; }
    }
}
