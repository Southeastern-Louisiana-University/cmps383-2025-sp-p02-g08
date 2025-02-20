using Selu383.SP25.P02.Api.Features.Users;
using System.ComponentModel.DataAnnotations;

namespace Selu383.SP25.P02.Api.Features.Theaters
{
    public class Theater
    {
        public int Id { get; set; }
        [MaxLength(120)]
        public required string Name { get; set; }
        public required string Address { get; set; }
        public required int SeatCount { get; set; }
        public int? ManagerId { get; set; }
        public User? Manager { get; set; }
    }
}
