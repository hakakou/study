using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.IO;

public class S106_PluginFromOpenApi : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        builder.Services.AddSingleton<IMechanicService>(new FakeMechanicService());
        var kernel = builder.Build();

        // Requires package Microsoft.SemanticKernel.Plugins.OpenApi (preview)

        // Import directly:
        //var plugin = await kernel.ImportPluginFromOpenApiAsync("RepairService", "media/repair-service.json");

        // Create and transform
        var plugin = await kernel.CreatePluginFromOpenApiAsync("RepairService", "media/repair-service.json");
        kernel.Plugins.Add(TransformPlugin(plugin));

        PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

        //Console.WriteLine(await kernel.InvokePromptAsync("List all of the repairs assignedTo to Tezan.", new(settings)));
        Console.WriteLine(await kernel.InvokePromptAsync("Book an appointment to replace engine. Tell me id and who was it booked to.", new(settings)));
    }

    public static KernelPlugin TransformPlugin(KernelPlugin plugin)
    {
        List<KernelFunction>? functions = [];

        foreach (KernelFunction function in plugin)
        {
            if (function.Name == "createRepair")
            {
                functions.Add(CreateRepairFunction(function));
            }
            else
            {
                functions.Add(function);
            }
        }

        return KernelPluginFactory.CreateFromFunctions(plugin.Name, plugin.Description, functions);
    }

    private static KernelFunction CreateRepairFunction(KernelFunction function)
    {
        var method = async (
            Kernel kernel,
            KernelFunction currentFunction,
            KernelArguments arguments,
            [FromKernelServices] IMechanicService mechanicService,
            CancellationToken cancellationToken) =>
        {
            arguments.Add("assignedTo", mechanicService.GetMechanic());
            arguments.Add("date", DateTime.UtcNow.ToString("R"));

            var r = await function.InvokeAsync(kernel, arguments, cancellationToken);
            return r;
        };

        var options = new KernelFunctionFromMethodOptions()
        {
            FunctionName = function.Name,
            Description = function.Description,
            Parameters = function.Metadata.Parameters.Where(p => p.Name == "title" || p.Name == "description").ToList(),
            ReturnParameter = function.Metadata.ReturnParameter,
        };

        return KernelFunctionFactory.CreateFromMethod(method, options);
    }

    public interface IMechanicService
    {
        /// <summary>
        /// Return the name of the mechanic to assign the next job to.
        /// </summary>
        string GetMechanic();
    }

    /// <summary>
    /// Fake implementation of <see cref="IMechanicService"/>
    /// </summary>
    public class FakeMechanicService : IMechanicService
    {
        /// <inheritdoc/>
        public string GetMechanic() => "Bob";
    }

}