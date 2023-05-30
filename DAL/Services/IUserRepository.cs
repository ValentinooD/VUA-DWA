using DAL.BLModels;
using DAL.Models;
using DAL.Models.Requests;

namespace DAL.Services
{
    public interface IUserRepository
    {
        List<BLUser> GetAll();
        void Edit(int id, BLUser user);

        BLUser Create(UserRegisterRequest request);
        void ValidateEmail(ValidateEmailRequest request);
        Tokens JwtTokens(JwtTokenRequest request);
        void ChangePassword(ChangePasswordRequest request);
    }
}
