using EY.TaskShare.Entities;
using EY.TaskShare.Services;
using Microsoft.AspNetCore.Mvc;

namespace EY.TaskShare.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        public TaskService _taskService = default!;
        public TasksController(TaskService taskService)
        {
            _taskService = taskService;
        }
        [HttpPost("create/{projectId}")]
        public IActionResult CreateTask([FromBody] Tasks tasks, int projectId)
        {
            try
            {
                string authorizationHeader = Request.Headers["Authorization"];
                _taskService.CreateTask(tasks, projectId, authorizationHeader);
                return Ok(new { message = "Task created successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while creating the task.");
            }
        }

        [HttpGet("{projectId}")]
        public IActionResult GetTasksForSupervisorProject(int projectId)
        {
            string authorizationHeader = Request.Headers["Authorization"];
            try
            {
                var tasks = _taskService.GetTasksForSupervisorProject(projectId, authorizationHeader);
                return Ok(tasks);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("TaskUser")]
        public IActionResult GetTasksForCurrentUser()
        {
            string authorizationHeader = Request.Headers["Authorization"];
            try
            {
                var tasks = _taskService.GetTasksForCurrentUser(authorizationHeader);
                return Ok(tasks);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("TaskSupervisor")]
        public IActionResult GetTasksForSupervisor()
        {
            string authorizationHeader = Request.Headers["Authorization"];
            try
            {
                var tasks = _taskService.GetTasksForSupervisor(authorizationHeader);
                return Ok(tasks);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}

