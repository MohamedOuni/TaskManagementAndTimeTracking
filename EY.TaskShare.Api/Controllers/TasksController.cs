using EY.TaskShare.Entities;
using EY.TaskShare.Services;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Style;

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
                string authorizationHeader = Request.Headers["Authorization"]!;
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
            string authorizationHeader = Request.Headers["Authorization"]!;
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
            string authorizationHeader = Request.Headers["Authorization"]!;
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
            string authorizationHeader = Request.Headers["Authorization"]!;
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
        [HttpPut("{taskId}")]
        public IActionResult UpdateTask(int taskId, [FromBody] Tasks updatedTask)
        {
            try
            {
                string authorizationHeader = Request.Headers["Authorization"]!;
                _taskService.UpdateTask(taskId, updatedTask, authorizationHeader);
                return Ok(new { message = "Task updated successfully." });
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
                return StatusCode(500, "An error occurred while updating the task.");
            }
        }

        [HttpDelete("{taskId}")]
        public IActionResult DeleteProject(int taskId)
        {
            try
            {
                var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
                _taskService.DeleteTask(taskId, authorizationHeader!);
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

        [HttpGet("export/{weekNumber}")]
        public IActionResult ExportWeeklyReport(int weekNumber)
        {
            try
            {
                var authorizationHeader = Request.Headers["Authorization"].ToString();
                var tasks = _taskService.GetTasksWithTimeForWeek(authorizationHeader, weekNumber);

                var stream = new MemoryStream();
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Weekly Report");
                    worksheet.Cells[1, 1].Value = "Creation Date";
                    worksheet.Cells[1, 2].Value = "Task";
                    worksheet.Cells[1, 3].Value = "Description";
                    worksheet.Cells[1, 4].Value = "Time Spent";
                    worksheet.Cells[1, 5].Value = "Project Name";

                    using (var headerRange = worksheet.Cells[1, 1, 1, 5])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                        headerRange.Style.Font.Color.SetColor(Color.White);
                    }

                    int row = 2;
                    foreach (var task in tasks)
                    {
                        worksheet.Cells[row, 1].Value = task.CurrentDate.ToString("yyyy-MM-dd");
                        worksheet.Cells[row, 2].Value = task.Title;
                        worksheet.Cells[row, 3].Value = task.Description;
                        worksheet.Cells[row, 4].Value = task.WorkHours;
                        var projectTitle = _taskService.GetProjectTitleById((int)task.ProjectId!);
                        worksheet.Cells[row, 5].Value = projectTitle;

                        row++;
                    }

                    worksheet.Column(1).AutoFit();
                    worksheet.Column(2).AutoFit();
                    worksheet.Column(3).AutoFit();
                    worksheet.Column(4).AutoFit();
                    worksheet.Column(5).AutoFit();


                    worksheet.Column(1).Width += 10;
                    worksheet.Column(2).Width += 10;
                    worksheet.Column(3).Width += 10;
                    worksheet.Column(4).Width += 10;
                    worksheet.Column(5).Width += 10;


                    package.Save();
                }

                stream.Position = 0;

                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = $"weekly_report_{weekNumber}.xlsx";

                return File(stream, contentType, fileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("export/monthly/{year}/{month}")]
        public IActionResult ExportMonthlyReport(int year, int month)
        {
            try
            {
                var authorizationHeader = Request.Headers["Authorization"].ToString();
                var tasks = _taskService.GetTasksWithTimeForMonth(authorizationHeader, year, month);

                var stream = new MemoryStream();
                using (var package = new ExcelPackage(stream))
                {
                    var workbook = package.Workbook;

                    for (int weekNumber = 1; weekNumber <= 5; weekNumber++)
                    {
                        var worksheet = workbook.Worksheets.Add($"Week {weekNumber}");

                        worksheet.Cells[1, 1].Value = "Creation Date";
                        worksheet.Cells[1, 2].Value = "Task";
                        worksheet.Cells[1, 3].Value = "Description";
                        worksheet.Cells[1, 4].Value = "Time Spent";
                        worksheet.Cells[1, 5].Value = "Project Name";


                        using (var headerRange = worksheet.Cells[1, 1, 1, 5])
                        {
                            headerRange.Style.Font.Bold = true;
                            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                            headerRange.Style.Font.Color.SetColor(Color.White);
                        }

                        int row = 2;
                        foreach (var task in tasks)
                        {
                            var taskTime = task.TimeSpentPerWeek.FirstOrDefault(tt => tt.WeekNumber == weekNumber);

                            worksheet.Cells[row, 1].Value = task.CurrentDate.ToString("yyyy-MM-dd");
                            worksheet.Cells[row, 2].Value = task.Title;
                            worksheet.Cells[row, 3].Value = task.Description;
                            worksheet.Cells[row, 4].Value = task.WorkHours;
                            var projectTitle = _taskService.GetProjectTitleById((int)task.ProjectId!);
                            worksheet.Cells[row, 5].Value = projectTitle;
                            row++;
                        }

                        worksheet.Column(1).AutoFit();
                        worksheet.Column(2).AutoFit();
                        worksheet.Column(3).AutoFit();
                        worksheet.Column(4).AutoFit();
                        worksheet.Column(5).AutoFit();


                        worksheet.Column(1).Width += 10;
                        worksheet.Column(2).Width += 10;
                        worksheet.Column(3).Width += 10;
                        worksheet.Column(4).Width += 10;
                        worksheet.Column(5).Width += 10;

                    }

                    package.Save();
                }

                stream.Position = 0;

                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = $"monthly_report_{year}_{month}.xlsx";

                return File(stream, contentType, fileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



    }
}

