using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace GoalSettingApp.Models
{
    /// <summary>
    /// User profile model for storing additional user information
    /// This should match a 'profiles' table in Supabase
    /// </summary>
    [Table("profiles")]
    public class UserProfile : BaseModel
    {
        [PrimaryKey("id", false)]
        [Column("id")]
        public string Id { get; set; } = string.Empty;

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("email_notifications_enabled")]
        public bool EmailNotificationsEnabled { get; set; } = true;
    }
}

