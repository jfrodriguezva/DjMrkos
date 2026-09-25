using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Modules.Commands;

/// <summary>
/// Soft-deletes: the module and its categories stop showing on the public menu, but the
/// rows stay (an event's historical song requests may still reference a category's module).
/// A hard delete is intentionally not exposed here — see docs/DATABASE.md.
/// </summary>
public sealed record DeleteModuleCommand(Guid Id) : IRequest;

public sealed class DeleteModuleCommandValidator : AbstractValidator<DeleteModuleCommand>
{
    public DeleteModuleCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class DeleteModuleCommandHandler(IModuleRepository repository) : IRequestHandler<DeleteModuleCommand>
{
    public async Task Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Modules.Module), request.Id);

        module.Deactivate();
        await repository.UpdateAsync(module, cancellationToken);
    }
}
