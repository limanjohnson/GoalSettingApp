using GoalSettingApp.Models;

namespace GoalSettingApp.Services
{
    /// <summary>
    /// Background service that checks for upcoming task due dates and sends reminder emails
    /// </summary>
    public class TaskReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TaskReminderService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

        public TaskReminderService(IServiceProvider serviceProvider, ILogger<TaskReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Task Reminder Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking for task reminders");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Task Reminder Service stopped");
        }

        private async Task CheckAndSendRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var supabase = scope.ServiceProvider.GetRequiredService<Supabase.Client>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            _logger.LogInformation("Checking for tasks with upcoming due dates...");

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            try
            {
                // Get all incomplete goals with due dates
                var response = await supabase
                    .From<Goal>()
                    .Where(g => g.IsCompleted == false)
                    .Get();

                var goals = response.Models;

                foreach (var goal in goals)
                {
                    if (goal.DueDate == null) continue;

                    var dueDate = goal.DueDate.Value.Date;
                    string? reminderType = null;

                    if (dueDate < today)
                    {
                        reminderType = "Overdue";
                    }
                    else if (dueDate == today)
                    {
                        reminderType = "Due Today";
                    }
                    else if (dueDate == tomorrow)
                    {
                        reminderType = "Due Tomorrow";
                    }

                    if (reminderType != null)
                    {
                        // Get user email from Supabase Auth
                        var userEmail = await GetUserEmailAsync(supabase, goal.UserId);
                        var userName = await GetUserNameAsync(supabase, goal.UserId);

                        if (!string.IsNullOrEmpty(userEmail))
                        {
                            await emailService.SendTaskReminderAsync(
                                userEmail,
                                userName ?? "User",
                                goal.Title,
                                goal.Description,
                                goal.DueDate.Value,
                                reminderType
                            );

                            _logger.LogInformation(
                                "Sent {ReminderType} reminder for task '{Task}' to {Email}",
                                reminderType, goal.Title, userEmail
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process task reminders");
            }
        }

        private async Task<string?> GetUserEmailAsync(Supabase.Client supabase, string userId)
        {
            try
            {
                // For now, we'll need to store user email in a profiles table or use admin API
                // This is a simplified approach - you may need to adjust based on your user data structure
                var response = await supabase
                    .From<UserProfile>()
                    .Where(p => p.UserId == userId)
                    .Single();

                return response?.Email;
            }
            catch
            {
                return null;
            }
        }

        private async Task<string?> GetUserNameAsync(Supabase.Client supabase, string userId)
        {
            try
            {
                var response = await supabase
                    .From<UserProfile>()
                    .Where(p => p.UserId == userId)
                    .Single();

                return response?.DisplayName;
            }
            catch
            {
                return null;
            }
        }
    }
}

