using System;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Maui.Permissioners.ExactAlarm.Abstract;
#if ANDROID
using Android.Content;
using Android.Provider;
using Microsoft.Maui.ApplicationModel;
#endif

namespace Soenneker.Maui.Permissioners.ExactAlarm;

public sealed class ExactAlarmPermissioner : IExactAlarmPermissioner
{
    public bool IsSupported =>
#if ANDROID
        true;
#else
        false;
#endif

    public ValueTask<bool> Has(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
#if ANDROID
        if (!OperatingSystem.IsAndroidVersionAtLeast(31))
            return ValueTask.FromResult(true);
        var context = Android.App.Application.Context;
        using var manager = (Android.App.AlarmManager?)context.GetSystemService(Context.AlarmService);
        return ValueTask.FromResult(manager?.CanScheduleExactAlarms() == true);
#else
        return ValueTask.FromResult(false);
#endif
    }

    public ValueTask<bool> Request(CancellationToken cancellationToken = default) => RequestCore(cancellationToken);

    public ValueTask<bool> RequestIfNotGranted(CancellationToken cancellationToken = default) => Request(cancellationToken);

    private async ValueTask<bool> RequestCore(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsSupported)
            return false;
        if (await Has(cancellationToken).ConfigureAwait(false))
            return true;
#if ANDROID
        return await AndroidSettingsRequest.Run(async () =>
        {
            if (await Has(cancellationToken).ConfigureAwait(false))
                return true;
            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!OperatingSystem.IsAndroidVersionAtLeast(31))
                    return true;
                using var intent = new Intent(Settings.ActionRequestScheduleExactAlarm);
                intent.SetData(Android.Net.Uri.Parse("package:" + Android.App.Application.Context.PackageName));
                if (!await AndroidSettingsRequest.Open(intent, cancellationToken).ConfigureAwait(false))
                    return false;
                return await Has(cancellationToken).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);
#else
        return false;
#endif
    }
}
