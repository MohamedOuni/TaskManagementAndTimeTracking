using EY.TaskShare.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EY.TaskShare.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly AuthenticateService _authenticateService;
        private readonly ProjectService _projectService;
        public ProjectController(ProjectService projectService, AuthenticateService authenticateService)
        {
            _projectService = projectService;
            _authenticateService = authenticateService;
        }

        [HttpPost("create")]
        public IActionResult CreateProject([FromBody] Project project)
        {
            try
            {
                var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
                _projectService.CreateProject(project,
                                              authorizationHeader!);
                return Ok(new { message = "Project created successfully", project });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{projectId}")]
        public IActionResult DeleteProject(int projectId)
        {
            try
            {
                var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
                _projectService.DeleteProject(projectId,
                                              authorizationHeader!);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{projectId}")]
        public IActionResult UpdateProject(int projectId, [FromBody] Project updatedProject)
        {
            try
            {
                _projectService.UpdateProject(projectId,
                                              updatedProject,
                                              Request.Headers["Authorization"]!);
                return Ok("Project updated successfully");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("projectsForUser")]
        public IActionResult GetProjectsForUser()
        {
            try
            {
                var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
                var projects = _projectService.GetProjectsForUser(authorizationHeader!);
                return Ok(projects);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getprojectforteam")]
        public ActionResult<List<Project>> GetProjectsForTeam()
        {
            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            var projects = _projectService.GetProjectsForTeam(authorizationHeader!);

            return projects;
        }

    }
}