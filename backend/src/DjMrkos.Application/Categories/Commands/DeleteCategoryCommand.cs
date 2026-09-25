using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Categories.Commands;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest;

public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class DeleteCategoryCommandHandler(ICategoryRepository repository) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Modules.Category), request.Id);

        category.Deactivate();
        await repository.UpdateAsync(category, cancellationToken);
    }
}
