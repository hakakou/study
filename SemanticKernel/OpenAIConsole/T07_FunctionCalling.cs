using OpenAI.Chat;
using System.Text.Json;

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

public class T07_FunctionCalling
{
    public static async Task Run()
    {
        var chatClient = Program.ChatClient;

        var getWeatherFunction = ChatTool.CreateFunctionTool(
        functionName: "GetWeather",
        functionDescription: "Use this tool when the user asks for weather information or forecasts, providing the city and time frame of interest.",
        functionParameters: BinaryData.FromObjectAsJson(new
        {
            type = "object",
            properties = new
            {
                WeatherInfoRequest = new
                {
                    type = "object",
                    properties = new
                    {
                        city = new
                        {
                            type = "string",
                            description = "The city the user wants to check the weather for."
                        },
                        startDate = new
                        {
                            type = "string",
                            format = "date",
                            description = "The start date the user is interested in for the weather forecast."
                        },
                        endDate = new
                        {
                            type = "string",
                            format = "date",
                            description = "The end date the user is interested in for the weather forecast."
                        }
                    },
                    required = new[] { "city" }
                }
            },
            required = new[] { "WeatherInfoRequest" }
        }));

        var chatOptions = new ChatCompletionOptions()
        {
            Temperature = 0,
            MaxOutputTokenCount = 1000,
            ToolChoice = ChatToolChoice.CreateAutoChoice()
        };
        chatOptions.Tools.Add(getWeatherFunction);

        var chatMessages = new List<ChatMessage>();

        chatMessages.Add(new SystemChatMessage(
            "You are a helpful assistant. Your task is to converse in a friendly manner with the user."));

        chatMessages.Add(new UserChatMessage("Introduce yourself"));

        // Make first completion call
        ChatCompletion initialCompletion = await chatClient.CompleteChatAsync(chatMessages, chatOptions);
        var initialResponse = initialCompletion;

        if (initialResponse is not null)
        {
            // If model responds directly
            if (initialResponse.FinishReason != ChatFinishReason.ToolCalls)
            {
                Console.WriteLine("ASSISTANT: " + initialResponse.Content);
                chatMessages.Add(new UserChatMessage(initialResponse.Content));
            }
        }

        // Enter a loop to interact with the user
        while (true)
        {
            Console.Write("USER: ");
            var userMessage = Console.ReadLine();
            if (string.IsNullOrEmpty(userMessage)) break;
            chatMessages.Add(new UserChatMessage(userMessage));

            // Call the model
            ChatCompletion userCompletion = await chatClient.CompleteChatAsync(chatMessages, chatOptions);
            var userResponse = userCompletion;

            // If the model wants to call a function, handle it
            if (userResponse != null && userResponse.FinishReason == ChatFinishReason.ToolCalls)
            {
                bool functionCallingComplete = false;
                while (!functionCallingComplete)
                {
                    // Add assistant message that initiated the tool call
                    //chatMessages.Add(new AssistantChatMessage(userResponse.Content));

                    // The model may request one or more tool calls
                    foreach (ChatToolCall toolCall in userResponse.ToolCalls)
                    {
                        var functionArguments = toolCall.FunctionArguments.ToString();
                        var weatherInfoRequest = JsonSerializer
                            .Deserialize<WeatherInfoRequestObject>(functionArguments);

                        // Call the actual function
                        var functionResult = GetWeather(weatherInfoRequest.WeatherInfoRequest);

                        // Add the function's response as a tool message
                        chatMessages.Add(new FunctionChatMessage("GetWeather", functionResult));
                    }

                    // Another call to see if the model is now ready to respond to the user
                    ChatCompletion functionCompletion = await chatClient.CompleteChatAsync(chatMessages, chatOptions);
                    var functionResponse = functionCompletion;
                    if (functionResponse != null)
                    {
                        // If after the function call the model does not request more function calls, it should now produce a final answer
                        //chatMessages.Add(new AssistantChatMessage("GetWeather", functionResponse.Content[0]));

                        if (functionResponse.FinishReason != ChatFinishReason.ToolCalls)
                        {
                            // Final answer to user
                            Console.WriteLine("ASSISTANT: " + functionResponse.Content[0].Text);
                            functionCallingComplete = true;
                        }
                        else
                        {
                            // If still wants to call more functions, loop continues
                            userResponse = functionResponse;
                        }
                    }
                }
            }
            else
            {
                // Direct answer to the user
                if (userResponse?.Content != null)
                {
                    Console.WriteLine("ASSISTANT: " + userResponse.Content[0].Text);
                    chatMessages.Add(new AssistantChatMessage("GetWeather", userResponse.Content[0]));
                }
            }
        }
    }

    // {"WeatherInfoRequest":{"city":"Patras","startDate":"2023-10-10"}}
    public class WeatherInfoRequestObject
    {
        public WeatherInfoRequest WeatherInfoRequest { get; set; }
    }

    public class WeatherInfoRequest
    {
        public string city { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
    }

    private static string GetWeather(WeatherInfoRequest request)
    {
        // Here you'd implement the actual weather retrieval logic.
        // For now, just simulate a response.
        var city = request.city;
        var start = request.startDate.HasValue ? request.startDate.Value.ToShortDateString() : "N/A";
        var end = request.endDate.HasValue ? request.endDate.Value.ToShortDateString() : "N/A";
        return $"Simulated weather for {city}, from {start} to {end}: Sunny with mild temperatures.";
    }
}