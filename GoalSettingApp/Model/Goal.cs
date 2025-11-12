namespace GoalSettingApp
{
    public class Goal
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public DateTime CreatedAt { get; set; }
        bool IsCompleted { get; set; } = false;

    }

    public enum PriorityLevel
    {
        Low,
        Medium,
        High
    }
}

