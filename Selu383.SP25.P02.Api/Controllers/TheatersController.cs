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

        [HttpGet]
        public IQueryable<TheaterDto> GetAllTheaters()
        {
            return GetTheaterDtos(theaters);
        }

        [HttpGet]
        [Route("{id}")]
        public ActionResult<TheaterDto> GetTheaterById(int id)
        {
            var result = GetTheaterDtos(theaters.Where(x => x.Id == id)).FirstOrDefault();
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult<TheaterDto> CreateTheater(TheaterDto dto)
        {
            if (IsInvalid(dto))
            {
                return BadRequest();
            }

            //  Ensure ManagerId is valid (can be null)
            if (dto.ManagerId.HasValue)
            {
                var managerExists = dataContext.Users.Any(u => u.Id == dto.ManagerId);
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

            dataContext.SaveChanges();

            dto.Id = theater.Id;

            return CreatedAtAction(nameof(GetTheaterById), new { id = dto.Id }, dto);
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize] // Requires authentication
        public ActionResult<TheaterDto> UpdateTheater(int id, TheaterDto dto)
        {
            if (IsInvalid(dto))
            {
                return BadRequest();
            }

            var theater = theaters.FirstOrDefault(x => x.Id == id);
            if (theater == null)
            {
                return NotFound();
            }

            //  Get logged-in user ID
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(); // User must be logged in
            }

            int userId = int.Parse(userIdClaim);
            bool isAdmin = User.IsInRole("Admin");
            bool isManager = theater.ManagerId.HasValue && theater.ManagerId == userId; //  Bob must be ManagerId

              //Allow Admins or Managers to update
            if (!isAdmin && !isManager)
            {
                return Forbid(); //  403 Forbidden if user is not Manager/Admin
            }

           

            theater.Name = dto.Name;
            theater.Address = dto.Address;
            theater.SeatCount = dto.SeatCount;
            theater.ManagerId = dto.ManagerId;

            dataContext.SaveChanges();

            dto.Id = theater.Id;

            return Ok(dto);
        }



        [HttpDelete]
        [Route("{id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteTheater(int id)
        {
            var theater = theaters.FirstOrDefault(x => x.Id == id);
            if (theater == null)
            {
                return NotFound();
            }

            theaters.Remove(theater);

            dataContext.SaveChanges();

            return Ok();
        }

        private static bool IsInvalid(TheaterDto dto)
        {
            return string.IsNullOrWhiteSpace(dto.Name) ||
                   dto.Name.Length > 120 ||
                   string.IsNullOrWhiteSpace(dto.Address) ||
                   dto.SeatCount <= 0;
        }

        private static IQueryable<TheaterDto> GetTheaterDtos(IQueryable<Theater> theaters)
        {
            return theaters
                .Select(x => new TheaterDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Address = x.Address,
                    SeatCount = x.SeatCount,
                    ManagerId = x.ManagerId
                });
        }
    }
}
