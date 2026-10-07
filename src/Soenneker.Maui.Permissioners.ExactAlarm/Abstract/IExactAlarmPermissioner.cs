using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Maui.Permissioners.ExactAlarm.Abstract;

/// <summary>Checks Android exact alarm access. Other platforms are unsupported. Apps requesting access must declare android.permission.SCHEDULE_EXACT_ALARM.</summary>
public interface IExactAlarmPermissioner
{
    /// <summary>Whether this build supports the platform's permission API.</summary>
    bool IsSupported { get; }

    /// <summary>Checks current authorization without displaying UI. Returns false on unsupported platforms.</summary>
    ValueTask<bool> Has(CancellationToken cancellationToken = default);

    /// <summary>Requests authorization when missing and returns the resulting status. Returns false when unsupported or no settings activity is available.</summary>
    /// <remarks>Call from a foreground app. Cancellation stops waiting, not the system UI. Waits for the app to return from settings, up to five minutes; timeout throws TimeoutException. No app lifecycle forwarding is required.</remarks>
    ValueTask<bool> Request(CancellationToken cancellationToken = default);

    /// <summary>Checks authorization and requests it only when missing.</summary>
    ValueTask<bool> RequestIfNotGranted(CancellationToken cancellationToken = default);
}

