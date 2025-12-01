namespace GoalSettingApp.Helpers;

public class PriorityBadgeHelper
{
    public static string GetPriorityColor(PriorityLevel priority)
    {
        return priority switch
        {
            PriorityLevel.Low => "secondary",
            PriorityLevel.Medium => "warning",
            PriorityLevel.High => "danger",
            _ => "secondary"
        };
    }
}