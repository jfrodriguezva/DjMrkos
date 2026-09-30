using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Events.Dtos;
using DjMrkos.Application.Leads.Dtos;
using DjMrkos.Domain.Events;
using DjMrkos.Domain.Leads;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Leads.Commands;

/// <summary>
/// The bridge between a lead (a request submitted through Cotizar/Agendar/Contratar/Contacto)
/// and the DJ's real calendar: confirming a lead creates the <see cref="Event"/> that backs it
/// — same entity the QR song-request flow uses — and marks the lead <see cref="Domain.Leads.LeadStatus.Won"/>
/// so it stops showing up as pending in the admin's Cotizaciones tab.
/// </summary>
public sealed record ConfirmLeadCommand(Guid LeadId, DateTimeOffset EventDateUtc, string? Location) : IRequest<ConfirmLeadResult>;

public sealed record ConfirmLeadResult(LeadDto Lead, EventDto Event);

public sealed class ConfirmLeadCommandValidator : AbstractValidator<ConfirmLeadCommand>
{
    public ConfirmLeadCommandValidator()
    {
        RuleFor(x => x.LeadId).NotEmpty();
        RuleFor(x => x.EventDateUtc).NotEmpty();
    }
}

public sealed class ConfirmLeadCommandHandler(ILeadRepository leads, IEventRepository events, IQrTokenService qrTokenService)
    : IRequestHandler<ConfirmLeadCommand, ConfirmLeadResult>
{
    public async Task<ConfirmLeadResult> Handle(ConfirmLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await leads.GetByIdAsync(request.LeadId, cancellationToken)
            ?? throw new NotFoundException(nameof(Lead), request.LeadId);

        var scheduled = Event.Schedule(lead.Name, request.Location, request.EventDateUtc);
        scheduled.IssueQrToken(qrTokenService.IssueToken(scheduled.Id));
        var createdEvent = await events.AddAsync(scheduled, cancellationToken);

        lead.MarkWon();
        await leads.UpdateAsync(lead, cancellationToken);

        return new ConfirmLeadResult(LeadDto.From(lead), EventDto.From(createdEvent));
    }
}
