using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Testimonials.Commands;

public sealed record ApproveTestimonialCommand(Guid Id) : IRequest;

public sealed class ApproveTestimonialCommandValidator : AbstractValidator<ApproveTestimonialCommand>
{
    public ApproveTestimonialCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class ApproveTestimonialCommandHandler(ITestimonialRepository repository) : IRequestHandler<ApproveTestimonialCommand>
{
    public async Task Handle(ApproveTestimonialCommand request, CancellationToken cancellationToken)
    {
        var testimonial = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Testimonials.Testimonial), request.Id);

        testimonial.Approve();
        await repository.UpdateAsync(testimonial, cancellationToken);
    }
}
