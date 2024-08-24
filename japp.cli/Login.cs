using japp.lib;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.CommandLine;

namespace japp.cli;

public class Login : Command
{
    private readonly ILogger log;
    private readonly IConfiguration config;

    public Login(ILogger log, IConfiguration config) : base("login", "Login to registry")
    {
        this.log = log;
        this.config = config;

        Option username = new Option<string>(["--username", "-u"], "User name")
        {
            IsRequired = true
        };
        AddOption(username);

        Option password = new Option<string>(["--password", "-p"], "Password")
        {
            IsRequired = false
        };
        AddOption(password);

        this.SetHandler((string username, string password) => HandleLogin(username, password), username, password);
    }

    private int HandleLogin(string username, string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            password = GetPassword();
        }

        string reductedPassword = new string('*', password.Length);

        log.Debug("Login: username={username}, password={password}", username, reductedPassword);

        return new Japp(log, config).Login(username, password);
    }

    private string GetPassword() // private SecureString GetPassword()
    {
        Console.Write("Password: ");

        var password = string.Empty; // var password = new SecureString();
        while (true)
        {
            ConsoleKeyInfo input = Console.ReadKey(true);
            if (input.Key == ConsoleKey.Enter)
            {
                break;
            }
            else if (input.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Remove(password.Length - 1); // password.RemoveAt(password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            else if (input.KeyChar != '\u0000' ) // KeyChar == '\u0000' if the key pressed does not correspond to a printable character, e.g. F1, Pause-Break, etc
            {
                password += input.KeyChar; // password.AppendChar(input.KeyChar);
                Console.Write("*");
            }
        }

        Console.WriteLine();

        return password;
    }
}
