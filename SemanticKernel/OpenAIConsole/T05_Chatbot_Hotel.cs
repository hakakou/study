using OpenAI.Chat;
using Spectre.Console;

internal class T05_Chatbot_Hotel
{
    public static async Task Run()
    {
        var chatClient = Program.ChatClient;

        var list = new List<ChatMessage>() {
            new SystemChatMessage(@"Suppose you want to build a booking chatbot for a hotel brand group. A reasonable system prompt
might look something like this:
You are a HotelBot, an automated service to collect hotel bookings within a hotel brand group,
in different cities.
You first greet the customer, then collect the booking, asking the name of the customer, the
city the customer wants to book, room type and additional services.
You wait to collect the entire booking, then summarize it and check for a final time if the
customer wants to add anything else.
You ask for arrival date, departure date, and calculate the number of nights. You ask for a
passport number. Make sure to clarify all options and extras to uniquely identify the item from
the pricing list.
You respond in a short, very conversational friendly style. Available cities: Rome, Lisbon,
Bucharest.
The hotel rooms are:
single 150.00 per night
double 250 per night
suite 350 per night
Extra services:
parking 20.00 per day,
late checkout 100.00
airport transfer 50.00
SPA 30.00 per day"),

            new UserChatMessage("Introduce yourself")
        };

        do
        {
            ChatCompletion completion = await chatClient.CompleteChatAsync(list);
            AnsiConsole.MarkupLine($"[green]Assistant:[/] {completion.Content[0].Text}");
            list.Add(new AssistantChatMessage(completion.Content[0].Text));

            string? userQuestion = AnsiConsole.Ask<string>("[yellow]What's your question for the assistant?[/]");
            if (userQuestion == "end") break;
            list.Add(new UserChatMessage(userQuestion));
        } while (true);

        list.Add(new UserChatMessage(@"Return a json summary of the previous booking. Itemize the price for each item.
The json fields should be
1) name,
2) passport,
3) city,
4) room type with total price,
5) list of extras including total price,
6) arrival date,
7) departure date,
8) total days
9) total price of rooms and extras (calculated as the sum of the total room price and extra
price).
Return only the json, without introduction or final sentences.
Simulating a conversation with the HotelBot, a json like the following would be generated from
the previous prompt:
{""name"":""Francesco Esposito"",""passport"":""XXCONTOSO123"",""city"":""Lisbon"",""room_type"":{""single"":15
0.00},""extras"":{""parking"":{""price_per_day"":20.00,""total_price"":40.00}},""arrival_date"":""2023-06-
28"",""departure_date"":""2023-06-30"",""total_days"":2,""total_price"":340.00}"""));
        ChatCompletion completion2 = await chatClient.CompleteChatAsync(list);
        AnsiConsole.MarkupLine($"[green]Assistant:[/] {completion2.Content[0].Text}");
    }
}