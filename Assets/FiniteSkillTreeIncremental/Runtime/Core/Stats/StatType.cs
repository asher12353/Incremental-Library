namespace FiniteSkillTreeIncremental.Core.Stats
{
    public enum StatType
    {
        GlobalProductionMultiplier,
        ClickPowerMultiplier,
        GeneratorSpeedMultiplier,
        SkillCostReduction,
        PrestigeRewardMultiplier,
        PrestigeRequirementReduction,
        Custom
    }

    public enum ModifierType
    {
        /// <summary>
        /// Added directly to the base value: (Base + Flat)
        /// </summary>
        Flat = 100,

        /// <summary>
        /// Added together additively with other PercentAdd modifiers: (1 + sum(PercentAdd))
        /// A value of 0.1 represents +10%.
        /// </summary>
        PercentAdd = 200,

        /// <summary>
        /// Multiplied consecutively: product(1 + PercentMult)
        /// A value of 0.5 represents a 1.5x multiplier.
        /// </summary>
        PercentMult = 300
    }
}
