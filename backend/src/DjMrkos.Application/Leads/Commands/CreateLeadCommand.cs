using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Leads.Dtos;
using DjMrkos.Domain.Leads;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Leads.Commands;

public sealed record CreateLeadCommand(string Name, string Email, string? Phone, DateOnly? EventDate, string Message) : IRequest<LeadDto>;

public sealed class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(1000);
    }
}

public sealed class CreateLeadCommandHandler(ILeadRepository repository) : IRequestHandler<CreateLeadCommand, LeadDto>
{
    public async Task<LeadDto> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = Lead.Create(request.Name, request.Email, request.Phone, request.EventDate, request.Message);
        var created = await repository.AddAsync(lead, cancellationToken);
        return LeadDto.From(created);
    }
}
