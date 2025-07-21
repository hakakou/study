using Azure.AI.OpenAI;
using OpenAI.Chat;
using Spectre.Console;
using System.Text.Json;
using System.Text.Json.Serialization;

class T06_HomemadeFunctionCalling
{
    public static async Task Run()
    {
        var chatClient = Program.ChatClient;

        var list = new List<ChatMessage>() {

            new SystemChatMessage("""
You are a helpful assistant. Your task is to converse in a friendly manner with the user.
If the user's request requires it, you can use external tools to answer their questions. Ask the
user the necessary questions to collect the parameters needed to use the tools. The tools you
can use are ONLY the following:

>>Weather forecast access: Use this tool when the user asks for weather information, providing
the city and the time frame of interest. To use this tool, you must provide at least one of the
following parameters: ['city', 'startDate', 'endDate']

>>Email access: Use this tool when the user asks for information about their emails, possibly
specifying a time frame. To use this tool, you can specify one of these parameters, but not
necessarily both: ['startTime', 'endTime']

>>Stock market quotation access: Use this tool when the user asks for information about the
American stock market, specifying the stock name, index, and time frame. To use this tool,
you must provide at least three of the following parameters: ['stock_name', 'index_name',
'startDate', 'endDate']

RESPONSE FORMAT INSTRUCTIONS ----------------------------

**Option 1:**
Use this if you want to use a tool.
Markdown code snippet formatted in the following schema:

{
"tool": string \ The tool to use. Must be one of: Weather, Email, StockMarket
"tool_input": string \ The input to the action, formatted as json
}


**Option #2:**
Use this if you want to respond directly to the user.
Markdown code snippet formatted in the following schema:

{
"tool": "Answer",
"tool_input": string \ You should put what you want to return to user here
}

Remember to always respond with one of the two Options and NOTHING else.
"""),

            new UserChatMessage("Introduce yourself")
        };

        var opts = new ChatCompletionOptions()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat() 
        };
        ChatCompletion completion = await chatClient.CompleteChatAsync(list, opts);

        while (true)
        {
            var userMessage = AnsiConsole.Ask<string>("[yellow]What's your question for the assistant?[/]");
            list.Add(new UserChatMessage(userMessage));
            AnsiConsole.MarkupLine("[green]BOT:[/]");

            var chatCompletionsResponse = (await chatClient.CompleteChatAsync(list,opts)).Value;
            var llmResponse = chatCompletionsResponse.Content[0].Text;
            var deserializedResponse = Json<ChatCompletionResponse>(llmResponse);

            // Keep going until you get a final answer from the LLM, even if this requires multiple calls
            while (deserializedResponse.tool != "Answer")
            {
                var tempResponse = "";
                switch ((string)deserializedResponse.tool)
                {
                    case "Weather":
                        var functionResponse = GetWeather(deserializedResponse.tool_input.ToString());
                        var getAnswerMessage = $@"
GIVEN THE FOLLOWING TOOL RESPONSE:
---------------------
{functionResponse}
--------------------
What is the response to my last comment?
Remember to respond with a markdown code snippet of
a json blob with a single action,
and NOTHING else.";
                        list.Add(new UserChatMessage(getAnswerMessage));
                        tempResponse = (await chatClient.CompleteChatAsync(list,opts)).Value.Content[0].Text;
                        deserializedResponse = Json<ChatCompletionResponse>(tempResponse);
                        break;
                    case "Email":
                        // Implement Email function call here
                        break;
                    case "StockMarket":
                        // Implement StockMarket function call here
                        break;
                }
            }

            var responseForUser = deserializedResponse.tool_input.ToString();
            // Here we have the final response for the user
            AnsiConsole.MarkupLine($"[green]{responseForUser}[/]");
            list.Add(new AssistantChatMessage(responseForUser));
        }
    }

    private static string GetWeather(string toolInput)
    {
        // Simulate a weather function response
        return $"Weather data for {toolInput}";
    }

    private static T Json<T>(string input)
    {
        int startIndex = input.IndexOf('{');
        int endIndex = input.LastIndexOf('}');
        if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
        {
            return JsonSerializer.Deserialize<T>(input.Substring(startIndex, endIndex - startIndex + 1));
        }
        else
        {
            throw new Exception("No JSON object found in the input string: "+input);
        }
    }

    public class ChatCompletionResponse
    {
        public string tool { get; set; }
        public string tool_input { get; set; }

    }
}
