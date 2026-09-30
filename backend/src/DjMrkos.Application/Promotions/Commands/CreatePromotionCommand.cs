using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Promotions.Dtos;
using DjMrkos.Domain.Modules;
using DjMrkos.Domain.Promotions;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Promotions.Commands;

public sealed record CreatePromotionCommand(Guid? ModuleId, Guid? CategoryId, string Label, decimal DiscountPercentage)
    : IRequest<PromotionDto>;

public sealed class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DiscountPercentage).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x)
            .Must(x => (x.ModuleId is not null && x.ModuleId != Guid.Empty) != (x.CategoryId is not null && x.CategoryId != Guid.Empty))
            .WithMessage("Elige exactamente un módulo o una categoría, no ambos ni ninguno.");
    }
}

public sealed class CreatePromotionCommandHandler(IPromotionRepository promotions, IModuleRepository modules, ICategoryRepository categories)
    : IRequestHandler<CreatePromotionCommand, PromotionDto>
{
    public async Task<PromotionDto> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        if (request.ModuleId is { } moduleId)
            _ = await modules.GetByIdAsync(moduleId, cancellationToken) ?? throw new NotFoundException(nameof(Module), moduleId);
        if (request.CategoryId is { } categoryId)
            _ = await categories.GetByIdAsync(categoryId, cancellationToken) ?? throw new NotFoundException(nameof(Category), categoryId);

        var promotion = Promotion.Create(request.ModuleId, request.CategoryId, request.Label, request.DiscountPercentage);
        var created = await promotions.AddAsync(promotion, cancellationToken);
        return PromotionDto.From(created);
    }
}
