// Name: LIM RUI YANG RYAN
// Admin No: 252206Z
// Tutorial Group: IT2508
using ArcaneVault.Api.Data;
using ArcaneVault.Api.Dtos;
using ArcaneVault.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.Api.Controllers;

/// <summary>
/// Account registration and credential verification.
///
/// This controller owns passwords end to end: it hashes them on registration and
/// verifies them on login. The Razor front end never sees a password hash - it sends
/// the credentials once, gets back a <see cref="UserDto"/>, and issues its own
/// authentication cookie from that.
/// </summary>
[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<ArcaneVaultUser> _passwordHasher;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(
        AppDbContext db,
        IPasswordHasher<ArcaneVaultUser> passwordHasher,
        ILogger<AccountsController> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    // POST api/accounts/register
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterInputDto input)
    {
        // Validation requirement: reject duplicate usernames (the primary key).
        if (await _db.ArcaneVaultUsers.AnyAsync(u => u.UserName == input.UserName))
        {
            return BadRequest(new { message = $"The username '{input.UserName}' is already taken." });
        }

        // Validation requirement: reject duplicate emails. Checked here for a friendly
        // message, and backed by a unique index on the column so it holds even if two
        // registrations race each other.
        if (await _db.ArcaneVaultUsers.AnyAsync(u => u.Email == input.Email))
        {
            return BadRequest(new { message = $"An account already exists for '{input.Email}'." });
        }

        var user = new ArcaneVaultUser
        {
            UserName = input.UserName,
            Email = input.Email,
            // Self-registration always creates a collector. Staff accounts are provisioned
            // by seeding, so a visitor can never grant themselves administrator rights.
            RoleId = RoleIds.User,
            IsDeleted = false,
        };

        // PBKDF2 hash with a per-user random salt, produced by ASP.NET Core's PasswordHasher.
        user.PasswordHash = _passwordHasher.HashPassword(user, input.Password);

        _db.ArcaneVaultUsers.Add(user);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Registered new account {UserName}", user.UserName);

        var role = await _db.ArcaneVaultUserRoles.FirstAsync(r => r.RoleId == user.RoleId);

        return CreatedAtAction(nameof(GetByUserName), new { userName = user.UserName }, new UserDto
        {
            UserName = user.UserName,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = role.RoleName,
        });
    }

    // POST api/accounts/login
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginInputDto input)
    {
        var user = await _db.ArcaneVaultUsers
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserName == input.UserName);

        // Deliberately identical message whether the username is unknown, the account is
        // deactivated, or the password is wrong - revealing which one would let an attacker
        // enumerate valid usernames.
        const string failureMessage = "Incorrect username or password.";

        if (user is null || user.IsDeleted)
        {
            return Unauthorized(new { message = failureMessage });
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, input.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Failed login attempt for {UserName}", input.UserName);
            return Unauthorized(new { message = failureMessage });
        }

        // If the stored hash used older parameters, PasswordHasher asks us to re-hash it
        // transparently on a successful login so credentials stay current.
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, input.Password);
            await _db.SaveChangesAsync();
        }

        _logger.LogInformation("Successful login for {UserName}", user.UserName);

        return Ok(new UserDto
        {
            UserName = user.UserName,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = user.Role.RoleName,
        });
    }

    // GET api/accounts/{userName}
    [HttpGet("{userName}")]
    public async Task<ActionResult<UserDto>> GetByUserName(string userName)
    {
        var user = await _db.ArcaneVaultUsers
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserName == userName);

        if (user is null)
        {
            return NotFound(new { message = $"No account found for '{userName}'." });
        }

        return Ok(new UserDto
        {
            UserName = user.UserName,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = user.Role.RoleName,
        });
    }
}
