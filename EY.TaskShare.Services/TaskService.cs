using EY.TaskShare.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

            return dbContext.Tasks.Where(t => projectIds.Contains((int)t.ProjectId)).ToList();
        }

    }

}

