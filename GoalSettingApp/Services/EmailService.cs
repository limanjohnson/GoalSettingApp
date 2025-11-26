using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using GoalSettingApp.Models;

namespace GoalSettingApp.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<EmailService> _logger;

        public EmailService(EmailSettings settings, IWebHostEnvironment environment, ILogger<EmailService> logger)
        {
            _settings = settings;
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Sends a task reminder email to the user
        /// </summary>
        public async Task<bool> SendTaskReminderAsync(string toEmail, string toName, string taskTitle, string taskDescription, DateTime dueDate, string reminderType)
        {
            try
            {
                // Load the HTML template
                var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", "task-reminder.html");
                
                if (!File.Exists(templatePath))
                {
                    _logger.LogError("Email template not found at {Path}", templatePath);
                    return false;
                }

                var htmlTemplate = await File.ReadAllTextAsync(templatePath);

                // Replace placeholders with actual values
                htmlTemplate = htmlTemplate
                    .Replace("{{to_name}}", toName)
                    .Replace("{{task_title}}", taskTitle)
                    .Replace("{{task_description}}", string.IsNullOrEmpty(taskDescription) ? "No description provided" : taskDescription)
                    .Replace("{{due_date}}", dueDate.ToString("dddd, MMMM dd, yyyy"))
                    .Replace("{{reminder_type}}", reminderType)
                    .Replace("{{app_url}}", Environment.GetEnvironmentVariable("APP_URL") ?? "http://localhost:5200");

                // Create the email message
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = GetSubjectLine(reminderType, taskTitle);

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlTemplate
                };
                message.Body = bodyBuilder.ToMessageBody();

                // Send the email
                using var client = new SmtpClient();
                
                await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Task reminder email sent to {Email} for task: {Task}", toEmail, taskTitle);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send task reminder email to {Email}", toEmail);
                return false;
            }
        }

        private static string GetSubjectLine(string reminderType, string taskTitle)
        {
            return reminderType.ToLower() switch
            {
                "overdue" => $"⚠️ Overdue Task: {taskTitle}",
                "today" => $"📅 Task Due Today: {taskTitle}",
                "tomorrow" => $"🔔 Task Due Tomorrow: {taskTitle}",
                _ => $"Task Reminder: {taskTitle}"
            };
        }
    }
}

