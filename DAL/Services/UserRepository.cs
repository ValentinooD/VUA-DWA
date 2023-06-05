using AutoMapper;
using Azure.Core;
using DAL.BLModels;
using DAL.Models;
using DAL.Models.Requests;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DAL.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly IMapper mapper;
        private readonly RwaMoviesContext ctx;
        private readonly IConfiguration configuration;

        public UserRepository(IServiceScopeFactory factory)
        {
            var provider = factory.CreateScope().ServiceProvider;
            this.ctx = provider.GetRequiredService<RwaMoviesContext>();
            this.configuration = provider.GetRequiredService<IConfiguration>();
            this.mapper = provider.GetRequiredService<IMapper>();
        }

        public BLUser Create(UserRegisterRequest request)
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

            return mapper.Map<BLUser>(newUser);
        }

        public void ValidateEmail(ValidateEmailRequest request)
        {
            var target = ctx.Users.FirstOrDefault(x =>
                x.Username == request.Username && x.SecurityToken == request.B64SecToken);

            if (target == null)
                throw new InvalidOperationException("Authentication failed");

            target.IsConfirmed = true;
            ctx.SaveChanges();
        }

        public Tokens JwtTokens(JwtTokenRequest request)
        {
            if (!Authenticate(request.Username, request.Password))
                throw new InvalidOperationException("Authentication failed");

            // Get secret key bytes
            var jwtKey = configuration["JWT:Key"];
            var jwtKeyBytes = Encoding.UTF8.GetBytes(jwtKey);
            
            // Create a token descriptor (represents a token, kind of a "template" for token)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new System.Security.Claims.Claim[]
                {
                    new Claim(ClaimTypes.Name, request.Username),
                    new Claim(JwtRegisteredClaimNames.Sub, request.Username),
                }),
                Issuer = configuration["JWT:Issuer"],
                Audience = configuration["JWT:Audience"],
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(jwtKeyBytes),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            // Create token using that descriptor, serialize it and return it
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var serializedToken = tokenHandler.WriteToken(token);

            return new Tokens
            {
                Token = serializedToken
            };
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

        private bool Authenticate(string username, string password)
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

        public IEnumerable<BLUser> GetAll()
        {
            return (IEnumerable<BLUser>)mapper.Map<IEnumerable<BLUser>>(ctx.Users.ToList());
        }

        public void Edit(int id, BLUser user)
        {
            User dbUser = ctx.Users.FirstOrDefault(x => x.Id == id);
            
            if (dbUser == null) return;

            if (ctx.Users.Any(x => x.Username.Equals(user.Username.ToLower().Trim())))
                throw new InvalidOperationException("Username already exists.");

            if (ctx.Users.Any(x => x.Email.Equals(user.Email.ToLower().Trim())))
                throw new InvalidOperationException("This E-Mail is already in use.");

            Country? country = ctx.Countries.FirstOrDefault(x => x.Id == user.CountryOfResidenceId);
            if (country == null)
                throw new InvalidOperationException("Country does not exist.");


            dbUser.Username = user.Username;
            dbUser.FirstName = user.FirstName;
            dbUser.LastName = user.LastName;
            dbUser.Email = user.Email;
            dbUser.CountryOfResidenceId = country.Id;

            ctx.SaveChanges();
        }

        public BLUser GetUser(int id)
        {
            var dbUser = ctx.Users.FirstOrDefault(x => x.Id == id);
            var blUser = mapper.Map<BLUser>(dbUser);

            return blUser;
        }

        public void SoftDeleteUser(int id)
        {
            var dbUser = ctx.Users.FirstOrDefault(x => x.Id == id);
            dbUser.DeletedAt = DateTime.UtcNow;

            ctx.SaveChanges();
        }

        public bool CheckUsernameExists(string username)
           => ctx.Users.Any(x => x.Username == username && x.DeletedAt == null);

        public bool CheckEmailExists(string email)
            => ctx.Users.Any(x => x.Email == email && x.DeletedAt == null);

        public BLUser CreateUser(string username, string firstName, string lastName, string email, string password)
        {
            string hash;
            string salt;
            ComputeHashAndSalt(password, out salt, out hash); 
            
            byte[] securityToken = RandomNumberGenerator.GetBytes(256 / 8);
            string b64SecToken = Convert.ToBase64String(securityToken);

            // Create BLUser object
            var dbUser = new User()
            {
                CreatedAt = DateTime.UtcNow,
                Username = username,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PwdHash = hash,
                PwdSalt = salt,
                SecurityToken = b64SecToken
            };
            ctx.Users.Add(dbUser);

            ctx.SaveChanges();

            var blUser = mapper.Map<BLUser>(dbUser);

            return blUser;
        }

        public void ConfirmEmail(string email, string securityToken)
        {
            var target = ctx.Users.FirstOrDefault(x =>
                x.Email == email && x.SecurityToken == securityToken);

            if (target == null)
                throw new InvalidOperationException("Authentication failed");

            target.IsConfirmed = true;
            ctx.SaveChanges();
        }

        public BLUser GetConfirmedUser(string username, string password)
        {
            var dbUser = ctx.Users.FirstOrDefault(x =>
                x.Username == username &&
                x.IsConfirmed == true);

            string hash;
            string salt;
            ComputeHashAndSalt(password, out salt, out hash);

            if (dbUser.PwdHash != hash)
                return null;

            var blUser = mapper.Map<BLUser>(dbUser);

            return blUser;
        }

        public void ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (!Authenticate(username, oldPassword))
                throw new InvalidOperationException("Authentication failed");

            string hash;
            string salt;
            ComputeHashAndSalt(newPassword, out salt, out hash);

            // Update user
            var target = ctx.Users.Single(x => x.Username == username);
            target.PwdSalt = salt;
            target.PwdHash = hash;
            ctx.SaveChanges();
        }
    }
}
