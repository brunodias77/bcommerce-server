using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bcomerce.Application.Abstractions;
using Bcommerce.Domain.Categories.Repositories;
using Bcommerce.Domain.Validations;
using Bcommerce.Domain.Validations.Handlers;
using Bcommerce.Infrastructure.Data.Repositories;

namespace Bcomerce.Application.UseCases.Catalog.DeleteCategories
{
    public class DeleteCategoryUseCase : IDeleteCategoryUseCase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _uow;

        public DeleteCategoryUseCase(ICategoryRepository categoryRepository, IUnitOfWork uow)
        {
            _categoryRepository = categoryRepository;
            _uow = uow;
        }

        public async Task<Result<bool, Notification>> Execute(Guid input)
        {
            var notification = Notification.Create();
            var category = await _categoryRepository.Get(input, CancellationToken.None);

            if (category is null)
            {
                notification.Append(new Error("Categoria não encontrada."));
                return Result<bool, Notification>.Fail(notification);
            }

            // Aqui você poderia adicionar lógicas, como não permitir deletar categorias com produtos.

            await _uow.Begin();
            await _categoryRepository.Delete(category, CancellationToken.None);
            await _uow.Commit();

            return Result<bool, Notification>.Ok(true);
        }
    }
}
