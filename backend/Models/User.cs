namespace backend.Models;

// maps directly to the Users table — Password holds the BCrypt hash,
// never plain text
public class User
{
    public int Id {get; set;}
    public string Username {get; set;} = "";
    public string Email {get; set;} = "";   
    public string Password {get; set;} = "";
}

public class RegisterDto
{
    public string Username {get; set;} = "";
    public string Email {get; set;} = "";   
    public string Password {get; set;} = "";
}

// what the frontend sends when logging in
public class LoginDto
{
    public string Email {get; set;} = "";   
    public string Password {get; set;} = "";
}