namespace Api_BarberShop.Model
{
    public class ResetPasswordDto
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string NewConfirmPassword { get; set; }
    }
}
