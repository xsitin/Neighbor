﻿using System;

namespace Common.Models;

public class SecurityToken
{
    public string Username { get; set; }
    public string Role { get; set; }
    public string AccessToken { get; set; }
    public DateTime ExpiredAt { get; set; }
}