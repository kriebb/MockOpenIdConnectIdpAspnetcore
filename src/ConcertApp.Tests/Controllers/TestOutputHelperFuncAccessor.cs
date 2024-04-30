using MartinCostello.Logging.XUnit;
using Xunit.Abstractions;

namespace ConcertApp.Tests.Controllers;

public class TestOutputHelperFuncAccessor(Func<ITestOutputHelper?> testOutputHelperAccessor) : ITestOutputHelperAccessor
{
    public ITestOutputHelper? OutputHelper
    {
        get => testOutputHelperAccessor.Invoke();
        set { testOutputHelperAccessor = () => value; }
    } 
}