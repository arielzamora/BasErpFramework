using System;
using System.Collections.Generic;
using System.Text;

namespace BasErpFramework.Application.Dto
{
    public sealed record TokenDto
    {
        public string? Token { get; set; }
        public string? TokenType { get; set; }= "Bearer";
        public int Expiration { get; set; }

    }
}
