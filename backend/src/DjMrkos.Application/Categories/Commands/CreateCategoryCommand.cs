using DjMrkos.Application.Categories.Dtos;
using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Domain.Modules;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Categories.Commands;

public sealed record CreateCategoryCommand(Guid ModuleId, string Name, string? Description, string? ImageUrl, int DisplayOrder)
    : IRequest<CategoryDto>;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.ModuleId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateCategoryCommandHandler(ICategoryRepository categories, IModuleRepository modules)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        _ = await modules.GetByIdAsync(request.ModuleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Module), request.ModuleId);

        var category = Category.Create(request.ModuleId, request.Name, request.Description, request.ImageUrl, request.DisplayOrder);
        var created = await categories.AddAsync(category, cancellationToken);
        return CategoryDto.From(created);
    }
}
