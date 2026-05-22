using System;
using System.Collections.Generic;
using System.Text;

namespace BasErpFramework.Application.Dto
{
    public sealed record SignUpDto
    {
        public string FirstName { get; set; }=string.Empty;
        public string LastName{ get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

    }
}
