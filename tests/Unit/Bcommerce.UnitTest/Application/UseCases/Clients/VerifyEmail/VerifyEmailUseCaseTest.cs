using Bcommerce.Domain.Customers.Clients;
using Bcommerce.Domain.Customers.Clients.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Clients.VerifyEmail
{
    [Collection(nameof(VerifyEmailUseCaseTestFixture))]
    public class VerifyEmailUseCaseTest
    {
        private readonly VerifyEmailUseCaseTestFixture _fixture;

        public VerifyEmailUseCaseTest(VerifyEmailUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.ClientRepositoryMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
            _fixture.TokenRepositoryMock.Invocations.Clear();
        }

        [Fact(DisplayName = "Deve Verificar E-mail com Sucesso")]
        [Trait("Application", "VerifyEmail - UseCase")]
        public async Task Execute_WhenTokenIsValid_ShouldVerifyEmailAndCommit()
        {
            // Arrange
            var client = _fixture.CreateValidClient();
            var tokenEntity = _fixture.CreateValidTokenEntity(client.Id);
            var useCase = _fixture.CreateUseCase();

            // CORREÇÃO: Mock para o método correto 'GetByTokenHashAsync' retornando a entidade de domínio.
            _fixture.TokenRepositoryMock
                .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokenEntity);

            _fixture.ClientRepositoryMock
                .Setup(r => r.Get(client.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            // Act
            // CORREÇÃO: O input é uma string. O UseCase é responsável pelo hash.
            var result = await useCase.Execute("raw_token_string");

            // Assert
            result.IsSuccess.Should().BeTrue();
            // Verifica se o método de negócio na entidade foi chamado, resultando na data de verificação.
            _fixture.ClientRepositoryMock.Verify(r => r.Update(It.Is<Client>(c => c.EmailVerified != null), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.TokenRepositoryMock.Verify(r => r.DeleteAsync(tokenEntity, It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Verificar E-mail com Token Inválido")]
        [Trait("Application", "VerifyEmail - UseCase")]
        public async Task Execute_WhenTokenIsInvalid_ShouldReturnError()
        {
            // Arrange
            var useCase = _fixture.CreateUseCase();
            var invalidToken = "invalid-token";

            _fixture.TokenRepositoryMock
                .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EmailVerificationToken)null); // Token não encontrado

            // Act
            var result = await useCase.Execute(invalidToken);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Token de verificação inválido ou expirado.");
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }

        [Fact(DisplayName = "Não Deve Verificar E-mail com Token Expirado")]
        [Trait("Application", "VerifyEmail - UseCase")]
        public async Task Execute_WhenTokenIsExpired_ShouldReturnError()
        {
            // Arrange
            var client = _fixture.CreateValidClient();
            var expiredToken = _fixture.CreateValidTokenEntity(client.Id, isExpired: true); // Token expirado
            var useCase = _fixture.CreateUseCase();

            _fixture.TokenRepositoryMock
                .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expiredToken);
            
            // Act
            var result = await useCase.Execute("raw_token_string");
            
            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Token de verificação inválido ou expirado.");
            // Garante que o token expirado foi removido do banco
            _fixture.TokenRepositoryMock.Verify(r => r.DeleteAsync(expiredToken, It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once); // Commit para a deleção do token
        }
    }
}