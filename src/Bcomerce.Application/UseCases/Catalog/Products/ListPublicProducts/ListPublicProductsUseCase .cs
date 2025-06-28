using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Catalog.Brands.Repositories;
using Bcommerce.Domain.Catalog.Categories.Repositories;
using Bcommerce.Domain.Catalog.Products.Repositories;
using Bcommerce.Domain.Validation.Handlers;

namespace Bcomerce.Application.UseCases.Catalog.Products.ListPublicProducts;

public class ListPublicProductsUseCase : IListPublicProductsUseCase
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;

    public ListPublicProductsUseCase(IProductRepository productRepository, ICategoryRepository categoryRepository, IBrandRepository brandRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
    }

    public async Task<Result<PagedListOutput<PublicProductSummaryOutput>, Notification>> Execute(ListPublicProductsInput input)
    {
        Guid? categoryId = null;
        if (!string.IsNullOrWhiteSpace(input.CategorySlug))
        {
            var category = await _categoryRepository.GetBySlugAsync(input.CategorySlug, CancellationToken.None);
            if (category != null) categoryId = category.Id;
        }

        Guid? brandId = null;
        if (!string.IsNullOrWhiteSpace(input.BrandSlug))
        {
            var brand = await _brandRepository.GetBySlugAsync(input.BrandSlug, CancellationToken.None);
            if (brand != null) brandId = brand.Id;
        }

        var products = await _productRepository.ListAsync(input.Page, input.PageSize, input.SearchTerm, categoryId, brandId, input.SortBy, input.SortDirection, CancellationToken.None);
        var totalCount = await _productRepository.CountAsync(input.SearchTerm, categoryId, brandId, CancellationToken.None);

        var summaryOutput = products.Select(p => new PublicProductSummaryOutput(
            p.Id,
            p.Name,
            p.Slug,
            p.BasePrice.Amount,
            p.SalePrice?.Amount,
            p.Images.FirstOrDefault(i => i.IsCover)?.ImageUrl
        )).ToList();

        var pagedList = new PagedListOutput<PublicProductSummaryOutput>(
            input.Page,
            input.PageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)input.PageSize),
            summaryOutput
        );

        return Result<PagedListOutput<PublicProductSummaryOutput>, Notification>.Ok(pagedList);
    }
}