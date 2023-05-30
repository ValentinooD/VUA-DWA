using DAL.Models;
using DAL.Models.Requests;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PublicModule.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly RwaMoviesContext ctx;
        private readonly IConfiguration configuration;

        public UserRepository(IServiceScopeFactory factory)
        {
            var provider = factory.CreateScope().ServiceProvider;
            this.ctx = provider.GetRequiredService<RwaMoviesContext>();
            this.configuration = provider.GetRequiredService<IConfiguration>();
        }

        public User Create(UserRegisterRequest request)
        {
            // We do safety checks first
            if (ctx.Users.Any(x => x.Username.Equals(request.Username.ToLower().Trim())))
                throw new InvalidOperationException("Username already exists.");
            
            if (ctx.Users.Any(x => x.Email.Equals(request.Email.ToLower().Trim())))
                throw new InvalidOperationException("This E-Mail is already in use.");

            Country? country = ctx.Countries.FirstOrDefault(x => x.Code.Equals(request.CountryCode.Trim().ToUpper()));
            if (country == null)
                throw new InvalidOperationException("Country does not exist.");

            string hash;
            string salt;
            ComputeHashAndSalt(request.Password, out salt, out hash);

            byte[] securityToken = RandomNumberGenerator.GetBytes(256 / 8);
            string b64SecToken = Convert.ToBase64String(securityToken);

            var newUser = new User
            {
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                CountryOfResidenceId = country.Id,
                CreatedAt = DateTime.UtcNow,

                IsConfirmed = false,
                SecurityToken = b64SecToken,
                PwdSalt = salt,
                PwdHash = hash
            };

            ctx.Users.Add(newUser);
            ctx.SaveChanges();

            return newUser;
        }

        public bool Authenticate(string username, string password)
        {
            var target = ctx.Users.Single(x => x.Username == username);

            if (!target.IsConfirmed)
                return false;

            byte[] salt = Convert.FromBase64String(target.PwdSalt);
            byte[] hash = Convert.FromBase64String(target.PwdHash);

            byte[] calcHash =
                KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8);

            return hash.SequenceEqual(calcHash);
        }

        public void ChangePassword(ChangePasswordRequest request)
        {
            if (!Authenticate(request.Username, request.OldPassword))
                throw new InvalidOperationException("Authentication failed");

            string hash;
            string salt;
            ComputeHashAndSalt(request.NewPassword, out salt, out hash);

            // Update user
            var target = ctx.Users.Single(x => x.Username == request.Username);
            target.PwdSalt = salt;
            target.PwdHash = hash;
            ctx.SaveChanges();
        }

        private void ComputeHashAndSalt(string password, out string salt, out string hash)
        {
            byte[] bytesSalt = RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes
            string b64Salt = Convert.ToBase64String(bytesSalt);

            byte[] bytesHash =
                KeyDerivation.Pbkdf2(
                    password: password,
                    salt: bytesSalt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8);
            string b64Hash = Convert.ToBase64String(bytesHash);

            salt = b64Salt;
            hash = b64Hash;
        }
    }
}
