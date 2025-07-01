
using Bcomerce.Application.UseCases.Catalog.Categories.CreateCategory;
using Bcomerce.Application.UseCases.Catalog.Categories.DeleteCategory;
using Bcomerce.Application.UseCases.Catalog.Categories.GetCategoryById;
using Bcomerce.Application.UseCases.Catalog.Categories.ListCategories;
using Bcomerce.Application.UseCases.Catalog.Categories.UpdateCategory;
using Bcomerce.Application.UseCases.Catalog.Clients.AddAddress;
using Bcomerce.Application.UseCases.Catalog.Clients.Create;
using Bcomerce.Application.UseCases.Catalog.Clients.DeleteAddress;
using Bcomerce.Application.UseCases.Catalog.Clients.GetMyProfile;
using Bcomerce.Application.UseCases.Catalog.Clients.ListAddresses;
using Bcomerce.Application.UseCases.Catalog.Clients.Login;
using Bcomerce.Application.UseCases.Catalog.Clients.Logout;
using Bcomerce.Application.UseCases.Catalog.Clients.UpdateAddress;
using Bcomerce.Application.UseCases.Catalog.Clients.VerifyEmail;
using Bcomerce.Application.UseCases.Catalog.Products.CreateProduct;
using Bcomerce.Application.UseCases.Catalog.Products.GetProductById;
using Bcomerce.Application.UseCases.Catalog.Products.GetPublicProduct;
using Bcomerce.Application.UseCases.Catalog.Products.ListProducts;
using Bcomerce.Application.UseCases.Catalog.Products.ListPublicProducts;
using Bcomerce.Application.UseCases.Catalog.Products.UpdateProduct;
using Bcomerce.Application.UseCases.Marketing.Coupons.ApplyCoupon;
using Bcomerce.Application.UseCases.Sales.Carts.AddItemToCart;
using Bcomerce.Application.UseCases.Sales.Carts.GetCart;
using Bcomerce.Application.UseCases.Sales.Carts.RemoveCartItem;
using Bcomerce.Application.UseCases.Sales.Carts.UpdateCartItemQuantity;
using Bcomerce.Application.UseCases.Sales.Orders.CreateOrder;
using Bcomerce.Application.UseCases.Sales.Orders.ProcessPayment;
using Bcommerce.Application.Events.Clients;
using Bcommerce.Domain.Common;
using Bcommerce.Domain.Customers.Clients.Events;


namespace Bcommerce.Api.Configurations;

public static class ApplicationDependencyInjection
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddUseCases(services);
        AddEvents(services, configuration);
    }

    private static void AddUseCases(IServiceCollection services)
    {
        // Auth
        services.AddScoped<ICreateClientUseCase, CreateClientUseCase>();
        services.AddScoped<IVerifyEmailUseCase, VerifyEmailUseCase>();
        services.AddScoped<ILoginClientUseCase, LoginClientUseCase>();
        services.AddScoped<IGetMyProfileUseCase, GetMyProfileUseCase>();
        services.AddScoped<IAddAddressUseCase, AddAddressUseCase>();
        services.AddScoped<IListMyAddressesUseCase, ListMyAddressesUseCase>();
        services.AddScoped<IUpdateAddressUseCase, UpdateAddressUseCase>();
        services.AddScoped<IDeleteAddressUseCase, DeleteAddressUseCase>();
        services.AddScoped<ILogoutUseCase, LogoutUseCase>();
        services.AddScoped<ICreateCategoryUseCase, CreateCategoryUseCase>();
        services.AddScoped<IListCategoriesUseCase, ListCategoriesUseCase>();
        services.AddScoped<IDeleteCategoryUseCase, DeleteCategoryUseCase>();
        services.AddScoped<IGetCategoryByIdUseCase, GetCategoryByIdUseCase>();
        services.AddScoped<IUpdateCategoryUseCase, UpdateCategoryUseCase>();
        services.AddScoped<ICreateProductUseCase, CreateProductUseCase>();
        services.AddScoped<IGetProductByIdUseCase, GetProductByIdUseCase>();
        services.AddScoped<IListProductsUseCase, ListProductsUseCase>();
        services.AddScoped<IUpdateProductUseCase, UpdateProductUseCase>();
        services.AddScoped<IGetPublicProductBySlugUseCase, GetPublicProductBySlugUseCase>(); // <-- ADICIONE
        services.AddScoped<IListPublicProductsUseCase, ListPublicProductsUseCase>(); // <-- ADICIONE
        services.AddScoped<IAddItemToCartUseCase, AddItemToCartUseCase>(); // <-- ADICIONE
        services.AddScoped<IGetCartUseCase, GetCartUseCase>();
        services.AddScoped<IUpdateCartItemQuantityUseCase, UpdateCartItemQuantityUseCase>();
        services.AddScoped<IRemoveCartItemUseCase, RemoveCartItemUseCase>();
        services.AddScoped<ICreateOrderUseCase, CreateOrderUseCase>();
        services.AddScoped<IApplyCouponUseCase, ApplyCouponUseCase>();
        services.AddScoped<IProcessPaymentUseCase, ProcessPaymentUseCase>();

    }

    private static void AddEvents(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDomainEventHandler<ClientCreatedEvent>, ClientCreatedEventHandler>();
    }

}