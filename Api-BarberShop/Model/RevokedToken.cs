using System.ComponentModel.DataAnnotations;

namespace Api_BarberShop.Model
{
    public class RevokedToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime RevokedAt { get; set; }
    }
}
