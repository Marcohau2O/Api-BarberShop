namespace Api_BarberShop.Model
{
    public class Appointment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Status { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }
    }
}
