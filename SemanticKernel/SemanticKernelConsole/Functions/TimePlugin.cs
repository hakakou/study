using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace SemanticKernelConsole.Functions;

public class TimePlugin
{
    /// <summary>
    /// Retrieves the current time in UTC.
    /// </summary>
    /// <returns>The current time in UTC. </returns>
    [KernelFunction, Description("Retrieves the current time in UTC.")]
    public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
}


