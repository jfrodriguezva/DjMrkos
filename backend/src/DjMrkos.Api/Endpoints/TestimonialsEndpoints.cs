using DjMrkos.Api.Security;
using DjMrkos.Application.Testimonials.Commands;
using DjMrkos.Application.Testimonials.Queries;
using MediatR;

namespace DjMrkos.Api.Endpoints;

public static class TestimonialsEndpoints
{
    public static void MapTestimonialsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/testimonials", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetApprovedTestimonialsQuery(), ct)))
            .WithTags("Testimonials")
            .AllowAnonymous();

        app.MapPost("/api/testimonials", async (CreateTestimonialCommand command, ISender sender, CancellationToken ct) =>
        {
            var created = await sender.Send(command, ct);
            return Results.Created($"/api/testimonials/{created.Id}", created);
        })
        .WithTags("Testimonials")
        .AllowAnonymous();

        var admin = app.MapGroup("/api/admin/testimonials")
            .WithTags("Admin · Testimonials")
            .RequireAuthorization(ApiKeyAuthenticationHandler.SchemeName);

        admin.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetAllTestimonialsQuery(), ct)));

        admin.MapPost("/{id:guid}/approve", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ApproveTestimonialCommand(id), ct);
            return Results.NoContent();
        });
    }
}
