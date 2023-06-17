using Calcu;
using Discord;
using Discord.WebSocket;
using MathsParser;
using Environment = MathsParser.Environment;
using TokenType = Discord.TokenType;

public class Program
{
    private readonly List<Calculation> _calculations = new();

    private Number _ans = new(0);
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
            { "quadratic", CustomMath.Quadratic },
            { "factorial", CustomMath.Factorial },
            { "P", CustomMath.Permutation },
            { "C", CustomMath.Combination },
        };

        var variables = new Dictionary<string, double>
        {
            { "pi", Math.PI },
            { "π", Math.PI },
            { "e", Math.E },
            { "x", 2 },
            // ans is updated after every calculation
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

        _client.Ready += async () =>
        {
            Log("Bot is connected!");
            await _client.SetStatusAsync(UserStatus.Online);
            await _client.SetActivityAsync(new Game("with numbers, @ me!"));
        };

        _client.MessageReceived += OnMessage;

        _client.ReactionAdded += OnReaction;

        await Task.Delay(-1);
    }

    private async Task OnReaction(Cacheable<IUserMessage, ulong> messageEntity,
        Cacheable<IMessageChannel, ulong> channelEntity, SocketReaction reaction)
    {
        var message = await messageEntity.GetOrDownloadAsync();

        if (reaction.User.Value.IsBot) return;
        if (message.Author.Id != _client.CurrentUser.Id) return;

        // Find the calculation that this reaction is for
        var calculations = _calculations.Where(c => c.ResponseMessage?.Id == message.Id);
        if (!calculations.Any()) return;

        var calculation = calculations.First();

        if (reaction.Emote.Equals(new Emoji("🔁")))
        {
            // switch display mode
            calculation.DisplayMode = calculation.DisplayMode == CalculationDisplayMode.Fraction
                ? CalculationDisplayMode.Decimal
                : CalculationDisplayMode.Fraction;

            // update message
            await calculation.ResponseMessage.ModifyAsync(m => { m.Embed = calculation.ToEmbed(); });
        }

        if (reaction.Emote.Equals(new Emoji("📈")))
        {
            // TODO: graph
        }
    }


    private async Task OnMessage(SocketMessage arg)
    {
        // Ignore system and non-user messages
        if (arg is not SocketUserMessage { Source: MessageSource.User } message) return;

        // Only respond to messages that mention the bot
        if (message.MentionedUsers.All(u => u.Id != _client.CurrentUser.Id)) return;

        // Remove the mention from the message
        var content = message.Content.Replace($"<@{_client.CurrentUser.Id}>", "").Trim();

        IUserMessage replyMessage = message;
        IUserMessage reactMessage = message;

        // If message is empty, try to get the previous message from the channel
        if (string.IsNullOrWhiteSpace(content))
        {
            var possiblePreviousMessage = (await message.Channel.GetMessagesAsync(2).FlattenAsync()).Last();
            if (possiblePreviousMessage is not IUserMessage { Source: MessageSource.User } previousMessage) return;

            content = previousMessage.Content.Trim();

            // If the previous message mentions the bot, ignore it
            if (content.Contains($"<@{_client.CurrentUser.Id}>")) return;

            // If the previous message is also empty, ignore it
            if (string.IsNullOrWhiteSpace(content)) return;

            replyMessage = previousMessage;
        }

        var calculation = await TryPerformCalculation(reactMessage, replyMessage, content);
        if (calculation.Success) _calculations.Add(calculation);
    }

    private async Task<Calculation> TryPerformCalculation(IUserMessage reactMessage, IUserMessage replyMessage,
        string content)
    {
        try
        {
            _environment.Variables["ans"] = _ans.Value;

            var node = _parser.Read(content);
            var result = node.Evaluate(_environment);
            var number = new Number(result);

            var calculation = new Calculation(replyMessage, content, number, node);

            var reply = await replyMessage.ReplyAsync(embed: calculation.ToEmbed(),
                allowedMentions: AllowedMentions.None);
            await reactMessage.AddReactionAsync(new Emoji("✅"));

            calculation.ResponseMessage = reply;

            await AddReactionControls(reply);

            _ans = number;

            return calculation;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            await reactMessage.AddReactionAsync(new Emoji("❌"));
            return new Calculation(replyMessage, content);
        }
    }

    private static async Task AddReactionControls(IUserMessage message)
    {
        await message.AddReactionAsync(new Emoji("🔁")); // Switch between fraction and decimal
        // await message.AddReactionAsync(new Emoji("📈")); // Graph the function
    }
}