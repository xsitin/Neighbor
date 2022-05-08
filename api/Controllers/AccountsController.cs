using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using api.Infrastructure;
using BoardCommon.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;

namespace api.Controllers
{
    [Route("accounts")]
    [ApiController]
    public class AccountsController : Controller
    {
        private IAccountRepository Repository { get; set; }
        private ILogger<AccountsController> logger;

        public AccountsController(IAccountRepository repository, ILogger<AccountsController> logger)
        {
            Repository = repository;
            this.logger = logger;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetPublicUserData(string id)
        {
            var user = await Repository.GetByIdAsync(ObjectId.Parse(id));
            return Json(new { Name = user?.Name, Id = user?.Id.ToString() });
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            await Repository.RemoveById(ObjectId.Parse(id));
            return NoContent();
        }


        [Authorize(Roles = "Administrator")]
        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(string id, string role)
        {
            await Repository.UpdateRole(ObjectId.Parse(id), role);
            return Ok();
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateLoginAndPassword(AccountAuth account)
        {
            var userLogin = User.Claims.FirstOrDefault(x => x.Type == "Login")?.Value;
            if (userLogin != account.Login && !User.IsInRole("Administrator")) return Forbid();
            await Repository.Update(new Account() { Login = account.Login, Password = account.Password });
            return Ok();
        }


        [HttpPost("registration")]
        public async Task<IActionResult> Registration(AccountRegistration accountData)
        {
            var account = new Account()
            {
                Name = accountData.Name,
                Id = ObjectId.GenerateNewId(),
                Login = accountData.Login,
                Password = accountData.Password,
                Role = "User"
            };
            if (await CreateUser(account))
                return await Login(account);

            return BadRequest("Account exist");
        }

        private async Task<bool> CreateUser(Account account)
        {
            var isExist = (await Repository.GetByLoginAsync(account.Login)) != null;
            if (isExist)
                return false;
            account.Id ??= ObjectId.GenerateNewId();
            account.Role = Roles.User;
            account = new Account
            {
                Name = account.Name, Password = account.Password, Login = account.Login, Role = account.Role,
                Id = account.Id
            };
            account.Password = account.GetHashedPassword();
            await Repository.CreateUserAsync(account);
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

            var response = new
            {
                AccessToken = encodedJwt,
                username = identity.Name,
                ExpiredAt = now.Add(TimeSpan.FromMinutes(AuthOptions.Lifetime)),
                role = identity.FindFirst(nameof(Account.Role))?.Value
            };
            logger.Log(LogLevel.Information, "loggined {Time}",
                DateTime.Now.ToLocalTime().ToString(CultureInfo.InvariantCulture));


            return Json(response);
        }

        private async Task<ClaimsIdentity?> GetIdentity(string login, string password)
        {
            var person = await Repository.GetByLoginAsync(login);
            if (person == null)
                return null;

            var hasher = new PasswordHasher<Account>();
            var verificationResult = hasher.VerifyHashedPassword(person, person.Password, password);
            if (verificationResult != PasswordVerificationResult.Success)
                return null;


            var claims = new List<Claim>
            {
                new(nameof(Account.Name), person.Name),
                new(nameof(Account.Role), person.Role)
            };

            var claimsIdentity =
                new ClaimsIdentity(
                    claims,
                    "Token",
                    nameof(Account.Name),
                    nameof(Account.Role));
            return claimsIdentity;
        }
    }
}