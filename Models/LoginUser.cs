using System;
using System.ComponentModel.DataAnnotations;
public class LoginUser
{
    // No other fields!
    [EmailAddress]
    [Required]
    public string Email {get;set;}
    [Required]
    public string Password {get;set;}
}
