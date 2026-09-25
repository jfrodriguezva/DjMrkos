using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Modules.Dtos;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Modules.Commands;

public sealed record UpdateModuleCommand(Guid Id, string Name, string? Icon, int DisplayOrder, bool IsActive) : IRequest<ModuleDto>;

public sealed class UpdateModuleCommandValidator : AbstractValidator<UpdateModuleCommand>
{
    public UpdateModuleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateModuleCommandHandler(IModuleRepository repository) : IRequestHandler<UpdateModuleCommand, ModuleDto>
{
    public async Task<ModuleDto> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Modules.Module), request.Id);

        module.Rename(request.Name);
        module.SetIcon(request.Icon);
        module.MoveTo(request.DisplayOrder);
        if (request.IsActive) module.Activate(); else module.Deactivate();

        await repository.UpdateAsync(module, cancellationToken);
        return ModuleDto.From(module);
    }
}
