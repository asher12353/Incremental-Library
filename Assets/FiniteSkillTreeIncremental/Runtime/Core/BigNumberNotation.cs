namespace FiniteSkillTreeIncremental.Core
{
    public enum BigNumberNotation
    {
        /// <summary>
        /// Uses standard abbreviations like K, M, B, T, Qa, Qi, etc.
        /// </summary>
        Standard,

        /// <summary>
        /// Uses scientific notation like 1.23e15.
        /// </summary>
        Scientific,

        /// <summary>
        /// Uses engineering notation where exponents are multiples of 3, e.g. 12.34e12.
        /// </summary>
        Engineering,

        /// <summary>
        /// Uses short words like Thousand, Million, Billion, Trillion, etc.
        /// </summary>
        ShortWords
    }
}
