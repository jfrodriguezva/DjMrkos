using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Testimonials.Dtos;
using DjMrkos.Domain.Testimonials;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Testimonials.Commands;

public sealed record CreateTestimonialCommand(string ClientName, Guid? EventId, int Rating, string Comment) : IRequest<TestimonialDto>;

public sealed class CreateTestimonialCommandValidator : AbstractValidator<CreateTestimonialCommand>
{
    public CreateTestimonialCommandValidator()
    {
        RuleFor(x => x.ClientName).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(600);
    }
}

/// <summary>Lands as unapproved — an admin has to publish it before it appears on the public site.</summary>
public sealed class CreateTestimonialCommandHandler(ITestimonialRepository repository) : IRequestHandler<CreateTestimonialCommand, TestimonialDto>
{
    public async Task<TestimonialDto> Handle(CreateTestimonialCommand request, CancellationToken cancellationToken)
    {
        var testimonial = Testimonial.Create(request.ClientName, request.EventId, request.Rating, request.Comment);
        var created = await repository.AddAsync(testimonial, cancellationToken);
        return TestimonialDto.From(created);
    }
}
