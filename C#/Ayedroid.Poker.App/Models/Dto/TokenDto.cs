﻿namespace Ayedroid.Poker.App.Models.Dto
{
    public class TokenDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
    }
}
