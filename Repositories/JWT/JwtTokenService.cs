using InventoryManagementAPI.Models.CoreModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InventoryManagementAPI.Repositories.JWT
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }
        public string GenerateToken(User user)
        {
            //Create claims based on the user information
            //Claims are pieces of information about the user that will be included in the token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.ID.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            //Create a symmetric security key using the secret key from the JWT settings
            //The key is used to sign the token and verify its authenticity
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

            //Create signing credentials using the security key and the HMAC-SHA256 algorithm
            //Signing credentials are used to sign the token and ensure its integrity
            //The HMAC-SHA256 algorithm is a widely used cryptographic algorithm for signing tokens
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //Assign an expiration time for the token based on the duration specified in the JWT settings
            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

            //Create the JWT token using the claims, signing credentials, and expiration time
            //The JwtSecurityToken class represents a JSON Web Token and provides methods for creating and validating tokens
            //The issuer and audience are also specified to indicate the intended recipient of the token
            
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            //Return the serialized token as a string
            //The WriteToken method of the JwtSecurityTokenHandler class is used to serialize the token into a string format that can be sent to the client
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
