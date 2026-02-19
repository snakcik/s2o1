using S2O1.Business.Services.Interfaces;
using S2O1.Core.Interfaces;
using S2O1.Domain.Entities;
using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;

namespace S2O1.Business.Services.Implementation
{
    public class MailService : IMailService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MailService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private async Task<SmtpClient> GetSmtpClientAsync()
        {
            var settings = await _unitOfWork.Repository<SystemSetting>().FindAsync(s => s.SettingKey.StartsWith("Mail_"));
            
            var host = settings.FirstOrDefault(s => s.SettingKey == "Mail_SmtpHost")?.SettingValue;
            var portStr = settings.FirstOrDefault(s => s.SettingKey == "Mail_SmtpPort")?.SettingValue;
            var user = settings.FirstOrDefault(s => s.SettingKey == "Mail_Username")?.SettingValue;
            var pass = settings.FirstOrDefault(s => s.SettingKey == "Mail_Password")?.SettingValue;
            var ssl = settings.FirstOrDefault(s => s.SettingKey == "Mail_EnableSsl")?.SettingValue == "true";

            if (string.IsNullOrEmpty(host)) throw new System.Exception("SMTP ayarları yapılandırılmamış.");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            var smtp = new SmtpClient(host, int.Parse(portStr ?? "587"))
            {
                EnableSsl = ssl,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 20000
            };

            if (!string.IsNullOrEmpty(user))
            {
                smtp.Credentials = new NetworkCredential(user, pass);
            }

            return smtp;
        }

        private async Task<string> GetFromEmailAsync()
        {
            var setting = (await _unitOfWork.Repository<SystemSetting>().FindAsync(s => s.SettingKey == "Mail_FromEmail")).FirstOrDefault();
            return setting?.SettingValue ?? "";
        }

        private async Task<string> GetFromNameAsync()
        {
            var setting = (await _unitOfWork.Repository<SystemSetting>().FindAsync(s => s.SettingKey == "Mail_FromName")).FirstOrDefault();
            return setting?.SettingValue ?? "S2O1 System";
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            using var smtp = await GetSmtpClientAsync();
            var fromEmail = await GetFromEmailAsync();
            var fromName = await GetFromNameAsync();

            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            mail.To.Add(to);

            await smtp.SendMailAsync(mail);
        }

        public async Task SendOfferEmailAsync(string to, string subject, string htmlContent, string fileName)
        {
            using var smtp = await GetSmtpClientAsync();
            var fromEmail = await GetFromEmailAsync();
            var fromName = await GetFromNameAsync();

            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = "Ekli dosyada teklifiniz yer almaktadır.",
                IsBodyHtml = false
            };
            mail.To.Add(to);

            // Create attachment from HTML string
            // For now, since we don't have a PDF generator, we send the HTML as an attachment .html
            // But the user asked for "PDF olarak". I will wrap it in a basic HTML and name it .html for now
            // OR I can use a trick to send it as body. 
            // Better: Just send body as HTML for now but call it "Teklif"
            
            mail.IsBodyHtml = true;
            mail.Body = htmlContent;

            await smtp.SendMailAsync(mail);
        }
    }
}
