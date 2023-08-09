using EY.TaskShare.Entities;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EY.TaskShare.Services
{
    public class TaskService
    {
        private readonly TaskShareContext dbContext;
        private readonly AuthenticateService authenticateService;
        public TaskService(TaskShareContext dbContext, AuthenticateService authenticateService)
        {
            this.dbContext = dbContext;
            this.authenticateService = authenticateService;
        }
        public void CreateTask(Tasks tasks, int projectId, string authorizationHeader)
        {
            var token = authorizationHeader.Substring(7);
            var currentUser = authenticateService.ValidateTokenAndGetUser(token);

            var project = dbContext.Projects.Include(p => p.Users).FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                throw new ArgumentException("Project not found.");
            }

            var isCurrentUserInSameTeam = project.Users.Any(u => u.Team == currentUser.Team);
            if (!isCurrentUserInSameTeam)
            {
                throw new UnauthorizedAccessException("User is not authorized to create a task for this project.");
            }

            tasks.UserId = currentUser.Id;
            tasks.ProjectId = projectId;
            tasks.CurrentDate = DateTime.UtcNow;
            dbContext.Tasks.Add(tasks);
            dbContext.SaveChanges();
        }
        public ICollection<Tasks> GetTasksForSupervisorProject(int projectId, string authorizationHeader)
        {
            var token = authorizationHeader.Substring(7);
            var currentUser = authenticateService.ValidateTokenAndGetUser(token);

            var project = dbContext.Projects.Include(p => p.Users).Include(p => p.Tasks).FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                throw new ArgumentException("Project not found.");
            }

            if (project.Users.All(u => u.Id != currentUser.Id))
            {
                throw new UnauthorizedAccessException("User is not authorized to access tasks for this project.");
            }

            return project.Tasks;
        }
        public ICollection<Tasks> GetTasksForCurrentUser(string authorizationHeader)
        {
            var token = authorizationHeader.Substring(7);
            var currentUser = authenticateService.ValidateTokenAndGetUser(token);

            return dbContext.Tasks.Where(t => t.UserId == currentUser.Id).ToList();
        }
        public ICollection<Tasks> GetTasksForSupervisor(string authorizationHeader)
        {
            var token = authorizationHeader.Substring(7);
            var currentUser = authenticateService.ValidateTokenAndGetUser(token);

            var projectIds = dbContext.Projects.Where(p => p.Users.Any(u => u.Id == currentUser.Id))
                                               .Select(p => p.Id)
                                               .ToList();

            return dbContext.Tasks.Where(t => projectIds.Contains((int)t.ProjectId!)).ToList();
        }
        public void UpdateTask(int taskId, Tasks updatedTask, string authorizationHeader)
        {
            var token = authorizationHeader.Substring(7);
            var currentUser = authenticateService.ValidateTokenAndGetUser(token);

            var task = dbContext.Tasks.Include(t => t.TimeSpentPerWeek)
                                      .FirstOrDefault(t => t.Id == taskId);

            if (task == null)
            {
                throw new ArgumentException("Task not found");
            }

            if (task.UserId != currentUser.Id)
            {
                throw new UnauthorizedAccessException("User is not authorized to update this task");
            }

            var isoWeekNumber = GetIsoWeekNumber(DateTime.Today);

            var taskTime = task.TimeSpentPerWeek.FirstOrDefault(tt => tt.WeekNumber == isoWeekNumber);

            if (taskTime == null)
            {
                taskTime = new TaskTime
                {
                    WeekNumber = isoWeekNumber
                };

                task.TimeSpentPerWeek.Add(taskTime);
            }

            var timeSpentThisWeek = taskTime?.TimeSpent ?? 0;

            var totalTimeSpent = timeSpentThisWeek + updatedTask.WorkHours;

            task.WorkHours = totalTimeSpent;
            taskTime!.TimeSpent = totalTimeSpent;

            dbContext.SaveChanges();
        }


        private int GetIsoWeekNumber(DateTime date)
        {
            var culture = CultureInfo.CurrentCulture;
            var calendar = culture.Calendar;
            var isoWeekNumber = calendar.GetWeekOfYear(date, culture.DateTimeFormat.CalendarWeekRule, DayOfWeek.Monday);
            if (isoWeekNumber == 53 && date.Month == 1)
            {
                isoWeekNumber = 1;
            }
            return isoWeekNumber;
        }

        public void DeleteTask(int taskId, string authorizationHeader)
        {
            var token = authorizationHeader.Substring(7);
            var currentUser = authenticateService.ValidateTokenAndGetUser(token);
            var task = dbContext.Tasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null)
            {
                throw new ArgumentException("Project not found");
            }

            if (task.UserId != currentUser.Id)
            {
                throw new UnauthorizedAccessException("User is not authorized to update this task");
            }

            dbContext.Tasks.Remove(task);
            dbContext.SaveChanges();
        }

        public ICollection<Tasks> GetTasksWithTimeForWeek(string authorizationHeader, int weekNumber)
        {
            var token = authorizationHeader.Substring(7);
            var currentUser = authenticateService.ValidateTokenAndGetUser(token);
            var tasks = dbContext.Tasks.Include(t => t.TimeSpentPerWeek)
                                       .Where(t => t.UserId == currentUser.Id)
                                       .ToList();

            foreach (var task in tasks)
            {
                var timeSpent = task.TimeSpentPerWeek.FirstOrDefault(tt => tt.WeekNumber == weekNumber);
                if (timeSpent != null)
                {
                    task.WorkHours = timeSpent.TimeSpent;


                }
                else
                {
                    task.WorkHours = 0;
                }
                task.ProjectName = GetProjectTitleById((int)task.ProjectId);

            }

            return tasks;
        }

        public string GetProjectTitleById(int projectId)
        {
            var project = dbContext.Projects.FirstOrDefault(p => p.Id == projectId);
            return project!.Title;
        }

        public string GetUserNameById(int userId)
        {
            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            return user!.UserName;
        }

        public ICollection<Tasks> GetTasksWithTimeForMonthSupervisor(string authorizationHeader, int year, int month)
        {
            var token = authorizationHeader.Substring(7);
            var currentUser = authenticateService.ValidateTokenAndGetUser(token);

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var projectIds = dbContext.Projects.Where(p => p.Users.Any(u => u.Id == currentUser.Id))
                                              .Select(p => p.Id)
                                              .ToList();

            var tasks = dbContext.Tasks.Include(t => t.TimeSpentPerWeek)
                                       .Where(t => projectIds.Contains((int)t.ProjectId!))
                                       .ToList();


            foreach (var task in tasks)
            {
                var totalHours = task.TimeSpentPerWeek.Where(tt => tt.tasks != null && tt.WeekNumber >= GetIsoWeekNumber(startDate) && tt.WeekNumber <= GetIsoWeekNumber(endDate))
                                                     .Select(tt => tt.TimeSpent)
                                                     .DefaultIfEmpty(0)
                                                     .Sum();

                task.WorkHours = totalHours;

                task.ProjectName = GetProjectTitleById((int)task.ProjectId!);

                task.UserName = GetUserNameById((int)task.UserId!);
            }

            return tasks;
        }


    }


}

