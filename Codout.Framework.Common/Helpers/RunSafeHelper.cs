using System;
using System.Threading;
using System.Threading.Tasks;

namespace Codout.Framework.Common.Helpers;

public static class RunSafeHelper
{
    public static async Task RunSafe(this Task task, Action<Exception> onError = null,
        CancellationToken token = default)
    {
        Exception exception = null;
        try
        {
            if (!token.IsCancellationRequested)
                await Task.Run(() =>
                {
                    task.Start();
                    task.Wait(token);
                }, token);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Task Cancelled");
        }
        catch (AggregateException e)
        {
            var ex = e.InnerException;
            while (ex is { InnerException: not null })
                ex = ex.InnerException;
            exception = ex;
        }
        catch (Exception e)
        {
            exception = e;
        }

        if (exception != null)
        {
            //TODO: Log to Insights
            Console.WriteLine(exception);
            onError?.Invoke(exception);
        }
    }
}