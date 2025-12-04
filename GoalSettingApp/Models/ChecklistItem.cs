using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace GoalSettingApp
{
    [Table("checklist_items")]
    public class ChecklistItem : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("goal_id")]
        public int GoalId { get; set; }

        [Column("text")]
        public string Text { get; set; } = string.Empty;

        [Column("is_completed")]
        public bool IsCompleted { get; set; } = false;

        [Column("sort_order")]
        public int SortOrder { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}

