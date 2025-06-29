using Bcommerce.Domain.Catalog.Products.ValueObjects;
using Bcommerce.Domain.Exceptions;
using Bcommerce.Domain.Sales.Carts;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace Bcommerce.UnitTest.Domain.Entities.Carts
{
    [Collection(nameof(CartTestFixture))]
    public class CartUnitTest
    {
        private readonly CartTestFixture _fixture;

        public CartUnitTest(CartTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(DisplayName = "Deve Criar Carrinho com Sucesso")]
        [Trait("Domain", "Cart - Aggregate")]
        public void NewCart_WithValidClientId_ShouldCreateSuccessfully()
        {
            // Arrange
            var clientId = Guid.NewGuid();

            // Act
            var cart = Cart.NewCart(clientId);

            // Assert
            cart.Should().NotBeNull();
            cart.ClientId.Should().Be(clientId);
            cart.Items.Should().BeEmpty();
        }

        [Fact(DisplayName = "Deve Adicionar Novo Item ao Carrinho")]
        [Trait("Domain", "Cart - Aggregate")]
        public void AddItem_WhenItemIsNew_ShouldAddItemToCollection()
        {
            // Arrange
            var cart = _fixture.CreateValidCart();
            var (productVariantId, quantity, unitPrice) = _fixture.GetValidCartItemInput();

            // Act
            cart.AddItem(productVariantId, quantity, unitPrice);

            // Assert
            cart.Items.Should().HaveCount(1);
            var item = cart.Items.First();
            item.ProductVariantId.Should().Be(productVariantId);
            item.Quantity.Should().Be(quantity);
            item.Price.Should().Be(unitPrice);
            cart.GetTotalPrice().Amount.Should().Be(quantity * unitPrice.Amount);
        }

        [Fact(DisplayName = "Deve Incrementar Quantidade de Item Existente")]
        [Trait("Domain", "Cart - Aggregate")]
        public void AddItem_WhenItemAlreadyExists_ShouldIncreaseQuantity()
        {
            // Arrange
            var cart = _fixture.CreateValidCart();
            var (productVariantId, _, unitPrice) = _fixture.GetValidCartItemInput();
            cart.AddItem(productVariantId, 2, unitPrice); // Quantidade inicial: 2

            // Act
            cart.AddItem(productVariantId, 3, unitPrice); // Adiciona mais 3 do mesmo item

            // Assert
            cart.Items.Should().HaveCount(1);
            cart.Items.First().Quantity.Should().Be(5); // 2 + 3 = 5
            cart.GetTotalPrice().Amount.Should().Be(5 * unitPrice.Amount);
        }

        [Fact(DisplayName = "Deve Remover Item do Carrinho")]
        [Trait("Domain", "Cart - Aggregate")]
        public void RemoveItem_WhenItemExists_ShouldRemoveItem()
        {
            // Arrange
            var cart = _fixture.CreateValidCart();
            var (productVariantId, quantity, price) = _fixture.GetValidCartItemInput();
            cart.AddItem(productVariantId, quantity, price);
            var itemToRemove = cart.Items.First();

            // Act
            // CORREÇÃO: Usando o ID do item do carrinho (cartItemId), não o ID da variante do produto.
            cart.RemoveItem(itemToRemove.Id);

            // Assert
            cart.Items.Should().BeEmpty();
        }
        
        [Fact(DisplayName = "Deve Atualizar Quantidade de Item")]
        [Trait("Domain", "Cart - Aggregate")]
        public void UpdateItemQuantity_WhenItemExists_ShouldUpdateQuantity()
        {
            // Arrange
            var cart = _fixture.CreateValidCart();
            var (productVariantId, _, price) = _fixture.GetValidCartItemInput();
            cart.AddItem(productVariantId, 2, price); // Total inicial = 2 * price
            var itemToUpdate = cart.Items.First();
            var newQuantity = 5;

            // Act
            // CORREÇÃO: Usando o ID do item do carrinho.
            cart.UpdateItemQuantity(itemToUpdate.Id, newQuantity);

            // Assert
            cart.Items.First().Quantity.Should().Be(newQuantity);
            cart.GetTotalPrice().Amount.Should().Be(newQuantity * price.Amount);
        }
        
        [Fact(DisplayName = "Deve Remover Item se Quantidade Atualizada for Zero")]
        [Trait("Domain", "Cart - Aggregate")]
        public void UpdateItemQuantity_ToZero_ShouldRemoveItem()
        {
            // Arrange
            var cart = _fixture.CreateValidCart();
            var (productVariantId, _, price) = _fixture.GetValidCartItemInput();
            cart.AddItem(productVariantId, 2, price);
            var itemToUpdate = cart.Items.First();

            // Act
            cart.UpdateItemQuantity(itemToUpdate.Id, 0);

            // Assert
            cart.Items.Should().BeEmpty();
        }

        [Fact(DisplayName = "Não Deve Atualizar Item Inexistente e Deve Lançar Exceção")]
        [Trait("Domain", "Cart - Aggregate")]
        public void UpdateItemQuantity_WhenItemDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var cart = _fixture.CreateValidCart();
            var nonExistentItemId = Guid.NewGuid();

            // Act
            Action action = () => cart.UpdateItemQuantity(nonExistentItemId, 5);

            // Assert
            action.Should().Throw<DomainException>()
                .WithMessage("Item não encontrado no carrinho.");
        }

        [Fact(DisplayName = "Deve Limpar todos os Itens do Carrinho")]
        [Trait("Domain", "Cart - Aggregate")]
        public void Clear_WhenCalled_ShouldRemoveAllItems()
        {
            // Arrange
            var cart = _fixture.CreateValidCart();
            cart.AddItem(Guid.NewGuid(), 2, Money.Create(50m));
            cart.AddItem(Guid.NewGuid(), 1, Money.Create(75m));
            cart.Items.Should().NotBeEmpty();

            // Act
            cart.Clear();

            // Assert
            cart.Items.Should().BeEmpty();
            cart.GetTotalPrice().Amount.Should().Be(0m);
        }
    }
}