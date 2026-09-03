using System;
using Microsoft.AspNetCore.Identity;

public class OrganiserModel
{
    public string CompanyName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
}

internal class Program
{
    static void Main(string[] args)
    {
        var organiser = new OrganiserModel
        {
            CompanyName = "Test Organiser",
            Email = "test@example.com"
        };

        var hasher = new PasswordHasher<OrganiserModel>();
        var hashedPassword = hasher.HashPassword(organiser, "TestAccount");

        Console.WriteLine("Hashed password:");
        Console.WriteLine(hashedPassword);
    }
}