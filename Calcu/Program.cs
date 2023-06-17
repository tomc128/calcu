using Discord;
using Discord.WebSocket;
using MathsParser;
using Environment = MathsParser.Environment;
using TokenType = Discord.TokenType;

public class Program
{
    private DiscordSocketClient _client;
    private Environment _environment;
    private Parser _parser;

    public static Task Main(string[] args)
    {
        return new Program().MainAsync();
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private void Log(string msg)
    {
        Console.WriteLine(msg);
    }


    private async Task MainAsync()
    {
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
        };

        Log("Setting up MathsParser...");

        var functions = new Dictionary<string, Delegate>
        {
            { "sin", Math.Sin },
            { "cos", Math.Cos },
            { "tan", Math.Tan },
            { "asin", Math.Asin },
            { "acos", Math.Acos },
            { "atan", Math.Atan },
            { "abs", new Func<double, double>(Math.Abs) },
            { "sqrt", Math.Sqrt },
            { "clamp", new Func<double, double, double, double>(Math.Clamp) },
            { "log", new Func<double, double, double>(Math.Log) },
            { "ln", new Func<double, double>(x => Math.Log(x, Math.E)) },
        };

        var variables = new Dictionary<string, double>
        {
            { "pi", Math.PI },
            { "π", Math.PI },
            { "e", Math.E },
            { "x", 2 },
        };

        _environment = new Environment(functions, variables);
        _parser = new Parser();

        Log("Setting up Discord client...");

        _client = new DiscordSocketClient(config);
        _client.Log += Log;

        Log("Connecting to Discord...");

        var token = await File.ReadAllTextAsync("token.txt");

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        _client.Ready += () =>
        {
            Log("Bot is connected!");
            return Task.CompletedTask;
        };

        _client.MessageReceived += OnMessage;

        await Task.Delay(-1);
    }

    private async Task OnMessage(SocketMessage arg)
    {
        // Ignore system and non-user messages
        if (arg is not SocketUserMessage message) return;
        if (message.Source != MessageSource.User) return;

        // Only respond to messages that mention the bot
        if (message.MentionedUsers.All(u => u.Id != _client.CurrentUser.Id)) return;

        // Remove the mention from the message
        var content = message.Content.Replace($"<@{_client.CurrentUser.Id}>", "").Trim();

        // Ignore empty messages
        if (string.IsNullOrWhiteSpace(content)) return;

        try
        {
            var result = _parser.Read(content).Evaluate(_environment);
            var number = new Number(result);

            await message.ReplyAsync($"{number}");
            await message.AddReactionAsync(new Emoji("✅"));
        }
        catch
        {
            await message.AddReactionAsync(new Emoji("❌"));
        }
    }
}