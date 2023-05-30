using DAL.Models;
using DAL.Models.Requests;

namespace PublicModule.Services
{
    public interface IUserRepository
    {
        User Create(UserRegisterRequest request);
        bool Authenticate(string username, string password);
        void ChangePassword(ChangePasswordRequest request);
    }
}
