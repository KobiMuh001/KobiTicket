namespace KobiMuhendislikTicket.Application.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// E-posta gönderir
        /// </summary>
        /// <param name="toEmail">Alıcı e-posta adresi</param>
        /// <param name="subject">E-posta konusu</param>
        /// <param name="body">E-posta içeriği (HTML destekler)</param>
        /// <param name="isHtml">İçerik HTML mi?</param>
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Ticket atama bildirimi e-postası gönderir
        /// </summary>
        Task SendTicketAssignmentEmailAsync(string toEmail, string staffName, string ticketTitle, string tenantName, int ticketId);

        /// <summary>
        /// Ticket'a yeni yorum eklendiği bildirimi e-postası gönderir
        /// </summary>
        Task SendNewCommentEmailAsync(string toEmail, string staffName, string ticketTitle, string authorName, string commentPreview, int ticketId);

        /// <summary>
        /// Admin'e yeni ticket oluşturuldu bildirimi e-postası gönderir
        /// </summary>
        Task SendNewTicketEmailToAdminAsync(string ticketTitle, string tenantName, string priority, string description, int ticketId);

        /// <summary>
        /// Müşteriye ticket oluşturma onay e-postası gönderir
        /// </summary>
        Task SendTicketCreatedConfirmationEmailAsync(string toEmail, string toName, string ticketTitle, string ticketCode, int ticketId);

        /// <summary>
        /// Müşteriye ticket durum değişikliği hakkında e-posta gönderir
        /// </summary>
        Task SendTicketStatusChangedEmailAsync(string toEmail, string toName, string ticketTitle, string newStatus, int ticketId);
    }
}
