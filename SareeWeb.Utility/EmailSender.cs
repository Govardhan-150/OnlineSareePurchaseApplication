using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SareeWeb.Utility
{
    public class EmailSender : IEmailSender
    {
        //public string SendGripSecretKey { get; set; }
        //public EmailSender(IConfiguration _config)
        //{
        //    SendGripSecretKey = _config.GetValue<string>("SendGrip:Secret");
        //}
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Task.CompletedTask;
        }
        //public Task SendEmailAsync(string email, string subject, string htmlMessage)
        //{
        //    var client = new SendGridClient(SendGripSecretKey);
        //    var from = new EmailAddress("govardhan@bookstore.com","Book Store");
        //    var to=new EmailAddress(email);
        //    var message = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);
        //    return client.SendEmailAsync(message);
        //}
    }
}
