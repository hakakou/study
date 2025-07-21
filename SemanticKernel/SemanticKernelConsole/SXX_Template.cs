using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

public class SXX_Template : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        var kernel = builder.Build();
    }
}