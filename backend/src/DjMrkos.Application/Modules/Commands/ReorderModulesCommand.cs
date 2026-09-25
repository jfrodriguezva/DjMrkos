using DjMrkos.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Modules.Commands;

/// <summary>Applies a full new ordering after the admin drags a module up or down in the menu editor.</summary>
public sealed record ReorderModulesCommand(IReadOnlyList<Guid> OrderedModuleIds) : IRequest;

public sealed class ReorderModulesCommandValidator : AbstractValidator<ReorderModulesCommand>
{
    public ReorderModulesCommandValidator() => RuleFor(x => x.OrderedModuleIds).NotEmpty();
}

public sealed class ReorderModulesCommandHandler(IModuleRepository repository) : IRequestHandler<ReorderModulesCommand>
{
    public async Task Handle(ReorderModulesCommand request, CancellationToken cancellationToken)
    {
        var all = (await repository.GetAllAsync(onlyActive: false, cancellationToken)).ToDictionary(m => m.Id);

        for (var position = 0; position < request.OrderedModuleIds.Count; position++)
        {
            if (!all.TryGetValue(request.OrderedModuleIds[position], out var module))
                continue;

            module.MoveTo(position);
            await repository.UpdateAsync(module, cancellationToken);
        }
    }
}
