using DjMrkos.Application.Categories.Dtos;
using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Categories.Commands;

public sealed record UpdateCategoryCommand(Guid Id, string Name, string? Description, string? ImageUrl, decimal? Price, int DisplayOrder, bool IsActive)
    : IRequest<CategoryDto>;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0m).When(x => x.Price.HasValue);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateCategoryCommandHandler(ICategoryRepository repository) : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Modules.Category), request.Id);

        category.Update(request.Name, request.Description, request.ImageUrl, request.Price);
        category.MoveTo(request.DisplayOrder);
        if (request.IsActive) category.Activate(); else category.Deactivate();

        await repository.UpdateAsync(category, cancellationToken);
        return CategoryDto.From(category);
    }
}
