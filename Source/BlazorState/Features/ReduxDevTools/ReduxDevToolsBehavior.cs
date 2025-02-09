#nullable enable
namespace TimeWarp.Features.ReduxDevTools;

/// <summary>
///
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class ReduxDevToolsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull, IAction
{
  private readonly ILogger Logger;
  private readonly ReduxDevToolsInterop ReduxDevToolsInterop;
  private readonly ReduxDevToolsOptions ReduxDevToolsOptions;
  private readonly IReduxDevToolsStore Store;
  private readonly Regex TraceFilterRegex;

  public ReduxDevToolsBehavior
  (
    ILogger<ReduxDevToolsBehavior<TRequest, TResponse>> logger,
    ReduxDevToolsInterop reduxDevToolsInterop,
    ReduxDevToolsOptions reduxDevToolsOptions,
    IReduxDevToolsStore store
  )
  {
    Logger = logger;
    Logger.LogDebug(EventIds.ReduxDevToolsBehavior_Constructing, "constructing ReduxDevToolsBehavior");
    Store = store;
    ReduxDevToolsInterop = reduxDevToolsInterop;
    ReduxDevToolsOptions = reduxDevToolsOptions;
    TraceFilterRegex = new Regex(reduxDevToolsOptions.TraceFilterExpression);
  }

  public async Task<TResponse> Handle
  (
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    Logger.LogDebug(EventIds.ReduxDevToolsBehavior_Begin,"{classname}: Start", GetType().Name);

    string? stackTrace = null;
    int maxItems = ReduxDevToolsOptions.TraceLimit == 0 ? int.MaxValue : ReduxDevToolsOptions.TraceLimit;
    
    if (ReduxDevToolsOptions.Trace) stackTrace = BuildStackTrace(maxItems);
    TResponse response = await next();

    try
    {
      await ReduxDevToolsInterop.DispatchAsync(request, Store.GetSerializableState(), stackTrace);
      Logger.LogDebug(EventIds.ReduxDevToolsBehavior_End, "ReduxDevToolsBehavior Completed");
    }
    catch (Exception exception)
    {
      Logger.LogDebug
      (
        EventIds.ReduxDevToolsBehavior_Exception,
        exception,
        "Error dispatching Request to Redux DevTools"
      );

      throw;
    }
 
    return response;
  }

  private string BuildStackTrace(int maxItems)
  {
    StringBuilder stringBuilder = new();
    return string.Join
      (
        "\r\n",
        new StackTrace(fNeedFileInfo: true)
          .GetFrames()
          .Select
          (
            stackFrame =>
            {
              stringBuilder.Clear();
              stringBuilder.Append("at ");
              stringBuilder.Append(stackFrame.GetMethod()?.DeclaringType?.FullName);
              stringBuilder.Append('.');
              stringBuilder.Append(stackFrame.GetMethod()?.Name);
              stringBuilder.Append(' ');
              if (stackFrame.GetFileName() is not null)
              {
                stringBuilder.Append('(');
                stringBuilder.Append(stackFrame.GetFileName());
                stringBuilder.Append(':');
                stringBuilder.Append(stackFrame.GetFileLineNumber());
                stringBuilder.Append(':');
                stringBuilder.Append(stackFrame.GetFileColumnNumber());
                stringBuilder.Append(')');
              }
              return stringBuilder.ToString();
            }
          )
          .Where(x => TraceFilterRegex?.IsMatch(x) != false)
          .Take(maxItems)
      );
  }
}
