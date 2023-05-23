using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using WebApi.Infrastructure;

namespace WebApi.Controllers;

using System.Text.Json;
using Mapster;
using Microsoft.AspNetCore.JsonPatch;
using SecurityToken = Common.Models.SecurityToken;

[Route("accounts")]
[ApiController]
public class AccountsController : Controller
{
    private IAccountRepository AccountRepository { get; }
    private IImageRepository ImageRepository { get; }
    private readonly ILogger<AccountsController> logger;


    public AccountsController(IAccountRepository accountRepository, ILogger<AccountsController> logger,
        IImageRepository imageRepository)
    {
        AccountRepository = accountRepository;
        this.logger = logger;
        ImageRepository = imageRepository;
    }


    [HttpGet("{login}")]
    public async Task<IActionResult> GetPublicUserData(string login)
    {
        var account = await AccountRepository.GetByLoginAsync(login);
        return Json(account.Adapt<AccountViewModel>());
    }

    [Authorize]
    [HttpGet("{login}/full")]
    public async Task<IActionResult> GetFullUserData(string login)
    {
        var userLogin = User.Claims.First(x => x.Type == nameof(Account.Login)).Value;
        if (userLogin != login && !User.IsInRole("Administrator"))
            return Forbid();

        var account = await AccountRepository.GetByLoginAsync(login);
        return Json(account);
    }

    [Authorize(Roles = "Administrator")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(string id)
    {
        await AccountRepository.Delete(new Account() { Id = id });
        return NoContent();
    }


    [Authorize(Roles = "Administrator")]
    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(string id, string role)
    {
        if (!Enum.TryParse<Role>(role, out var parsed)) return BadRequest("Invalid role");
        await AccountRepository.UpdateRole(id, parsed);
        return Ok();
    }

    [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateLoginAndPassword(AccountAuth account)
    {
        var userLogin = User.Claims.FirstOrDefault(x => x.Type == "Login")?.Value;
        if (userLogin != account.Login && !User.IsInRole("Administrator")) return Forbid();
        await AccountRepository.Update(account.Adapt<Account>());
        return Ok();
    }

    [Authorize]
    [HttpPatch("{login}")]
    public async Task<IActionResult> Update(string login, [FromBody] JsonPatchDocument patchDocument)
    {
        var account = await AccountRepository.GetByLoginAsync(login);
        if (account == null)
        {
            logger.LogTrace($"Not found account with login: {login}");
            return NotFound();
        }

        patchDocument.ApplyTo(account);
        if (patchDocument.Operations.Any(x => x.path == nameof(Account.Password)))
        {
            account.Password = account.GetHashedPassword();
        }


        await AccountRepository.Update(account);
        return Ok();
    }


    [HttpPost("registration")]
    public async Task<IActionResult> Registration()
    {
        var accountData =
            JsonSerializer.Deserialize<AccountRegistration>(Request.Form[nameof(AccountRegistration)].ToString(), new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
        if (accountData is null || Request.Form.Files.Count != 1)
            return BadRequest("Invalid account data");
        var account = accountData.Adapt<Account>();
        var image = await ImageRepository.Add(Request.Form.Files[0]);
        account.AvatarId = image.Id;
        if (await CreateUser(account))
            return await Login(account);

        return BadRequest("Account exist");
    }

    private async Task<bool> CreateUser(Account account)
    {
        var isExist = (await AccountRepository.GetByLoginAsync(account.Login)) != null;
        if (isExist)
            return false;
        account.Id ??= ObjectId.GenerateNewId().ToString();
        account.Role = Role.User;
        //account.Clone() saves unhashed password for autologin after registration
        account = (Account)account.Clone();
        account.Password = account.GetHashedPassword();
        await AccountRepository.Create(account);
        return true;
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(AccountAuth account)
    {
        var password = account.Password;
        var login = account.Login;
        var identity = await GetIdentity(login, password);
        if (identity == null)
            return BadRequest("Invalid username or password.");

        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.Issuer,
            audience: AuthOptions.Audience,
            notBefore: now,
            claims: identity.Claims,
            expires: now.ToLocalTime().Add(TimeSpan.FromMinutes(AuthOptions.Lifetime)),
            signingCredentials: new SigningCredentials(AuthOptions.GetAsymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256));

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        var response = new SecurityToken()
        {
            AccessToken = encodedJwt,
            Login = identity.Name,
            ExpiredAt = now.Add(TimeSpan.FromMinutes(AuthOptions.Lifetime)),
            Role = identity.FindFirst(nameof(Account.Role))?.Value
        };
        logger.Log(LogLevel.Information, "loggined {Time}",
            DateTime.Now.ToLocalTime().ToString(CultureInfo.InvariantCulture));


        return Json(response);
    }

    private async Task<ClaimsIdentity?> GetIdentity(string login, string password)
    {
        var person = await AccountRepository.GetByLoginAsync(login);
        if (person == null)
            return null;

        var hasher = new PasswordHasher<Account>();
        var verificationResult = hasher.VerifyHashedPassword(person, person.Password, password);
        if (verificationResult != PasswordVerificationResult.Success)
            return null;


        var claims = new List<Claim>
        {
            new(nameof(Account.Login), person.Login), new(nameof(Account.Role), person.Role.ToString())
        };

        var claimsIdentity =
            new ClaimsIdentity(
                claims,
                "Token",
                nameof(Account.Login),
                nameof(Account.Role));
        return claimsIdentity;
    }
}
