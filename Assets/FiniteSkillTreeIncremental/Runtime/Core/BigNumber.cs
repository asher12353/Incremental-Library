using System;
using System.Globalization;
using UnityEngine;

namespace FiniteSkillTreeIncremental.Core
{
    /// <summary>
    /// High-precision scientific notation number struct for incremental / idle games.
    /// Represents numbers as mantissa * 10^exponent where 1.0 <= |mantissa| < 10.0 (or mantissa == 0).
    /// Supports virtually unlimited scale (exponent stored as long).
    /// </summary>
    [Serializable]
    public struct BigNumber : IEquatable<BigNumber>, IComparable<BigNumber>, IComparable, ISerializationCallbackReceiver
    {
        [SerializeField] private double _mantissa;
        [SerializeField] private long _exponent;

        public static readonly BigNumber Zero = new BigNumber(0.0, 0);
        public static readonly BigNumber One = new BigNumber(1.0, 0);
        public static readonly BigNumber Ten = new BigNumber(1.0, 1);
        public static readonly BigNumber MaxValue = new BigNumber(9.99999999999999, long.MaxValue);
        public static readonly BigNumber MinValue = new BigNumber(-9.99999999999999, long.MaxValue);

        private static readonly string[] StandardSuffixes = new string[]
        {
            "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc",
            "Ud", "Dd", "Td", "Qad", "Qid", "Sxd", "Spd", "Ocd", "Nod", "Vg",
            "Uvg", "Dvg", "Tvg", "Qavg", "Qivg", "Sxvg", "Spvg", "Ocvg", "Novg", "Tg",
            "Utg", "Dtg", "Ttg", "Qatg", "Qitg", "Sxtg", "Sptg", "Octg", "Notg", "Qag",
            "Uqag", "Dqag", "Tqag", "Qaqag", "Qiqag", "Sxqag", "Spqag", "Ocqag", "Noqag", "Qig",
            "Uqig", "Dqig", "Tqig", "Qaqig", "Qiqig", "Sxqig", "Spqig", "Ocqig", "Noqig", "Sxg",
            "Usxg", "Dsxg", "Tsxg", "Qasxg", "Qisxg", "Sxsxg", "Spsxg", "Ocsxg", "Nosxg", "Spg",
            "Uspg", "Dspg", "Tspg", "Qaspg", "Qispg", "Sxspg", "Spspg", "Ocspg", "Nospg", "Ocg",
            "Uocg", "Docg", "Tocg", "Qaocg", "Qiocg", "Sxocg", "Spocg", "Ococg", "Noocg", "Nog",
            "Unog", "Dnog", "Tnog", "Qanog", "Qinog", "Sxnog", "Spnog", "Ocnog", "Nonog", "Cent"
        };

        private static readonly string[] ShortWordsSuffixes = new string[]
        {
            "", " Thousand", " Million", " Billion", " Trillion", " Quadrillion", " Quintillion",
            " Sextillion", " Septillion", " Octillion", " Nonillion", " Decillion",
            " Undecillion", " Duodecillion", " Tredecillion", " Quattuordecillion", " Quindecillion"
        };

        public double Mantissa => _mantissa;
        public long Exponent => _exponent;
        public bool IsZero => _mantissa == 0.0;
        public bool IsNegative => _mantissa < 0.0;
        public bool IsPositive => _mantissa > 0.0;
        public int Sign => _mantissa > 0.0 ? 1 : (_mantissa < 0.0 ? -1 : 0);

        public BigNumber(double value)
        {
            _mantissa = value;
            _exponent = 0;
            Normalize(ref _mantissa, ref _exponent);
        }

        public BigNumber(double mantissa, long exponent)
        {
            _mantissa = mantissa;
            _exponent = exponent;
            Normalize(ref _mantissa, ref _exponent);
        }

        public BigNumber(long value) : this((double)value) { }
        public BigNumber(int value) : this((double)value) { }
        public BigNumber(float value) : this((double)value) { }

        public static BigNumber FromScientific(double mantissa, long exponent)
        {
            return new BigNumber(mantissa, exponent);
        }

        public static BigNumber Pow10(long exponent)
        {
            return new BigNumber(1.0, exponent);
        }

