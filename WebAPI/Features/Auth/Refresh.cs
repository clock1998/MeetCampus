﻿using EmailService;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Template.WebAPI.Repositories;
using WebAPI.Features.Users;
using WebAPI.Infrastructure.Context;
using WebAPI.Infrastructure.Helper;

namespace WebAPI.Features.Auth
{
    public sealed record RefreshRequest(string Token, string RefreshToken);
    public sealed class RefreshValidator : AbstractValidator<RefreshRequest>
    {
        public RefreshValidator()
        {
            RuleFor(n => n.Token).NotEmpty();
            RuleFor(n => n.RefreshToken).NotEmpty();
        }
    }

    public class RefreshHandler
    {
        private readonly AuthHandler _authHandler;
        public RefreshHandler(AuthHandler authHandler)
        {
            _authHandler = authHandler;
        }

        public async Task<AppContextResponse> HandlerAsync(RefreshRequest request)
        {
            var principal = AuthHelper.GetPrincipalFromExpiredToken(request.Token, _authHandler.Configuration);
            var id = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new NullReferenceException();
            var user = await _authHandler.UserManager.FindByIdAsync(id);
            if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new InvalidOperationException("User is null or different refresh token or refresh token expired.");
            var newRefreshToken = AuthHelper.CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _authHandler.UserManager.UpdateAsync(user);
            var roles = await _authHandler.UserManager.GetRolesAsync(user);
            if (roles.Any())
            {
                var response = new AppContextResponse
                {
                    Token = AuthHelper.CreateToken(user, roles.ToList(), _authHandler.Configuration),
                    RefreshToken = newRefreshToken,
                    User = new UserReponse
                    {
                        Id = user.Id.ToString(),
                        Email = user.Email!,
                        FirstName = user.UserProfile.FirstName,
                        LastName = user.UserProfile.LastName,
                        Roles = user.UserRoles.Where(n => n.Role.Name != null).Select(n => n.Role.Name!).ToList(),
                    },
                };
                return response;
            }
            throw new InvalidOperationException("The user does not have a role.");
        }
    }

    public class RefreshController : AuthController
    {
        private readonly RefreshHandler _handler;
        private readonly IValidator<RefreshRequest> _validator;
        public RefreshController(RefreshHandler handler, ILogger<AuthController> logger, IValidator<RefreshRequest> validator)
        {
            _handler = handler;
            _validator = validator;
        }

        [HttpPost]
        [Route("RefreshToken")]
        [SwaggerOperation(Tags = new[] { "Auth" })]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest request)
        {
            var validatorResult = await _validator.ValidateAsync(request);
            if (!validatorResult.IsValid)
            {
                return Problem(detail: "Invalide input", instance: null, StatusCodes.Status400BadRequest, title: "Bad Request",
                     extensions: new Dictionary<string, object?>{
                        { "erros", validatorResult.Errors.Select(n => n.ErrorMessage).ToArray()}
                     });
            }
            try
            {
                var result = await _handler.HandlerAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception details
                //_logger.LogError(ex, "Error occurred while refreshing token");
                return Problem(detail: ex.Message, instance: null, statusCode: 500, title: "RefreshAsync", type: "Exception");
            }
        }
    }
}
