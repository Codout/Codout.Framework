namespace Codout.Framework.Common.Helpers
{
    public static class DistanceMath
    {
        public static double Phi(double x)
        {
            // constants
            const double a1 = 0.254829592;
            const double a2 = -0.284496736;
            const double a3 = 1.421413741;
            const double a4 = -1.453152027;
            const double a5 = 1.061405429;
            const double p = 0.3275911;

            // Save the sign of x
            var sign = 1;
            if (x < 0)
                sign = -1;
            x = System.Math.Abs(x) / System.Math.Sqrt(2.0);

            // A&S formula 7.1.26
            var t = 1.0 / (1.0 + p * x);
            var y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * System.Math.Exp(-x * x);

            return 0.5 * (1.0 + sign * y);
        }
    }
}
