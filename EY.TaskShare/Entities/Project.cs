using System.Text.Json.Serialization;

namespace EY.TaskShare.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        [JsonIgnore]
        public ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();
        [JsonIgnore]

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
