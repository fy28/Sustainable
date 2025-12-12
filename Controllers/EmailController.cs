using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Sustainable.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EmailController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromForm] EmailRequest request)
        {
            try
            {
                var fromEmail = _config["SMTP:Email"];
                var password = _config["SMTP:Password"];

                var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, password),
                    EnableSsl = true
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(fromEmail, "Mada Market Export"),
                    Subject = request.Subject,
                    Body = request.Body,
                    IsBodyHtml = false,
                    BodyEncoding = Encoding.UTF8
                };

                mail.To.Add(request.To);

                // 🔥 Ajout des pièces jointes uploadées depuis le front
                if (request.Attachments != null)
                {
                    foreach (var file in request.Attachments)
                    {
                        if (file.Length > 0)
                        {
                            var memory = new MemoryStream();
                            await file.CopyToAsync(memory);
                            memory.Position = 0;

                            mail.Attachments.Add(
                                new Attachment(memory, file.FileName)
                            );
                        }
                    }
                }

                await smtp.SendMailAsync(mail);

                return Ok(new { message = "Email envoyé avec succès !" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class EmailRequest
    {
        public string To { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
        public List<IFormFile>? Attachments { get; set; }
    }
}
