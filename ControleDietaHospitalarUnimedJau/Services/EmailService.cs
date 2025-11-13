using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using ControleDietaHospitalarUnimedJau.Settings;
using MimeKit.Text;

namespace ControleDietaHospitalarUnimedJau.Services
{
    public class EmailService
    {

        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }
        // ------------------------------------------------------------------
        // NOVO MÉTODO: O Assinante do Evento
        // ------------------------------------------------------------------
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;

                email.Body = new TextPart(TextFormat.Html) { Text = message };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Logue o erro para diagnosticar depois
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
                // Opcionalmente, lance a exceção para ser tratada mais acima
                throw;
            }
        }

    }//fim da classe

}
