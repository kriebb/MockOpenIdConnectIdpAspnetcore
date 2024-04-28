using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace ConcertApp.Tests.Controllers;

public class TestOutputHelperFuncAccessor : ITestOutputHelperAccessor
{
    private Func<ITestOutputHelper?> _testoutputhelper;

    public TestOutputHelperFuncAccessor(Func<ITestOutputHelper?> testoutputhelper)
    {
        _testoutputhelper = testoutputhelper;
    }

    public ITestOutputHelper? OutputHelper
    {
        get { return _testoutputhelper.Invoke();}
        set { _testoutputhelper = () => value; }
    } 
}