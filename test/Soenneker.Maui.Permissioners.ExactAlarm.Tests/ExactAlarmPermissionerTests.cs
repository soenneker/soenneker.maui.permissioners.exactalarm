using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Soenneker.Maui.Permissioners.ExactAlarm.Abstract;
using Soenneker.Maui.Permissioners.ExactAlarm.Registrars;

namespace Soenneker.Maui.Permissioners.ExactAlarm.Tests;

public sealed class ExactAlarmPermissionerTests
{
    [Test]
    public async Task Unsupported_platform_never_reports_granted(CancellationToken cancellationToken)
    {
        var permissioner = new ExactAlarmPermissioner();
        await Assert.That(permissioner.IsSupported).IsFalse();
        await Assert.That(await permissioner.Has(cancellationToken: cancellationToken)).IsFalse();
        await Assert.That(await permissioner.Request(cancellationToken: cancellationToken)).IsFalse();
        await Assert.That(await permissioner.RequestIfNotGranted(cancellationToken: cancellationToken)).IsFalse();
        
    }

    [Test]
    public async Task Canceled_requests_are_observed_even_when_unsupported(CancellationToken cancellationToken)
    {
        var permissioner = new ExactAlarmPermissioner();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.That(async () => await permissioner.Has(cancellation.Token)).Throws<OperationCanceledException>();
        await Assert.That(async () => await permissioner.Request(cancellation.Token)).Throws<OperationCanceledException>();
        await Assert.That(async () => await permissioner.RequestIfNotGranted(cancellation.Token)).Throws<OperationCanceledException>();
        
    }

    [Test]
    public async Task Registration_preserves_existing_lifetime(CancellationToken cancellationToken)
    {
        var services = new ServiceCollection();
        services.AddExactAlarmPermissionerAsScoped();
        services.AddExactAlarmPermissionerAsSingleton();
        await Assert.That(services.Count).IsEqualTo(1);
        await Assert.That(services[0].ServiceType).IsEqualTo(typeof(IExactAlarmPermissioner));
        await Assert.That(services[0].ImplementationType).IsEqualTo(typeof(ExactAlarmPermissioner));
        await Assert.That(services[0].Lifetime).IsEqualTo(ServiceLifetime.Scoped);
    }
}
