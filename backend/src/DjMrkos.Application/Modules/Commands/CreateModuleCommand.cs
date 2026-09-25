using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Modules.Dtos;
using DjMrkos.Domain.Modules;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Modules.Commands;

public sealed record CreateModuleCommand(string Name, string? Icon, int DisplayOrder) : IRequest<ModuleDto>;

public sealed class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
{
    public CreateModuleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Icon).MaximumLength(40);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateModuleCommandHandler(IModuleRepository repository) : IRequestHandler<CreateModuleCommand, ModuleDto>
{
    public async Task<ModuleDto> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
    {
        var module = Module.Create(request.Name, request.Icon, request.DisplayOrder);
        var created = await repository.AddAsync(module, cancellationToken);
        return ModuleDto.From(created);
    }
}
