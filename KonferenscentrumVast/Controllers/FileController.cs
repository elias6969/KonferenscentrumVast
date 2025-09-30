using KonferenscentrumVast.DTO;
using KonferenscentrumVast.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KonferenscentrumVast.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require JWT to upload/view files
    public class FileController : ControllerBase
    {
        private readonly FileService _fileService;

        public FileController(FileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// Upload a file (contract or facility image)
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] FileUploadDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("No file uploaded.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";

            var result = await _fileService.UploadFileAsync(dto.File, dto.BookingId, dto.FacilityId, userId);
            return Ok(result);
        }

        /// <summary>
        /// Get a signed download URL for a file
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFileUrl(int id)
        {
            var url = await _fileService.GetFileUrlAsync(id, TimeSpan.FromMinutes(15));
            if (url == null) return NotFound();

            return Ok(new { Url = url });
        }

        /// <summary>
        /// List all files linked to a booking
        /// </summary>
        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetBookingFiles(int bookingId)
        {
            var files = await _fileService.ListBookingFilesAsync(bookingId);
            return Ok(files);
        }

        /// <summary>
        /// List all files linked to a facility
        /// </summary>
        [HttpGet("facility/{facilityId}")]
        public async Task<IActionResult> GetFacilityFiles(int facilityId)
        {
            var files = await _fileService.ListFacilityFilesAsync(facilityId);
            return Ok(files);
        }

        /// <summary>
        /// Delete a file (removes from GCS and SQL)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var deleted = await _fileService.DeleteFileAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