        private static void Normalize(ref double mantissa, ref long exponent)
        {
            if (double.IsNaN(mantissa) || double.IsInfinity(mantissa) || mantissa == 0.0)
            {
                mantissa = 0.0;
                exponent = 0;
                return;
            }

            double abs = Math.Abs(mantissa);
            if (abs >= 10.0)
            {
                int shift = (int)Math.Floor(Math.Log10(abs));
                mantissa /= Math.Pow(10, shift);
                exponent += shift;
            }
            else if (abs < 1.0)
            {
                int shift = (int)Math.Floor(Math.Log10(abs));
                mantissa /= Math.Pow(10, shift);
                exponent += shift;
            }
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            Normalize(ref _mantissa, ref _exponent);
        }

        #region Arithmetic Operators

        public static BigNumber operator +(BigNumber a, BigNumber b)
        {
            if (a.IsZero) return b;
            if (b.IsZero) return a;

            long diff = a._exponent - b._exponent;
            if (diff >= 16) return a;
            if (diff <= -16) return b;

            if (diff >= 0)
            {
                double m = a._mantissa + b._mantissa / Math.Pow(10, diff);
                return new BigNumber(m, a._exponent);
            }
            else
            {
                double m = a._mantissa / Math.Pow(10, -diff) + b._mantissa;
                return new BigNumber(m, b._exponent);
            }
        }

        public static BigNumber operator -(BigNumber a, BigNumber b)
        {
            if (b.IsZero) return a;
            if (a.IsZero) return new BigNumber(-b._mantissa, b._exponent);

            long diff = a._exponent - b._exponent;
            if (diff >= 16) return a;
            if (diff <= -16) return new BigNumber(-b._mantissa, b._exponent);

            if (diff >= 0)
            {
                double m = a._mantissa - b._mantissa / Math.Pow(10, diff);
                return new BigNumber(m, a._exponent);
            }
            else
            {
                double m = a._mantissa / Math.Pow(10, -diff) - b._mantissa;
                return new BigNumber(m, b._exponent);
            }
        }

        public static BigNumber operator *(BigNumber a, BigNumber b)
        {
            if (a.IsZero || b.IsZero) return Zero;
            return new BigNumber(a._mantissa * b._mantissa, a._exponent + b._exponent);
        }

        public static BigNumber operator /(BigNumber a, BigNumber b)
        {
            if (b.IsZero) throw new DivideByZeroException("Cannot divide BigNumber by zero.");
            if (a.IsZero) return Zero;
            return new BigNumber(a._mantissa / b._mantissa, a._exponent - b._exponent);
        }

        public static BigNumber operator -(BigNumber a)
        {
            return new BigNumber(-a._mantissa, a._exponent);
        }

        public static BigNumber operator ++(BigNumber a) => a + One;
        public static BigNumber operator --(BigNumber a) => a - One;

        public static BigNumber operator +(BigNumber a, double b) => a + new BigNumber(b);
        public static BigNumber operator +(double a, BigNumber b) => new BigNumber(a) + b;
        public static BigNumber operator -(BigNumber a, double b) => a - new BigNumber(b);
        public static BigNumber operator -(double a, BigNumber b) => new BigNumber(a) - b;
        public static BigNumber operator *(BigNumber a, double b) => a * new BigNumber(b);
        public static BigNumber operator *(double a, BigNumber b) => new BigNumber(a) * b;
        public static BigNumber operator /(BigNumber a, double b) => a / new BigNumber(b);
        public static BigNumber operator /(double a, BigNumber b) => new BigNumber(a) / b;

        #endregion

        #region Comparisons

        public static bool operator ==(BigNumber a, BigNumber b)
        {
            if (a.IsZero && b.IsZero) return true;
            return a._exponent == b._exponent && Math.Abs(a._mantissa - b._mantissa) < 1e-12;
        }

        public static bool operator !=(BigNumber a, BigNumber b) => !(a == b);

