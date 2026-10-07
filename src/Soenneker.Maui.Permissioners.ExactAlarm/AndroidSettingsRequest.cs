#if ANDROID
using System;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Asyncs.Semaphores;
using Android.Content;
using Microsoft.Maui.ApplicationModel;

namespace Soenneker.Maui.Permissioners.ExactAlarm;

internal static class AndroidSettingsRequest
{
    private static readonly AsyncSemaphore _gate = new(1);

    internal static async Task<bool> Run(Func<Task<bool>> request, CancellationToken cancellationToken)
    {
        SemaphoreLease lease = await _gate.Acquire(cancellationToken).ConfigureAwait(false);
        try
        {
            return await request().ConfigureAwait(false);
        }
        finally
        {
            lease.Dispose();
        }
    }

    internal static async Task<bool> Open(Intent intent, CancellationToken cancellationToken)
    {
        var activity = Platform.CurrentActivity
            ?? throw new InvalidOperationException("A foreground Android activity is required to request permission.");
        int taskId = activity.TaskId;
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        bool paused = false;
        void OnStateChanged(object? sender, ActivityStateChangedEventArgs args)
        {
            if (args.Activity.TaskId != taskId)
                return;
            if (args.State == ActivityState.Paused)
                paused = true;
            else if (paused && args.State == ActivityState.Resumed)
                completion.TrySetResult(true);
        }

        Platform.ActivityStateChanged += OnStateChanged;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                activity.StartActivity(intent);
            }
            catch (ActivityNotFoundException)
            {
                return false;
            }
            return await completion.Task.WaitAsync(TimeSpan.FromMinutes(5), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            Platform.ActivityStateChanged -= OnStateChanged;
        }
    }
}
#endif
