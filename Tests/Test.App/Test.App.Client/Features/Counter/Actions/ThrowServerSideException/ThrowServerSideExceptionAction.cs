namespace Test.App.Client.Features.Counter;

public partial class CounterState
{
  public class ThrowServerSideExceptionAction : IAction
  {
    public string Message { get; set; }
  }
}
