﻿using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WebApi;

public static class AuthOptions
{
    public const string Issuer = "MyAuthServer";
    public const string Audience = "MyAuthClient";
    const string Key = "mysupersecret_secretkey!123";
    public const int Lifetime = 100000;

    public static SymmetricSecurityKey GetAsymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
    }
}