        public static bool operator <(BigNumber a, BigNumber b)
        {
            if (a.IsZero) return b._mantissa > 0;
            if (b.IsZero) return a._mantissa < 0;

            if (a.Sign != b.Sign) return a.Sign < b.Sign;

            if (a._exponent != b._exponent)
            {
                return a.Sign > 0 ? a._exponent < b._exponent : a._exponent > b._exponent;
            }

            return a._mantissa < b._mantissa;
        }

        public static bool operator <=(BigNumber a, BigNumber b) => a < b || a == b;
        public static bool operator >(BigNumber a, BigNumber b) => !(a <= b);
        public static bool operator >=(BigNumber a, BigNumber b) => !(a < b);

        public override bool Equals(object obj) => obj is BigNumber other && Equals(other);
        public bool Equals(BigNumber other) => this == other;

        public override int GetHashCode()
        {
            unchecked
            {
                return (_mantissa.GetHashCode() * 397) ^ _exponent.GetHashCode();
            }
        }

        public int CompareTo(BigNumber other)
        {
            if (this < other) return -1;
            if (this > other) return 1;
            return 0;
        }

        public int CompareTo(object obj)
        {
            if (obj is BigNumber other) return CompareTo(other);
            throw new ArgumentException("Object is not a BigNumber");
        }

        #endregion

        #region Conversions

        public static implicit operator BigNumber(double value) => new BigNumber(value);
        public static implicit operator BigNumber(float value) => new BigNumber(value);
        public static implicit operator BigNumber(int value) => new BigNumber(value);
        public static implicit operator BigNumber(long value) => new BigNumber(value);

        public double ToDouble()
        {
            if (IsZero) return 0.0;
            if (_exponent > 308) return _mantissa > 0 ? double.PositiveInfinity : double.NegativeInfinity;
            if (_exponent < -308) return 0.0;
            return _mantissa * Math.Pow(10, _exponent);
        }

        public float ToFloat() => (float)ToDouble();
        public long ToLong() => (long)ToDouble();
        public int ToInt() => (int)ToDouble();

        #endregion

        #region Mathematical Functions

        public static BigNumber Abs(BigNumber value) => new BigNumber(Math.Abs(value._mantissa), value._exponent);

