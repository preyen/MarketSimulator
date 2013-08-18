using System;

namespace HetroTradingRules.TestParticipant.Console.Solvers
{
    public abstract class Solver1D
    {
        #region Delegates

        public delegate double DFDX(double x);

        #endregion

        private const uint MAX_FUNCTION_EVALUATIONS = 100;
        protected const double QL_MIN_double = Double.MinValue;
        protected const string scientific = ""; //EMPTY token in C++ method
        protected readonly double QL_EPSILON = 0.0000000000001;
        protected uint evaluationNumber_;
        protected double fxMax_;
        protected double fxMin_;

        public double? LowerBound;
        protected uint maxEvaluations_;

        protected double root_;
        public double? UpperBound;
        protected double xMax_;
        protected double xMin_;

        protected Solver1D()
            : this(MAX_FUNCTION_EVALUATIONS)
        {
        }

        protected Solver1D(uint maxEvaluations)
        {
            maxEvaluations_ = maxEvaluations;
        }

        protected Solver1D(uint maxEvaluations, double? lowerBound, double? upperBound)
        {
            maxEvaluations_ = maxEvaluations;
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        private bool lowerBoundEnforced_
        {
            get { return LowerBound.HasValue; }
            set { }
        }

        private bool upperBoundEnforced_
        {
            get { return UpperBound.HasValue; }
            set { }
        }

        public uint MaxValuations
        {
            get { return maxEvaluations_; }
            set { maxEvaluations_ = value; }
        }

        protected abstract double solveImpl(Func<double, double> f, double xAccuracy);


        /**
         * Copy implementation bodies of both solve methods from C++ to C# file 
         * 
         * FindReplace "<<" with "+"
         * FindReplace "std::" with BLANK
         * FindReplace  "Real" with double
         * FindReplace  "Integer" with int
         * FindReplace  "this->impl()." with BLANK
         * FindReplace  "throw new ApplicationException" with "throw new ApplicationException"
         */

        public double solve(Func<double, double> f, double accuracy, double guess, double step)
        {
            QL_REQUIRE(accuracy > 0.0,
                       "accuracy (" + accuracy + ") must be positive");
            // check whether we doublely want to use epsilon
            accuracy = max(accuracy, QL_EPSILON);

            const double growthFactor = 1.6;
            int flipflop = -1;

            root_ = guess;
            fxMax_ = f(root_);

            // monotonically crescent bias, as in optionValue(volatility)
            if (fxMax_ == 0.0)
                return root_;
            else if (fxMax_ > 0.0)
            {
                xMin_ = enforceBounds_(root_ - step);
                fxMin_ = f(xMin_);
                xMax_ = root_;
            }
            else
            {
                xMin_ = root_;
                fxMin_ = fxMax_;
                xMax_ = enforceBounds_(root_ + step);
                fxMax_ = f(xMax_);
            }

            evaluationNumber_ = 2;
            while (evaluationNumber_ <= maxEvaluations_)
            {
                if (fxMin_ * fxMax_ <= 0.0)
                {
                    if (fxMin_ == 0.0) return xMin_;
                    if (fxMax_ == 0.0) return xMax_;
                    root_ = (xMax_ + xMin_) / 2.0;
                    return solveImpl(f, accuracy);
                }
                if (fabs(fxMin_) < fabs(fxMax_))
                {
                    xMin_ = enforceBounds_(xMin_ + growthFactor * (xMin_ - xMax_));
                    fxMin_ = f(xMin_);
                }
                else if (fabs(fxMin_) > fabs(fxMax_))
                {
                    xMax_ = enforceBounds_(xMax_ + growthFactor * (xMax_ - xMin_));
                    fxMax_ = f(xMax_);
                }
                else if (flipflop == -1)
                {
                    xMin_ = enforceBounds_(xMin_ + growthFactor * (xMin_ - xMax_));
                    fxMin_ = f(xMin_);
                    evaluationNumber_++;
                    flipflop = 1;
                }
                else if (flipflop == 1)
                {
                    xMax_ = enforceBounds_(xMax_ + growthFactor * (xMax_ - xMin_));
                    fxMax_ = f(xMax_);
                    flipflop = -1;
                }
                evaluationNumber_++;
            }

            throw new ApplicationException("unable to bracket root in " + maxEvaluations_
                                           + " function evaluations (last bracket attempt: "
                                           + "f[" + xMin_ + "," + xMax_ + "] "
                                           + "-> [" + fxMin_ + "," + fxMax_ + "])");
        }


        public double solve(Func<double, double> f, Double accuracy, double guess, double xMin, double xMax)
        {
            QL_REQUIRE(accuracy > 0.0,
                       "accuracy (" + accuracy + ") must be positive");
            // check whether we really want to use epsilon
            accuracy = max(accuracy, QL_EPSILON);

            xMin_ = xMin;
            xMax_ = xMax;

            QL_REQUIRE(xMin_ < xMax_,
                       "invalid range: xMin_ (" + xMin_
                       + ") >= xMax_ (" + xMax_ + ")");
            QL_REQUIRE(!lowerBoundEnforced_ || xMin_ >= LowerBound,
                       "xMin_ (" + xMin_
                       + ") < enforced low bound (" + LowerBound + ")");
            QL_REQUIRE(!upperBoundEnforced_ || xMax_ <= UpperBound,
                       "xMax_ (" + xMax_
                       + ") > enforced hi bound (" + UpperBound + ")");

            fxMin_ = f(xMin_);
            if (fxMin_ == 0.0)
                return xMin_;

            fxMax_ = f(xMax_);
            if (fxMax_ == 0.0)
                return xMax_;

            evaluationNumber_ = 2;

            QL_REQUIRE(fxMin_ * fxMax_ < 0.0,
                       "root not bracketed: f["
                       + xMin_ + "," + xMax_ + "] -> ["
                       + scientific
                       + fxMin_ + "," + fxMax_ + "]");

            QL_REQUIRE(guess >= xMin_,
                       "guess (" + guess + ") < xMin_ (" + xMin_ + ")");
            QL_REQUIRE(guess <= xMax_,
                       "guess (" + guess + ") > xMax_ (" + xMax_ + ")");

            root_ = guess;

            return solveImpl(f, accuracy);
        }

        protected double fabs(double d)
        {
            return Math.Abs(d);
        }

        protected double max(double a, double b)
        {
            return Math.Max(a, b);
        }

        protected void QL_REQUIRE(bool b, string s)
        {
            if (!b)
                throw new ApplicationException(s);
        }

        protected double enforceBounds_(double x)
        {
            if (lowerBoundEnforced_ && x < LowerBound)
                return (double)LowerBound;
            if (upperBoundEnforced_ && x > UpperBound)
                return (double)UpperBound;
            return x;
        }

        protected double sqrt(double d)
        {
            return Math.Sqrt(d);
        }
    }
}