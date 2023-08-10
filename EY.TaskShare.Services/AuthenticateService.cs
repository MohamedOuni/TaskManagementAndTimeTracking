using EY.TaskShare;
using EY.TaskShare.Entities;
using EY.TaskShare.Services.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthenticateService
{
    private readonly TaskShareContext dbContext;

    public AuthenticateService(TaskShareContext dbContext)
    {
        this.dbContext = dbContext;
    }
    public void CreateUser(User user)
    {
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.PasswordHash = passwordHash;
        dbContext.Users.Add(user);
        dbContext.SaveChanges();
    }
    public Tuple<bool, string> LoginUser(UserDetails req)
    {
        var user = dbContext.Users.FirstOrDefault(u => u.UserName == req.UserName);
        try
        {
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(req.PasswordHash, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Incorrect password");
            }
            string token = CreateToken(user);
            return new Tuple<bool, string>(true, token.Trim());
        }

        catch (Exception)
        {
            return new Tuple<bool, string>(false, string.Empty);
        }
    }
    public User ValidateTokenAndGetUser(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("this is my secret key for authentication");

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        SecurityToken validatedToken;
        var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

        var claimsIdentity = principal.Identity as ClaimsIdentity;
        if (claimsIdentity == null || !claimsIdentity.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Invalid token or user authentication");
        }

        var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Invalid user identifier");
        }
        if (!int.TryParse(userId, out int userIdInt))
        {
            throw new UnauthorizedAccessException("Invalid user identifier format");
        }
        var userRole = claimsIdentity.FindFirst(ClaimTypes.Role)?.Value;
        if (!Enum.TryParse(userRole, out Role role))
        {
            throw new UnauthorizedAccessException("Invalid user role");
        }
        var userTeam = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
        if (!Enum.TryParse(userTeam, out Team team))
        {
            throw new UnauthorizedAccessException("Invalid user team");
        }

        var user = dbContext.Users.FirstOrDefault(u => u.Id == userIdInt && u.Role == role && u.Team == team);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found or unauthorized role or unauthorized team");
        }

        return user;
    }
    private string CreateToken(User user)
    {
        List<Claim> claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Role, user.Role.ToString()),
        new Claim(ClaimTypes.Name, user.Team.ToString())
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is my secret key for authentication"));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }


}