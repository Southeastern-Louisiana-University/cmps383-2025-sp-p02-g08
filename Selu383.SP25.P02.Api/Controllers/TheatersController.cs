using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Selu383.SP25.P02.Api.Data;
using Selu383.SP25.P02.Api.Features.Theaters;
using Selu383.SP25.P02.Api.Features.Users;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Selu383.SP25.P02.Api.Controllers
{
    [Route("api/theaters")]
    [ApiController]
    public class TheatersController : ControllerBase
    {
        private readonly DbSet<Theater> theaters;
        private readonly DataContext dataContext;

        public TheatersController(DataContext dataContext)
        {
            this.dataContext = dataContext;
            theaters = dataContext.Set<Theater>();
        }

        // GET: List all theaters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TheaterDto>>> GetAllTheaters()
        {
            var theaterDtos = await GetTheaterDtos(theaters).ToListAsync();
            return Ok(theaterDtos);
        }

        // GET: Get theater by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<TheaterDto>> GetTheaterById(int id)
        {
            var result = await GetTheaterDtos(theaters.Where(x => x.Id == id)).FirstOrDefaultAsync();
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        // POST: Create a new theater
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TheaterDto>> CreateTheater([FromBody] TheaterDto dto)
        {
            if (IsInvalid(dto, out var errorMessage))
            {
                return BadRequest(errorMessage);
            }

            // Validate ManagerId if provided
            if (dto.ManagerId.HasValue)
            {
                bool managerExists = await dataContext.Users.AnyAsync(u => u.Id == dto.ManagerId);
                if (!managerExists)
                {
                    return BadRequest("Invalid ManagerId. User does not exist.");
                }
            }

            var theater = new Theater
            {
                Name = dto.Name,
                Address = dto.Address,
                SeatCount = dto.SeatCount,
                ManagerId = dto.ManagerId
            };

            theaters.Add(theater);
            await dataContext.SaveChangesAsync();

            dto.Id = theater.Id;
            return CreatedAtAction(nameof(GetTheaterById), new { id = dto.Id }, dto);
        }

        // PUT: Update a theater
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<TheaterDto>> UpdateTheater(int id, [FromBody] TheaterDto dto)
        {
            if (IsInvalid(dto, out var errorMessage))
            {
                return BadRequest(errorMessage);
            }

            var theater = await theaters.FindAsync(id);
            if (theater == null)
            {
                return NotFound();
            }

            if (!IsAuthorizedToModify(theater))
            {
                return Forbid();
            }

            theater.Name = dto.Name;
            theater.Address = dto.Address;
            theater.SeatCount = dto.SeatCount;
            theater.ManagerId = dto.ManagerId;

            await dataContext.SaveChangesAsync();

            dto.Id = theater.Id;
            return Ok(dto);
        }

        // DELETE: Delete a theater
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteTheater(int id)
        {
            var theater = await theaters.FindAsync(id);
            if (theater == null)
            {
                return NotFound();
            }

            theaters.Remove(theater);
            await dataContext.SaveChangesAsync();

            return NoContent(); // 204 is expected for DELETE in RESTful APIs
        }

        // Helper function: Validate input
        private static bool IsInvalid(TheaterDto dto, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                errorMessage = "Theater name cannot be empty.";
                return true;
            }
            if (dto.Name.Length > 120)
            {
                errorMessage = "Theater name exceeds 120 characters.";
                return true;
            }
            if (string.IsNullOrWhiteSpace(dto.Address))
            {
                errorMessage = "Theater address is required.";
                return true;
            }
            if (dto.SeatCount <= 0)
            {
                errorMessage = "Seat count must be greater than zero.";
                return true;
            }
            errorMessage = null;
            return false;
        }

        // Helper function: Convert Theater entities to DTOs
        private static IQueryable<TheaterDto> GetTheaterDtos(IQueryable<Theater> theaters)
        {
            return theaters.Select(x => new TheaterDto
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                SeatCount = x.SeatCount,
                ManagerId = x.ManagerId
            });
        }

        // Helper function: Check if the user is authorized to modify
        private bool IsAuthorizedToModify(Theater theater)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return false;
            }

            int userId = int.Parse(userIdClaim);
            bool isAdmin = User.IsInRole("Admin");
            bool isManager = theater.ManagerId.HasValue && theater.ManagerId == userId;

            return isAdmin || isManager;
        }
    }
}

