using System.Net;
using System.Net.Mail;

namespace HotelManagementSystem.SMTP
{
    public class SMTP
    {
        public static void SendSMTPMail(string toAddress, string body)
        {
            MailMessage msg = new MailMessage("hotelmgntsystem@gmail.com", toAddress, "Booking Confirmation for Hotel XYZ", body);

            msg.IsBodyHtml = true;
            var SMTPClientObj = new SmtpClient();

            SMTPClientObj.UseDefaultCredentials = false;
            SMTPClientObj.Credentials = new NetworkCredential("hotelmgntsystem@gmail.com", "P@ssword2017");
            SMTPClientObj.Host = "smtp.gmail.com";
            SMTPClientObj.Port = 587;
            SMTPClientObj.EnableSsl = true;
            SMTPClientObj.Send(msg);
            
        }
    }
}