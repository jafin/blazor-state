namespace Test.App.Client.Features.EventStream;

using System.Collections.Generic;

internal partial class EventStreamState : State<EventStreamState>
{

  /// <summary>
  /// Use in Tests ONLY, to initialize the State
  /// </summary>
  /// <param name="aEvents"></param>
  public void Initialize(List<string> aEvents)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    _Events = aEvents;
  }
}
