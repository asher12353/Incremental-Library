using System;
using System.Collections.Generic;
using FiniteSkillTreeIncremental.Core;

namespace FiniteSkillTreeIncremental.Save
{
    [Serializable]
    public class CurrencySaveEntry
    {
        public string currencyId;
        public double amountMantissa;
        public long amountExponent;
        public double lifetimeMantissa;
        public long lifetimeExponent;

        public BigNumber GetAmount() => new BigNumber(amountMantissa, amountExponent);
        public BigNumber GetLifetime() => new BigNumber(lifetimeMantissa, lifetimeExponent);

        public void SetAmount(BigNumber value)
        {
            amountMantissa = value.Mantissa;
            amountExponent = value.Exponent;
        }

        public void SetLifetime(BigNumber value)
        {
            lifetimeMantissa = value.Mantissa;
            lifetimeExponent = value.Exponent;
        }
    }

    [Serializable]
    public class GeneratorSaveEntry
    {
        public string generatorId;
        public int level;
    }

    [Serializable]
    public class SkillNodeSaveEntry
    {
        public string nodeId;
        public int rank;
    }

    [Serializable]
    public class PrestigeSaveData
    {
        public int totalPrestiges;
        public double lifetimePrestigeCurrencyMantissa;
        public long lifetimePrestigeCurrencyExponent;
        public float currentRunTimeSeconds;
        public float fastestPrestigeTimeSeconds;
        public List<string> unlockedMilestones = new List<string>();

        public BigNumber GetLifetimePrestigeCurrency() => new BigNumber(lifetimePrestigeCurrencyMantissa, lifetimePrestigeCurrencyExponent);

        public void SetLifetimePrestigeCurrency(BigNumber value)
        {
            lifetimePrestigeCurrencyMantissa = value.Mantissa;
            lifetimePrestigeCurrencyExponent = value.Exponent;
        }
    }

    [Serializable]
    public class GameSaveData
    {
        public int saveVersion = 1;
        public long timestampUnixUtc;
        public List<CurrencySaveEntry> currencies = new List<CurrencySaveEntry>();
        public List<GeneratorSaveEntry> generators = new List<GeneratorSaveEntry>();
        public List<SkillNodeSaveEntry> skills = new List<SkillNodeSaveEntry>();
        public PrestigeSaveData prestige = new PrestigeSaveData();
    }
}
