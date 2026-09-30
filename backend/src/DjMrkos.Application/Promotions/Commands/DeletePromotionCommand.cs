using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Promotions.Commands;

public sealed record DeletePromotionCommand(Guid Id) : IRequest;

public sealed class DeletePromotionCommandValidator : AbstractValidator<DeletePromotionCommand>
{
    public DeletePromotionCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class DeletePromotionCommandHandler(IPromotionRepository repository) : IRequestHandler<DeletePromotionCommand>
{
    public async Task Handle(DeletePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Promotions.Promotion), request.Id);

        promotion.Deactivate();
        await repository.UpdateAsync(promotion, cancellationToken);
    }
}