        public static BigNumber Min(BigNumber a, BigNumber b) => a < b ? a : b;
        public static BigNumber Max(BigNumber a, BigNumber b) => a > b ? a : b;
        public static BigNumber Clamp(BigNumber value, BigNumber min, BigNumber max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static BigNumber Floor(BigNumber value)
        {
            if (value._exponent >= 16) return value;
            return new BigNumber(Math.Floor(value.ToDouble()));
        }

        public static BigNumber Ceil(BigNumber value)
        {
            if (value._exponent >= 16) return value;
            return new BigNumber(Math.Ceiling(value.ToDouble()));
        }

        public static BigNumber Round(BigNumber value)
        {
            if (value._exponent >= 16) return value;
            return new BigNumber(Math.Round(value.ToDouble()));
        }

        public static BigNumber Sqrt(BigNumber value)
        {
            if (value.IsNegative) throw new ArgumentOutOfRangeException(nameof(value), "Cannot take square root of negative BigNumber");
            if (value.IsZero) return Zero;

            double m = value._mantissa;
            long exp = value._exponent;

            if (exp % 2 != 0)
            {
                m *= 10;
                exp -= 1;
            }

            return new BigNumber(Math.Sqrt(m), exp / 2);
        }

        public static BigNumber Pow(BigNumber baseValue, double exponent)
        {
            if (exponent == 0) return One;
            if (baseValue.IsZero) return Zero;
            if (exponent == 1) return baseValue;

            double totalExp = (Math.Log10(Math.Abs(baseValue._mantissa)) + baseValue._exponent) * exponent;
            long newExp = (long)Math.Floor(totalExp);
            double newMantissa = Math.Pow(10, totalExp - newExp);

            if (baseValue.IsNegative && (long)exponent % 2 != 0)
            {
                newMantissa = -newMantissa;
            }

            return new BigNumber(newMantissa, newExp);
        }

        #endregion

        #region Formatting & Parsing

        public override string ToString()
        {
            return ToString(BigNumberNotation.Standard, 2);
        }

        public string ToString(BigNumberNotation notation, int decimals = 2)
        {
            if (IsZero) return "0";
            if (double.IsNaN(_mantissa) || double.IsInfinity(_mantissa)) return "NaN";

            decimals = Math.Max(0, decimals);
            string decFormat = "F" + decimals;

            if (_exponent < 3 && _exponent >= 0)
            {
                double val = ToDouble();
                return decimals == 0 ? Math.Floor(val).ToString(CultureInfo.InvariantCulture) : val.ToString(decFormat, CultureInfo.InvariantCulture);
            }
            if (_exponent < 0)
            {
                return ToDouble().ToString(decFormat, CultureInfo.InvariantCulture);
            }

            switch (notation)
            {
                case BigNumberNotation.Standard:
                {
                    long tier = _exponent / 3;
                    if (tier < StandardSuffixes.Length)
                    {
                        int remainder = (int)(_exponent % 3);
                        double scaledMantissa = _mantissa * Math.Pow(10, remainder);
                        return string.Format(CultureInfo.InvariantCulture, "{0:" + decFormat + "}{1}", scaledMantissa, StandardSuffixes[tier]);
                    }
                    // Fallback to scientific if beyond standard suffixes table
                    return string.Format(CultureInfo.InvariantCulture, "{0:" + decFormat + "}e{1}", _mantissa, _exponent);
                }

                case BigNumberNotation.Scientific:
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0:" + decFormat + "}e{1}", _mantissa, _exponent);
                }

                case BigNumberNotation.Engineering:
                {
                    int remainder = (int)(_exponent % 3);
                    if (remainder < 0) remainder += 3;
                    double scaledMantissa = _mantissa * Math.Pow(10, remainder);
                    long engExp = _exponent - remainder;
                    return string.Format(CultureInfo.InvariantCulture, "{0:" + decFormat + "}e{1}", scaledMantissa, engExp);
                }

                case BigNumberNotation.ShortWords:
                {
                    long tier = _exponent / 3;
                    if (tier < ShortWordsSuffixes.Length)
                    {
                        int remainder = (int)(_exponent % 3);
                        double scaledMantissa = _mantissa * Math.Pow(10, remainder);
                        return string.Format(CultureInfo.InvariantCulture, "{0:" + decFormat + "}{1}", scaledMantissa, ShortWordsSuffixes[tier]);
                    }
                    return string.Format(CultureInfo.InvariantCulture, "{0:" + decFormat + "}e{1}", _mantissa, _exponent);
                }

                default:
                    return string.Format(CultureInfo.InvariantCulture, "{0:" + decFormat + "}e{1}", _mantissa, _exponent);
            }
        }

        public static bool TryParse(string input, out BigNumber result)
        {
            result = Zero;
            if (string.IsNullOrWhiteSpace(input)) return false;

            input = input.Trim();

            // Try standard double parse first
            if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double directVal))
            {
                result = new BigNumber(directVal);
                return true;
            }

            // Check scientific notation "1.23e15" or "1.23E+15"
            int eIndex = input.IndexOfAny(new char[] { 'e', 'E' });
            if (eIndex > 0)
            {
                string mantissaStr = input.Substring(0, eIndex);
                string expStr = input.Substring(eIndex + 1);

                if (double.TryParse(mantissaStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double m) &&
                    long.TryParse(expStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out long exp))
                {
                    result = new BigNumber(m, exp);
                    return true;
                }
            }

            // Check standard suffix matching (e.g. "1.25M", "450K")
            for (int i = StandardSuffixes.Length - 1; i >= 1; i--)
            {
                string suffix = StandardSuffixes[i];
                if (input.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    string numPart = input.Substring(0, input.Length - suffix.Length).Trim();
                    if (double.TryParse(numPart, NumberStyles.Float, CultureInfo.InvariantCulture, out double baseNum))
                    {
                        result = new BigNumber(baseNum) * Pow10(i * 3);
                        return true;
                    }
                }
            }

            return false;
        }

        public static BigNumber Parse(string input)
        {
            if (TryParse(input, out BigNumber result))
            {
                return result;
            }
            throw new FormatException($"Unable to parse '{input}' as BigNumber.");
        }

        #endregion
    }
}
