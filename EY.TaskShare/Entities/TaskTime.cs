

using System.Text.Json.Serialization;

namespace EY.TaskShare.Entities
{
    public class TaskTime
    {
        public int Id { get; set; }
        public int WeekNumber { get; set; }
        public float TimeSpent { get; set; }

        [JsonIgnore]

        public int TaskId { get; set; }
        [JsonIgnore]
        public Tasks? tasks { get; set; }
    }
}
