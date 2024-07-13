using Asp.Versioning;
using CityInfoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CityInfoAPI.Controllers
{
    [Route("api/v{version:apiVersion}/authentication")]
    [Produces("application/json", "application/xml")]
    [ApiController]
    [ApiVersion(1)]
    public class AuthenticationController : ControllerBase
    {
        public IConfiguration _configuration { get; }

        public AuthenticationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }



        /// <summary>
        /// user authentication.
        /// </summary>
        /// <param name="AuthenticationRequestBody"></param>
        /// <returns>user's token</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /PointOfInterest
        ///     {
        ///        "username": "RahimPamelo",
        ///        "password": "This is a relatively long sentence that acts as my password"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Generates the user's token</response>
        /// <response code="400">If the request properties is not correct</response>
        [HttpPost("authenticate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<string> Authenticate(AuthenticationRequestBody authenticationRequestBody)
        {
            // Step 1: Validate the user credentials
            var user = ValidateUserCredentials(authenticationRequestBody.Username, authenticationRequestBody.Password);
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
