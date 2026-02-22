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
            
            // Logonuzun internet üzerindeki tam URL'ini buraya yazın
            var logoUrl = "https://www.kobimuhendislik.com/assets/images/logo/KobiLogo.png"; 

            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
                        .container {{ max-width: 600px; margin: 20px auto; padding: 0; border: 1px solid #eee; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.05); }}
                        .header {{ background-color: #ffffff; padding: 25px; text-align: center; border-bottom: 3px solid #2196F3; }}
                        .logo {{ max-width: 180px; height: auto; margin-bottom: 10px; }}
                        .content {{ background-color: #ffffff; padding: 30px; }}
                        .welcome-text {{ font-size: 18px; color: #2c3e50; }}
                        .info-box {{ background-color: #f8fbfe; border: 1px solid #d1e9ff; border-radius: 8px; padding: 20px; margin: 20px 0; }}
                        .info-item {{ margin-bottom: 8px; font-size: 15px; }}
                        .info-label {{ font-weight: bold; color: #2196F3; width: 80px; display: inline-block; }}
                        .footer {{ background-color: #f9f9f9; padding: 20px; text-align: center; font-size: 12px; color: #888; border-top: 1px solid #eee; }}
                        .button {{ display: inline-block; padding: 12px 25px; background-color: #2196F3; color: #ffffff !important; text-decoration: none; border-radius: 5px; font-weight: bold; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <img src='{logoUrl}' alt='Kobi Mühendislik Logo' class='logo'>
                            <h2 style='margin:10px 0 0 0; color: #2196F3;'>Yeni Ticket Ataması</h2>
                        </div>
                        <div class='content'>
                            <p class='welcome-text'>Merhaba <strong>{staffName}</strong>,</p>
                            <p>Sistem üzerinden size yeni bir teknik destek talebi yönlendirildi. Detaylar aşağıdadır:</p>
                            
                            <div class='info-box'>
                                <div class='info-item'><span class='info-label'>Ticket:</span> {ticketTitle}</div>
                                <div class='info-item'><span class='info-label'>Firma:</span> {tenantName}</div>
                                <div class='info-item'><span class='info-label'>ID:</span> #{ticketId}</div>
                            </div>

                            <p>Lütfen talebi en kısa sürede kontrol ederek süreçle ilgili güncellemeleri sisteme giriniz.</p>
                            
                            
                        </div>
                        <div class='footer'>
                            <p><strong>Kobi Mühendislik</strong><br>
                            Bu e-posta otomatik olarak oluşturulmuştur, lütfen yanıtlamayınız.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body, true);
        }
        
        public async Task SendNewCommentEmailAsync(string toEmail, string staffName, string ticketTitle, string authorName, string commentPreview, int ticketId)
        {
            var subject = "Ticket'ınıza Yeni Yorum Eklendi";
            
            // Logonuzun URL'ini buraya ekleyin
            var logoUrl = "https://www.kobimuhendislik.com/assets/images/logo/KobiLogo.png"; 

            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #444; margin: 0; padding: 0; }}
                        .container {{ max-width: 600px; margin: 20px auto; padding: 0; border: 1px solid #eee; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.05); }}
                        .header {{ background-color: #ffffff; padding: 25px; text-align: center; border-bottom: 3px solid #4CAF50; }}
                        .logo {{ max-width: 180px; height: auto; margin-bottom: 10px; }}
                        .content {{ background-color: #ffffff; padding: 30px; }}
                        .welcome-text {{ font-size: 18px; color: #2c3e50; }}
                        .info-box {{ background-color: #f8f9fa; border-radius: 8px; padding: 15px; margin: 20px 0; border-left: 4px solid #4CAF50; }}
                        .comment-card {{ background-color: #f0f4f8; border-radius: 8px; padding: 20px; margin: 20px 0; font-style: italic; position: relative; }}
                        .comment-author {{ font-weight: bold; color: #2e7d32; display: block; margin-bottom: 8px; font-style: normal; }}
                        .footer {{ background-color: #f9f9f9; padding: 20px; text-align: center; font-size: 12px; color: #888; border-top: 1px solid #eee; }}
                        .button {{ display: inline-block; padding: 12px 25px; background-color: #4CAF50; color: #ffffff !important; text-decoration: none; border-radius: 5px; font-weight: bold; margin-top: 10px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <img src='{logoUrl}' alt='Kobi Mühendislik Logo' class='logo'>
                            <h2 style='margin:10px 0 0 0; color: #4CAF50;'>💬 Yeni Yorum Bildirimi</h2>
                        </div>
                        <div class='content'>
                            <p class='welcome-text'>Merhaba <strong>{staffName}</strong>,</p>
                            <p>Takip ettiğiniz <strong>#{ticketId}</strong> numaralı talebe yeni bir güncelleme eklendi:</p>
                            
                            <div class='info-box'>
                                <strong>Konu:</strong> {ticketTitle}<br>
                                <strong>Güncelleyen:</strong> {authorName}
                            </div>

                            <div class='comment-card'>
                                <span class='comment-author'>{authorName} yazdı:</span>
                                ""{commentPreview}""
                            </div>
                            
                            
                        </div>
                        <div class='footer'>
                            <p><strong>Kobi Mühendislik Ticket Sistemi</strong><br>
                            Bu bildirim otomatik olarak gönderilmiştir.</p>
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

            var subject = "Yeni Destek Talebi Oluşturuldu";
            
            // Logonuzun URL'ini buraya ekleyin
            var logoUrl = "https://www.kobimuhendislik.com/assets/images/logo/KobiLogo.png"; 

            // Öncelik rengini belirle
            string priorityColor = priority.ToLower() switch
            {
                "high" or "yüksek" => "#f44336",
                "medium" or "orta" => "#ff9800",
                _ => "#4caf50"
            };

            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
                        .container {{ max-width: 600px; margin: 20px auto; padding: 0; border: 1px solid #eee; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.1); }}
                        .header {{ background-color: #ffffff; padding: 25px; text-align: center; border-bottom: 4px solid #2196F3; }}
                        .logo {{ max-width: 180px; height: auto; margin-bottom: 10px; }}
                        .content {{ background-color: #ffffff; padding: 30px; }}
                        .admin-alert {{ color: #2196F3; font-size: 20px; font-weight: bold; margin-bottom: 20px; display: block; }}
                        .ticket-card {{ background-color: #f8fbfe; border: 1px solid #d1e9ff; border-radius: 8px; padding: 20px; margin-bottom: 20px; }}
                        .info-row {{ margin-bottom: 10px; font-size: 15px; border-bottom: 1px solid #eef2f5; padding-bottom: 5px; }}
                        .info-label {{ font-weight: bold; color: #555; width: 100px; display: inline-block; }}
                        .priority-badge {{ padding: 4px 12px; border-radius: 20px; color: white; font-size: 12px; font-weight: bold; text-transform: uppercase; background-color: {priorityColor}; }}
                        .description-box {{ background-color: #fff; border: 1px dashed #ccc; padding: 15px; margin-top: 15px; border-radius: 5px; font-style: italic; color: #666; }}
                        .footer {{ background-color: #f9f9f9; padding: 20px; text-align: center; font-size: 12px; color: #888; border-top: 1px solid #eee; }}
                        .button {{ display: inline-block; padding: 12px 25px; background-color: #2196F3; color: #ffffff !important; text-decoration: none; border-radius: 5px; font-weight: bold; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <img src='{logoUrl}' alt='Kobi Mühendislik Logo' class='logo'>
                            <h2 style='margin:10px 0 0 0; color: #2196F3;'>Sistem Bildirimi</h2>
                        </div>
                        <div class='content'>
                            <span class='admin-alert'>Yeni Destek Talebi</span>
                            <p>Sisteme yeni bir talep düştü. Lütfen personellere atama yapmak için paneli kontrol edin.</p>
                            
                            <div class='ticket-card'>
                                <div class='info-row'><span class='info-label'>Ticket ID:</span> #{ticketId}</div>
                                <div class='info-row'><span class='info-label'>Firma:</span> {tenantName}</div>
                                <div class='info-row'><span class='info-label'>Başlık:</span> {ticketTitle}</div>
                                <div class='info-row'><span class='info-label'>Öncelik:</span> <span class='priority-badge'>{priority}</span></div>
                                
                                <div class='description-box'>
                                    <strong>Açıklama:</strong><br>
                                    {description}
                                </div>
                            </div>

                            
                        </div>
                        <div class='footer'>
                            <p><strong>Kobi Mühendislik Yönetim Paneli</strong><br>
                            Bu e-posta sadece yöneticilere gönderilir.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(adminEmail, subject, body, true);
        }

        public async Task SendTicketCreatedConfirmationEmailAsync(string toEmail, string toName, string ticketTitle, string ticketCode, int ticketId)
        {
            if (string.IsNullOrEmpty(toEmail))
            {
                _logger.LogWarning("Müşteri e-posta adresi bulunamadı. Ticket onay e-postası gönderilemedi: {TicketId}", ticketId);
                return;
            }

            var subject = "Destek Talebiniz Alındı";
            var logoUrl = "https://www.kobimuhendislik.com/assets/images/logo/KobiLogo.png";

            // Frontend base url (opsiyonel, appsettings içinde yoksa '#' bırakılır)
            var portalBase = _configuration["FrontEnd:BaseUrl"] ?? "#";
            var ticketUrl = portalBase != "#" ? $"{portalBase.TrimEnd('/')}/customer/tickets/{ticketId}" : "#";

            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
                        .container {{ max-width: 600px; margin: 20px auto; padding: 0; border: 1px solid #eee; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.05); }}
                        .header {{ background-color: #ffffff; padding: 25px; text-align: center; border-bottom: 3px solid #2196F3; }}
                        .logo {{ max-width: 160px; height: auto; margin-bottom: 10px; }}
                        .content {{ background-color: #ffffff; padding: 30px; }}
                        .ticket-info {{ background-color: #f8fbfe; border: 1px solid #d1e9ff; border-radius: 8px; padding: 20px; margin: 20px 0; }}
                        .footer {{ background-color: #f9f9f9; padding: 20px; text-align: center; font-size: 12px; color: #888; border-top: 1px solid #eee; }}
                        .button {{ display: inline-block; padding: 12px 25px; background-color: #2196F3; color: #ffffff !important; text-decoration: none; border-radius: 5px; font-weight: bold; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <img src='{logoUrl}' alt='Kobi Mühendislik Logo' class='logo'>
                            <h2 style='margin:10px 0 0 0; color: #2196F3;'>Destek Talebiniz Alındı</h2>
                        </div>
                        <div class='content'>
                            <p>Merhaba <strong>{toName}</strong>,</p>
                            <p>Talebiniz tarafımıza başarıyla ulaşmıştır. Aşağıda talebin temel bilgileri yer almaktadır:</p>

                            <div class='ticket-info'>
                                <div><strong>Başlık:</strong> {ticketTitle}</div>
                                <div style='margin-top:8px;'><strong>Takip Kodu:</strong> {ticketCode ?? $"T{ticketId:D5}"}</div>
                                <div style='margin-top:8px;'><strong>ID:</strong> #{ticketId}</div>
                            </div>

                            {(ticketUrl != "#" ? $"<a href='{ticketUrl}' class='button'>Talebinizi Görüntüleyin</a>" : string.Empty)}

                            <p style='margin-top:18px;'>Kısa süre içinde yetkili personelimiz tarafından işleme alınacaktır. İyi günler dileriz.</p>
                        </div>
                        <div class='footer'>
                            <p><strong>Kobi Mühendislik</strong><br>Bu e-posta otomatik olarak gönderilmiştir.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body, true);
        }

        public async Task SendTicketStatusChangedEmailAsync(string toEmail, string toName, string ticketTitle, string newStatus, int ticketId)
        {
            if (string.IsNullOrEmpty(toEmail))
            {
                _logger.LogWarning("Müşteri e-posta adresi bulunamadı. Durum değişikliği e-postası gönderilemedi: {TicketId}", ticketId);
                return;
            }

            var subject = "Ticket Durum Güncellemesi";
            var logoUrl = "https://www.kobimuhendislik.com/assets/images/logo/KobiLogo.png";

            var portalBase = _configuration["FrontEnd:BaseUrl"] ?? "#";
            var ticketUrl = portalBase != "#" ? $"{portalBase.TrimEnd('/')}/customer/tickets/{ticketId}" : "#";

            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
                        .container {{ max-width: 600px; margin: 20px auto; padding: 0; border: 1px solid #eee; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.05); }}
                        .header {{ background-color: #ffffff; padding: 25px; text-align: center; border-bottom: 3px solid #2196F3; }}
                        .logo {{ max-width: 160px; height: auto; margin-bottom: 10px; }}
                        .content {{ background-color: #ffffff; padding: 30px; }}
                        .info {{ background-color: #f8fbfe; border: 1px solid #d1e9ff; border-radius: 8px; padding: 20px; margin: 20px 0; }}
                        .footer {{ background-color: #f9f9f9; padding: 20px; text-align: center; font-size: 12px; color: #888; border-top: 1px solid #eee; }}
                        .button {{ display: inline-block; padding: 12px 25px; background-color: #2196F3; color: #ffffff !important; text-decoration: none; border-radius: 5px; font-weight: bold; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <img src='{logoUrl}' alt='Kobi Mühendislik Logo' class='logo'>
                            <h2 style='margin:10px 0 0 0; color: #2196F3;'>Ticket Durumu Güncellendi</h2>
                        </div>
                        <div class='content'>
                            <p>Merhaba <strong>{toName}</strong>,</p>
                            <p>#{ticketId} numaralı talebinizin durumu güncellendi:</p>

                            <div class='info'>
                                <div><strong>Konu:</strong> {ticketTitle}</div>
                                <div style='margin-top:8px;'><strong>Yeni Durum:</strong> {newStatus}</div>
                                <div style='margin-top:8px;'><strong>ID:</strong> #{ticketId}</div>
                            </div>

                            {(ticketUrl != "#" ? $"<a href='{ticketUrl}' class='button'>Talebi Görüntüleyin</a>" : string.Empty)}

                            <p style='margin-top:18px;'>Herhangi bir sorunuz olursa bizimle iletişime geçebilirsiniz.</p>
                        </div>
                        <div class='footer'>
                            <p><strong>Kobi Mühendislik</strong><br>Bu e-posta otomatik olarak gönderilmiştir.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body, true);
        }
    }
}
