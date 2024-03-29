﻿using DAL.BLModels;
using DAL.Models;
using DAL.Models.Requests;

namespace DAL.Services
{
    public interface IUserRepository
    {
        IEnumerable<BLUser> GetAll();
        void Edit(int id, BLUser user);

        bool Authenticate(string username, string password);

        BLUser Create(UserRegisterRequest request);
        void ValidateEmail(ValidateEmailRequest request);
        Tokens JwtTokens(JwtTokenRequest request);
        void ChangePassword(ChangePasswordRequest request);

        BLUser CreateUser(string username, string firstName, string lastName, string email, string password, int countryId);
        void ConfirmEmail(string email, string securityToken);
        BLUser GetConfirmedUser(string username, string password);
        void ChangePassword(string username, string newPassword);

        BLUser GetUser(int id);
        void SoftDeleteUser(int id);
        bool CheckUsernameExists(string username);
        bool CheckEmailExists(string email);
    }
}
