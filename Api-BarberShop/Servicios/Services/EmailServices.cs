using Api_BarberShop.Servicios.IServices;
using System.Net;
using System.Net.Mail;

namespace Api_BarberShop.Servicios.Services
{
    public class EmailServices : IEmailServices
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailServices(string smtpServer, int smtpPort, string smtpUsername, string smtpPassword)
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpUsername = smtpUsername;
            _smtpPassword = smtpPassword;
        }
        public async Task SendPasswordResetEmail(string email, string token)
        {
            var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true
            };

            var from = new MailAddress("barbershop9087@gmail.com", "BarberShop");
            var to = new MailAddress(email);
            var subject = "Restablecimiento de Contraseña";
            var resetLink = $"http://localhost:5173/reset-password/{token}";
            var body = $"<strong>Haga clic en el siguiente enlace para restablecer su contraseña:</strong> <a href='{resetLink}'>Restablecer Contraseña</a>";

            var mailMassege = new MailMessage
            {
                From = from,
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMassege.To.Add(to);

            await client.SendMailAsync(mailMassege);
        }
    }

}
