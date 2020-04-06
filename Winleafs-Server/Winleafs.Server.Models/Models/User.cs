using System;
using System.ComponentModel.DataAnnotations;
using Winleafs.Server.Models.Interfaces;

namespace Winleafs.Server.Models.Models
{
    public class User : IEntity
    {
        [Key]
        public long Id { get; set; }

        public string ApplicationId { get; set; }

        public string SpotifyAccessToken { get; set; }

        public DateTime SpotifyExpiresOn { get; set; }

        public string RefreshToken { get; set; }
    }
}
