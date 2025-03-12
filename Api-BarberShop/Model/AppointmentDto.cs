namespace Api_BarberShop.Model
{
    public class AppointmentDto
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Status { get; set; }
        public int UserId { get; set; }
    }
}
