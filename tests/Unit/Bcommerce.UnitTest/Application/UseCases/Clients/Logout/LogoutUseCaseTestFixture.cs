using Bcomerce.Application.UseCases.Catalog.Clients.Logout;
using Bcommerce.Domain.Security;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.Logout
{
    [CollectionDefinition(nameof(LogoutUseCaseTestFixture))]
    public class LogoutUseCaseTestFixtureCollection : ICollectionFixture<LogoutUseCaseTestFixture> { }

    public class LogoutUseCaseTestFixture
    {
        public Mock<IRevokedTokenRepository> RevokedTokenRepositoryMock { get; }
        // CORREÇÃO: Mock da dependência correta
        public Mock<IHttpContextAccessor> HttpContextAccessorMock { get; }

        public LogoutUseCaseTestFixture()
        {
            RevokedTokenRepositoryMock = new Mock<IRevokedTokenRepository>();
            HttpContextAccessorMock = new Mock<IHttpContextAccessor>();
        }

        public LogoutUseCase CreateUseCase()
        {
            // CORREÇÃO: Injetando a dependência correta
            return new LogoutUseCase(HttpContextAccessorMock.Object, RevokedTokenRepositoryMock.Object);
        }

        /// <summary>
        /// Configura o mock do IHttpContextAccessor para simular um usuário autenticado com claims específicas.
        /// </summary>
        public void SetupAuthenticatedUser(Guid jti, Guid clientId, DateTime expiresAt)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, jti.ToString()),
                new(ClaimTypes.NameIdentifier, clientId.ToString()),
                new(JwtRegisteredClaimNames.Exp, new DateTimeOffset(expiresAt).ToUnixTimeSeconds().ToString())
            }, "TestAuthentication"));

            var httpContext = new DefaultHttpContext { User = user };
            HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        }

        /// <summary>
        /// Configura o mock para simular um usuário não autenticado.
        /// </summary>
        public void SetupUnauthenticatedUser()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity()); // Usuário sem identidade autenticada
            var httpContext = new DefaultHttpContext { User = user };
            HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        }
    }
}