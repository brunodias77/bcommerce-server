using Bcommerce.Domain.Catalog.Products;
using Bcommerce.Domain.Sales.Carts;
using FluentAssertions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bcommerce.UnitTest.Application.UseCases.Sales.Carts.AddItemToCart
{
    [Collection(nameof(AddItemToCartUseCaseTestFixture))]
    public class AddItemToCartUseCaseTest
    {
        private readonly AddItemToCartUseCaseTestFixture _fixture;

        public AddItemToCartUseCaseTest(AddItemToCartUseCaseTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.LoggedUserMock.Invocations.Clear();
            _fixture.CartRepositoryMock.Invocations.Clear();
            _fixture.ProductRepositoryMock.Invocations.Clear();
            _fixture.UnitOfWorkMock.Invocations.Clear();
        }

               [Fact(DisplayName = "Deve Adicionar Item e Criar Novo Carrinho")]
        [Trait("Application", "AddItemToCart - UseCase")]
        public async Task Execute_WhenNoExistingCart_ShouldCreateNewCartAndAddItem()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var input = _fixture.GetValidInput();
            var product = _fixture.CreateValidProduct();
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.CartRepositoryMock.Setup(r => r.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync((Cart)null);
            _fixture.ProductRepositoryMock.Setup(r => r.Get(input.ProductVariantId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

            // --- CORREÇÃO: Configurando os mocks de escrita ---
            _fixture.CartRepositoryMock
                .Setup(r => r.Insert(It.IsAny<Cart>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _fixture.UnitOfWorkMock
                .Setup(uow => uow.Commit())
                .Returns(Task.CompletedTask);
            // --- FIM DA CORREÇÃO ---

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue(); // Agora vai passar!
            result.Value.Should().NotBeNull();
            result.Value!.Items.Should().HaveCount(1);
            result.Value.Items.First().ProductVariantId.Should().Be(input.ProductVariantId);
            _fixture.CartRepositoryMock.Verify(r => r.Insert(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Deve Adicionar Item a Carrinho Existente")]
        [Trait("Application", "AddItemToCart - UseCase")]
        public async Task Execute_WhenCartExists_ShouldAddItemAndUpdateCart()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var existingCart = _fixture.CreateValidCart(clientId);
            var input = _fixture.GetValidInput();
            var product = _fixture.CreateValidProduct();
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.CartRepositoryMock.Setup(r => r.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync(existingCart);
            _fixture.ProductRepositoryMock.Setup(r => r.Get(input.ProductVariantId, It.IsAny<CancellationToken>())).ReturnsAsync(product);
            
            // --- CORREÇÃO: Configurando os mocks de escrita ---
            _fixture.CartRepositoryMock
                .Setup(r => r.Update(It.IsAny<Cart>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _fixture.UnitOfWorkMock
                .Setup(uow => uow.Commit())
                .Returns(Task.CompletedTask);
            // --- FIM DA CORREÇÃO ---

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value!.Items.Should().HaveCount(1);
            _fixture.CartRepositoryMock.Verify(r => r.Update(existingCart, It.IsAny<CancellationToken>()), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        }

        [Fact(DisplayName = "Não Deve Adicionar Item se Produto Não For Encontrado")]
        [Trait("Application", "AddItemToCart - UseCase")]
        public async Task Execute_WhenProductNotFound_ShouldReturnError()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var input = _fixture.GetValidInput();
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.ProductRepositoryMock.Setup(r => r.Get(input.ProductVariantId, It.IsAny<CancellationToken>())).ReturnsAsync((Product)null);

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Produto não encontrado.");
            _fixture.CartRepositoryMock.Verify(r => r.Insert(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Never);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }

        [Fact(DisplayName = "Deve Fazer Rollback em Caso de Erro na Persistência")]
        [Trait("Application", "AddItemToCart - UseCase")]
        public async Task Execute_WhenPersistenceThrows_ShouldRollback()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var input = _fixture.GetValidInput();
            var product = _fixture.CreateValidProduct();
            var useCase = _fixture.CreateUseCase();

            _fixture.LoggedUserMock.Setup(u => u.GetClientId()).Returns(clientId);
            _fixture.CartRepositoryMock.Setup(r => r.GetByClientIdAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync((Cart)null);
            _fixture.ProductRepositoryMock.Setup(r => r.Get(input.ProductVariantId, It.IsAny<CancellationToken>())).ReturnsAsync(product);
            _fixture.CartRepositoryMock.Setup(r => r.Insert(It.IsAny<Cart>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("DB Error"));

            // Act
            var result = await useCase.Execute(input);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error!.GetErrors().Should().Contain(e => e.Message == "Não foi possível adicionar o item ao carrinho.");
            _fixture.UnitOfWorkMock.Verify(u => u.Rollback(), Times.Once);
            _fixture.UnitOfWorkMock.Verify(u => u.Commit(), Times.Never);
        }
    }
}