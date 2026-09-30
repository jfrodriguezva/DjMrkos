using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Promotions.Dtos;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Promotions.Commands;

public sealed record UpdatePromotionCommand(Guid Id, string Label, decimal DiscountPercentage, bool IsActive) : IRequest<PromotionDto>;

public sealed class UpdatePromotionCommandValidator : AbstractValidator<UpdatePromotionCommand>
{
    public UpdatePromotionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DiscountPercentage).GreaterThan(0).LessThanOrEqualTo(100);
    }
}

public sealed class UpdatePromotionCommandHandler(IPromotionRepository repository) : IRequestHandler<UpdatePromotionCommand, PromotionDto>
{
    public async Task<PromotionDto> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Promotions.Promotion), request.Id);

        promotion.Update(request.Label, request.DiscountPercentage);
        if (request.IsActive) promotion.Activate(); else promotion.Deactivate();

        await repository.UpdateAsync(promotion, cancellationToken);
        return PromotionDto.From(promotion);
    }
}
