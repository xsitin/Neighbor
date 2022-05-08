using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Board.Infrastructure
{
    public class SecurityToken
    {

        public string Username { get; set; }
        public string Role { get; set; }
        public string AccessToken { get; set; }
        public DateTime ExpiredAt { get; set; }

      
    }
}