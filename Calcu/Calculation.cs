using Discord;
using MathsParser;

namespace Calcu;

public struct Calculation
{
    public IUserMessage CallMessage { get; }
    public IUserMessage? ResponseMessage { get; }
    public string Expression { get; }
    public Number? Result { get; }
    public Node? Node { get; }
    public bool Success { get; }

    public Calculation(IUserMessage callMessage, IUserMessage? responseMessage, string expression, Number? result,
        Node node)
    {
        CallMessage = callMessage;
        ResponseMessage = responseMessage;
        Expression = expression;
        Result = result;
        Node = node;
        Success = result is not null;
    }

    public Calculation(IUserMessage callMessage, string expression)
    {
        CallMessage = callMessage;
        ResponseMessage = null;
        Expression = expression;
        Result = null;
        Node = null;
        Success = false;
    }
}