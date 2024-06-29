using CityInfoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CityInfoAPI.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        public IConfiguration _configuration { get; }

        public AuthenticationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }



        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate(AuthenticationRequestBody authenticationRequestBody)
        {
            // Step 1: Validate the user credentials
            var user = ValidateUserCredentials(authenticationRequestBody.UserName, authenticationRequestBody.Password);
            if (user == null)
            {
                return Unauthorized();
            }

            // Step 2: Create the security key
            var securityKey = new SymmetricSecurityKey(
                Convert.FromBase64String(_configuration["Authentication:SecretForKey"]));

            // Step 3: Create the signature credentials using the security key
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Step 4: The claims
            var claimsForToken = new List<Claim>();
            claimsForToken.Add(new Claim("sub", user.UserId.ToString()));
            claimsForToken.Add(new Claim("given_name", user.FirstName));
            claimsForToken.Add(new Claim("family_name", user.LastName));
            claimsForToken.Add(new Claim("city", user.City));

            // Step 5: Create the token
            var jwtSecurityToken = new JwtSecurityToken(
                               issuer: _configuration["Authentication:Issuer"],
                               audience: _configuration["Authentication:Audience"],
                               claims: claimsForToken,
                               notBefore: DateTime.UtcNow,
                               expires: DateTime.UtcNow.AddHours(1),
                               signingCredentials: signingCredentials);

            // Step 6: Return the token
            var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return Ok(new
            {
                access_token = tokenToReturn
            });
        }

        private CityInfoUser ValidateUserCredentials(string? userName, string? password)
        {
            return new CityInfoUser(1, userName ?? "", "Rahim", "Pamelo", "Antwerp");
        }
    }
}
