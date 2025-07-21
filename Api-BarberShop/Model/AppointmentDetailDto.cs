namespace Api_BarberShop.Model
{
    public class AppointmentDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Status { get; set; }
        public string UserName { get; set; } // paciente
    }

}
