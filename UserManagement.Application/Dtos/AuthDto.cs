﻿namespace UserManagement.Application.Dtos;

public class AuthDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}