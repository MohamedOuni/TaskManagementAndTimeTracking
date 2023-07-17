using EY.TaskShare;
using EY.TaskShare.Entities;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class ProjectService
{
    private readonly TaskShareContext dbContext;
    private readonly AuthenticateService authenticateService;

    public ProjectService(TaskShareContext dbContext, AuthenticateService authenticateService)
    {
        this.dbContext = dbContext;
        this.authenticateService = authenticateService;
    }

    public void CreateProject(Project project, string authorizationHeader)
    {
        var token = authorizationHeader.Substring(7);

        var user = authenticateService.ValidateTokenAndGetUser(token);
        Console.WriteLine("User : " + user);

        if (user.Role != Role.Supervisor)
        {
            throw new UnauthorizedAccessException("User is not authorized to create a project");
        }
        project.Users.Add(user);
        dbContext.Projects.Add(project);
        dbContext.SaveChanges();
    }

    public void DeleteProject(int projectId, string authorizationHeader)
    {
        var token = authorizationHeader.Substring(7);
        var currentUser = authenticateService.ValidateTokenAndGetUser(token);
        var project = dbContext.Projects.Include(p => p.Users).FirstOrDefault(p => p.Id == projectId);

        if (project == null)
        {
            throw new ArgumentException("Project not found");
        }

        var isCurrentUserCreator = project.Users.Any(u => u.Id == currentUser.Id);

        if (!isCurrentUserCreator)
        {
            throw new UnauthorizedAccessException("User is not authorized to delete this project");
        }

        dbContext.Projects.Remove(project);
        dbContext.SaveChanges();
    }

    public void UpdateProject(int projectId, Project updatedProject, string authorizationHeader)
    {
        var token = authorizationHeader.Substring(7);

        var currentUser = authenticateService.ValidateTokenAndGetUser(token);

        var project = dbContext.Projects.Include(p => p.Users).FirstOrDefault(p => p.Id == projectId);

        if (project == null)
        {
            throw new ArgumentException("Project not found");
        }
        var isCurrentUserCreator = project.Users.Any(u => u.Id == currentUser.Id);

        if (!isCurrentUserCreator)
        {
            throw new UnauthorizedAccessException("User is not authorized to update this project");
        }

        project.Title = updatedProject.Title;
        dbContext.SaveChanges();
    }

    public List<Project> GetProjectsForUser(string authorizationHeader)
    {
        var token = authorizationHeader.Substring(7);
        var currentUser = authenticateService.ValidateTokenAndGetUser(token);

        var projects = dbContext.Projects.Include(p => p.Users).Where(p => p.Users.Any(u => u.Id == currentUser.Id)).ToList();

        return projects;
    }

    public List<Project> GetProjectsForTeam(string authorizationHeader)
    {
        var token = authorizationHeader.Substring(7);
        var currentUser = authenticateService.ValidateTokenAndGetUser(token);

        var projects = dbContext.Projects
            .Include(p => p.Users)
            .AsEnumerable()
            .Where(p => p.Users.Any(u => u.Team == currentUser.Team))
            .ToList();

        return projects;
    }

    
}


