using System;

namespace HetroTradingRules.TestParticipant.Console.Solvers
{
    public class Brent : Solver1D
    {
        public Brent()
        {
        }

        public Brent(uint maxEvaluations)
            : base(maxEvaluations)
        {
        }

        public Brent(uint maxEvaluations, double? lowerBound, double? upperBound)
            : base(maxEvaluations, lowerBound, upperBound)
        {
        }

        protected override double solveImpl(Func<double, double> f, double xAccuracy)
        {
            /* The implementation of the algorithm was inspired by
               Press, Teukolsky, Vetterling, and Flannery,
               "Numerical Recipes in C", 2nd edition, Cambridge
               University Press
            */

            double min1, min2;
            double froot, p, q, r, s, xAcc1, xMid;
            // dummy assignements to avoid compiler warning
            double d = 0.0, e = 0.0;

            root_ = xMax_;
            froot = fxMax_;
            while (evaluationNumber_ <= maxEvaluations_)
            {
                if ((froot > 0.0 && fxMax_ > 0.0) ||
                    (froot < 0.0 && fxMax_ < 0.0))
                {
                    // Rename xMin_, root_, xMax_ and adjust bounds
                    xMax_ = xMin_;
                    fxMax_ = fxMin_;
                    e = d = root_ - xMin_;
                }
                if (fabs(fxMax_) < fabs(froot))
                {
                    xMin_ = root_;
                    root_ = xMax_;
                    xMax_ = xMin_;
                    fxMin_ = froot;
                    froot = fxMax_;
                    fxMax_ = fxMin_;
                }
                // Convergence check
                xAcc1 = 2.0 * QL_EPSILON * fabs(root_) + 0.5 * xAccuracy;
                xMid = (xMax_ - root_) / 2.0;
                if (fabs(xMid) <= xAcc1 || froot == 0.0)
                    return root_;
                if (fabs(e) >= xAcc1 &&
                    fabs(fxMin_) > fabs(froot))
                {
                    // Attempt inverse quadratic interpolation
                    s = froot / fxMin_;
                    if (xMin_ == xMax_)
                    {
                        p = 2.0 * xMid * s;
                        q = 1.0 - s;
                    }
                    else
                    {
                        q = fxMin_ / fxMax_;
                        r = froot / fxMax_;
                        p = s * (2.0 * xMid * q * (q - r) - (root_ - xMin_) * (r - 1.0));
                        q = (q - 1.0) * (r - 1.0) * (s - 1.0);
                    }
                    if (p > 0.0) q = -q; // Check whether in bounds
                    p = fabs(p);
                    min1 = 3.0 * xMid * q - fabs(xAcc1 * q);
                    min2 = fabs(e * q);
                    if (2.0 * p < (min1 < min2 ? min1 : min2))
                    {
                        e = d; // Accept interpolation
                        d = p / q;
                    }
                    else
                    {
                        d = xMid; // Interpolation failed, use bisection
                        e = d;
                    }
                }
                else
                {
                    // Bounds decreasing too slowly, use bisection
                    d = xMid;
                    e = d;
                }
                xMin_ = root_;
                fxMin_ = froot;
                if (fabs(d) > xAcc1)
                    root_ += d;
                else
                    root_ += sign(xAcc1, xMid);
                froot = f(root_);
                evaluationNumber_++;
            }
            throw new ApplicationException("maximum number of function evaluations ("
                                           + maxEvaluations_ + ") exceeded");
        }


        private double sign(double a, double b)
        {
            return b >= 0.0 ? Math.Abs(a) : -Math.Abs(a);
        }
    }
}