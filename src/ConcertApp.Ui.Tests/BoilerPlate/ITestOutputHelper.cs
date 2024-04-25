using System.Diagnostics;
using NUnit.Framework;

namespace ConcertApp.Ui.Tests.BoilerPlate;

public interface ITestOutputHelper
{
    void WriteLine(string message)
    {
        TestContext.Out.WriteLine (message);    
        Debug.WriteLine(message);
    }
}