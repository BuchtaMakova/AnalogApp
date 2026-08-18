using AnalogHub.Application;
using AnalogHub.Application.Common.Interfaces;
using AnalogHub.Application.Photos.Commands;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AnalogHub.Tests.Common;

/// <summary>
/// Exercises the real DI-wired MediatR pipeline (not a handler called directly) so a regression in
/// how <see cref="AnalogHub.Application.Common.Behaviors.ValidationBehavior{TRequest,TResponse}"/>
/// is registered or constrained gets caught here rather than surfacing as a raw 500 in production.
/// A previous version constrained it to <c>TRequest : IRequest&lt;TResponse&gt;</c>, which plain
/// void <see cref="IRequest"/> commands (like <see cref="UpdatePhotoRatingCommand"/>) don't satisfy
/// in MediatR 12 — so the DI container silently produced zero pipeline behaviors for every
/// void command, and invalid input skipped validation entirely.
/// </summary>
public sealed class ValidationPipelineTests
{
    private static ISender BuildSender()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddSingleton<IApplicationDbContext>(TestDbContextFactory.Create());
        services.AddSingleton<ICurrentUserService>(new FakeCurrentUserService());

        return services.BuildServiceProvider().GetRequiredService<ISender>();
    }

    [Fact]
    public async Task Send_VoidCommandWithInvalidInput_ThrowsValidationExceptionRatherThanReachingTheHandler()
    {
        var sender = BuildSender();

        var act = () => sender.Send(new UpdatePhotoRatingCommand(Guid.NewGuid(), 9));

        // If this instead throws NotFoundException (or anything DB-related), validation was skipped
        // and the request reached the handler.
        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task Send_VoidCommandWithInvalidRotation_ThrowsValidationException()
    {
        var sender = BuildSender();

        var act = () => sender.Send(new UpdatePhotoRotationCommand(Guid.NewGuid(), 45));

        await Assert.ThrowsAsync<ValidationException>(act);
    }
}
