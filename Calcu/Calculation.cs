using Discord;
using MathsParser;

namespace Calcu;

public struct Calculation
{
    public IUserMessage CallMessage { get; }
    public IUserMessage? ResponseMessage { get; set; }
    public string Expression { get; }
    public Number? Result { get; }
    public Node? Node { get; }
    public DateTime Timestamp { get; }
    public bool Success { get; }
    public CalculationDisplayMode DisplayMode { get; set; }

    public Calculation(IUserMessage callMessage, IUserMessage? responseMessage, string expression, Number? result,
        Node node)
    {
        CallMessage = callMessage;
        ResponseMessage = responseMessage;
        Expression = expression;
        Result = result;
        Node = node;
        Timestamp = callMessage.Timestamp.DateTime;
        Success = result is not null;
        DisplayMode = CalculationDisplayMode.Fraction;
    }

    public Calculation(IUserMessage callMessage, string expression, Number? result,
        Node node)
    {
        CallMessage = callMessage;
        ResponseMessage = null;
        Expression = expression;
        Result = result;
        Node = node;
        Timestamp = callMessage.Timestamp.DateTime;
        Success = result is not null;
        DisplayMode = CalculationDisplayMode.Fraction;
    }

    public Calculation(IUserMessage callMessage, string expression)
    {
        CallMessage = callMessage;
        ResponseMessage = null;
        Expression = expression;
        Result = null;
        Node = null;
        Timestamp = callMessage.Timestamp.DateTime;
        Success = false;
        DisplayMode = CalculationDisplayMode.Fraction;
    }

    public Embed ToEmbed()
    {
        // TODO: precision / rounding in footer?
        if (Result is null) throw new InvalidOperationException("Cannot convert a failed calculation to an embed.");

        return new EmbedBuilder
        {
            Title = DisplayMode == CalculationDisplayMode.Decimal ? Result.Value.AsDecimal() : Result.Value.ToString(),
            Color = Color.Green,
        }.Build();
    }
}