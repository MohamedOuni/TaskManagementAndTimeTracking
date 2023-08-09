
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EY.TaskShare.Entities
{
    public class Tasks
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public float WorkHours { get; set; }

        public DateTime CurrentDate { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public int? UserId { get; set; }

        [JsonIgnore]
        public Project? Project { get; set; }
        [JsonIgnore]
        public int? ProjectId { get; set; }
        [JsonIgnore]
        public List<TaskTime> TimeSpentPerWeek { get; set; } = new List<TaskTime>();

        [NotMapped]
        public string ProjectName { get; set; } = string.Empty;

        [NotMapped]
        public string UserName { get; set; } = string.Empty;


    }
}
