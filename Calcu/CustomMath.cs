namespace Calcu;

public static class CustomMath
{
    public static double Quadratic(double a, double b, double c)
    {
        var sqrt = Math.Sqrt(b * b - 4 * a * c);
        return (-b + sqrt) / (2 * a);
        // TODO: return both solutions
    }
}