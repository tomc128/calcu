namespace Calcu;

public static class CustomMath
{
    public static double Quadratic(double a, double b, double c)
    {
        var sqrt = Math.Sqrt(b * b - 4 * a * c);
        return (-b + sqrt) / (2 * a);
        // TODO: return both solutions
    }


    // def ncr(n, r):
    //     return math.factorial(n) / (math.factorial(r) * math.factorial(n - r))
    //
    // def npr(n, r):
    //     return math.factorial(n) / math.factorial(n - r) 

    public static double Factorial(int n)
    {
        if (n <= 1) return 1;
        return n * Factorial(n - 1);
    }

    public static double Combination(int n, int r)
    {
        if (n < r) throw new ArgumentException("n must be greater than or equal to r");
        return Factorial(n) / (Factorial(r) * Factorial(n - r));
    }

    public static double Permutation(int n, int r)
    {
        if (n < r) throw new ArgumentException("n must be greater than or equal to r");
        return Factorial(n) / Factorial(n - r);
    }
}