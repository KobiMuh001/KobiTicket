using KobiMuhendislikTicket.Application.Interfaces;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KobiMuhendislikTicket.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // SMTP ayarlarını yapılandırmadan al
            _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
            _fromEmail = _configuration["EmailSettings:FromEmail"] ?? "";
            _fromName = _configuration["EmailSettings:FromName"] ?? "Kobi Mühendislik Ticket";
            _enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                // SMTP yapılandırması eksikse e-posta gönderme
                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    _logger.LogWarning("E-posta ayarları yapılandırılmamış. E-posta gönderilemedi: {ToEmail}", toEmail);
                    return;
                }

                using var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_fromEmail, _fromName);
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = isHtml;

                using var smtpClient = new SmtpClient(_smtpHost, _smtpPort);
                smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                smtpClient.EnableSsl = _enableSsl;

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("E-posta başarıyla gönderildi: {ToEmail}, Konu: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta gönderilirken hata oluştu: {ToEmail}, Konu: {Subject}", toEmail, subject);
                // Hata fırlatmıyoruz çünkü e-posta gönderilemese bile uygulama çalışmaya devam etmeli
            }
        }

        public async Task SendTicketAssignmentEmailAsync(string toEmail, string staffName, string ticketTitle, string tenantName, int ticketId)
        {
            var subject = "Size Yeni Bir Ticket Atandı";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                        .content {{ background-color: #f9f9f9; padding: 20px; border: 1px solid #ddd; }}
                        .footer {{ background-color: #f1f1f1; padding: 10px; text-align: center; font-size: 12px; color: #666; border-radius: 0 0 5px 5px; }}
                        .button {{ display: inline-block; padding: 10px 20px; background-color: #2196F3; color: white; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
                        .info {{ background-color: #e3f2fd; padding: 15px; border-left: 4px solid #2196F3; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>🎫 Yeni Ticket Ataması</h2>
                        </div>
                        <div class='content'>
                            <p>Merhaba <strong>{staffName}</strong>,</p>
                            <p>Size yeni bir destek talebi atandı:</p>
                            <div class='info'>
                                <strong>Ticket:</strong> {ticketTitle}<br>
                                <strong>Firma:</strong> {tenantName}<br>
                                <strong>Ticket ID:</strong> {ticketId}
                            </div>
                            <p>Lütfen en kısa sürede bu ticket'ı inceleyip gerekli işlemleri yapınız.</p>
                            
                        </div>
                        <div class='footer'>
                            <p>Bu e-posta Kobi Mühendislik Ticket Sistemi tarafından otomatik olarak gönderilmiştir.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body, true);
        }

        public async Task SendNewCommentEmailAsync(string toEmail, string staffName, string ticketTitle, string authorName, string commentPreview, int ticketId)
        {
            var subject = "Ticket'ınıza Yeni Yorum Eklendi";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                        .content {{ background-color: #f9f9f9; padding: 20px; border: 1px solid #ddd; }}
                        .footer {{ background-color: #f1f1f1; padding: 10px; text-align: center; font-size: 12px; color: #666; border-radius: 0 0 5px 5px; }}
                        .button {{ display: inline-block; padding: 10px 20px; background-color: #2196F3; color: white; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
                        .comment {{ background-color: #fff; padding: 15px; border-left: 4px solid #2196F3; margin: 15px 0; font-style: italic; }}
                        .info {{ background-color: #e3f2fd; padding: 15px; border-left: 4px solid #2196F3; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>💬 Yeni Yorum</h2>
                        </div>
                        <div class='content'>
                            <p>Merhaba <strong>{staffName}</strong>,</p>
                            <p>Yetkili olduğunuz ticket'a yeni bir yorum eklendi:</p>
                            <div class='info'>
                                <strong>Ticket:</strong> {ticketTitle}<br>
                                <strong>Yorum Yapan:</strong> {authorName}<br>
                                <strong>Ticket ID:</strong> {ticketId}
                            </div>
                            <div class='comment'>
                                <strong>Yorum:</strong><br>
                                {commentPreview}
                            </div>
                            <p>Yanıtlamak için lütfen sisteme giriş yapın.</p>
                            
                        </div>
                        <div class='footer'>
                            <p>Bu e-posta Kobi Mühendislik Ticket Sistemi tarafından otomatik olarak gönderilmiştir.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body, true);
        }

        public async Task SendNewTicketEmailToAdminAsync(string ticketTitle, string tenantName, string priority, string description, int ticketId)
        {
            // Admin email'ini configuration'dan al
            var adminEmail = _configuration["Admin:Email"];
            if (string.IsNullOrEmpty(adminEmail))
            {
                _logger.LogWarning("Admin email yapılandırılmamış. Yeni ticket bildirimi gönderilemedi.");
                return;
            }

            var subject = "🎫 Yeni Destek Talebi Oluşturuldu";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                        .content {{ background-color: #f9f9f9; padding: 20px; border: 1px solid #ddd; }}
                        .footer {{ background-color: #f1f1f1; padding: 10px; text-align: center; font-size: 12px; color: #666; border-radius: 0 0 5px 5px; }}
                        .button {{ display: inline-block; padding: 10px 20px; background-color: #2196F3; color: white; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
                        .ticket-info {{ background-color: #e3f2fd; padding: 15px; border-left: 4px solid #2196F3; margin: 15px 0; }}
                        .priority {{ display: inline-block; padding: 5px 10px; border-radius: 3px; font-weight: bold; color: white; }}
                        .priority-high {{ background-color: #f44336; }}
                        .priority-medium {{ background-color: #ff9800; }}
                        .priority-low {{ background-color: #4caf50; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>🆕 Yeni Destek Talebi</h2>
                        </div>
                        <div class='content'>
                            <p>Merhaba <strong>Admin</strong>,</p>
                            <p>Yeni bir destek talebi oluşturuldu:</p>
                            <div class='ticket-info'>
                                <strong>Firma:</strong> {tenantName}<br>
                                <strong>Başlık:</strong> {ticketTitle}<br>
                                <strong>Öncelik:</strong> <span class='priority priority-{priority.ToLower()}'>{priority}</span><br>
                                <strong>Ticket ID:</strong> {ticketId}
                            </div>
                            <div style='background-color: #fff; padding: 15px; border: 1px solid #ddd; margin: 15px 0;'>
                                <strong>Açıklama:</strong><br>
                                {description}
                            </div>
                            <p>Lütfen bu talebi en kısa sürede değerlendirip uygun bir personele atayınız.</p>
                            
                        </div>
                        <div class='footer'>
                            <p>Bu e-posta Kobi Mühendislik Ticket Sistemi tarafından otomatik olarak gönderilmiştir.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(adminEmail, subject, body, true);
        }
    }
}
