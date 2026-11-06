using Microsoft.AspNetCore.Identity;
using YadgarCafe.Application.DTOs.Auth;
using YadgarCafe.Application.Interfaces;
using YadgarCafe.Common.Constants;
using YadgarCafe.Common.Exceptions;
using YadgarCafe.Domain.Entities;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;

        public IdentityService(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn)
        {
            _users = users;
            _signIn = signIn;
        }

        public async Task<UserDto> CreateUserAsync(string email, string password, string fullName, string role)
        {
            var user = new ApplicationUser
            {
                UserName = email.Trim(),
                Email = email.Trim(),
                FullName = fullName.Trim(),
                UserType = role switch
                {
                    Roles.Admin => UserType.Admin,
                    Roles.Staff => UserType.Staff,
                    _ => UserType.Customer
                },
                // No email delivery exists yet, so accounts are active immediately.
                EmailConfirmed = true
            };

            var created = await _users.CreateAsync(user, password);
            if (!created.Succeeded)
                throw new BadRequestException(string.Join(" ", created.Errors.Select(e => e.Description)));

            var roleResult = await _users.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    "Could not assign role: " + string.Join(" ", roleResult.Errors.Select(e => e.Description)));

            return ToDto(user, role);
        }

        public async Task<UserDto> ValidateCredentialsAsync(string email, string password)
        {
            const string generic = "Invalid email or password.";

            var user = await _users.FindByEmailAsync(email.Trim());
            if (user is null)
                throw new UnauthorizedException(generic);

            // lockoutOnFailure: true throttles brute force attempts.
            var result = await _signIn.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                throw new UnauthorizedException("Account is temporarily locked after too many failed attempts. Try again later.");

            if (!result.Succeeded)
                throw new UnauthorizedException(generic);

            return ToDto(user, await GetRoleAsync(user));
        }

        public async Task<UserDto?> FindByIdAsync(Guid userId)
        {
            var user = await _users.FindByIdAsync(userId.ToString());
            return user is null ? null : ToDto(user, await GetRoleAsync(user));
        }

        public async Task<bool> EmailExistsAsync(string email) =>
            await _users.FindByEmailAsync(email.Trim()) is not null;

        private async Task<string> GetRoleAsync(ApplicationUser user) =>
            (await _users.GetRolesAsync(user)).FirstOrDefault() ?? Roles.Customer;

        private static UserDto ToDto(ApplicationUser user, string role) =>
            new(user.Id, user.Email!, user.FullName, role);
    }
}
