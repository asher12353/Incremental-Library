using System;
using UnityEngine;

namespace FiniteSkillTreeIncremental.SkillTree
{
    public enum SkillNodeType
    {
        /// <summary>
        /// Minor incremental stat node (e.g. +5% Gold, +10% Speed).
        /// </summary>
        Minor,

        /// <summary>
        /// Notable node offering substantial multipliers or utility perks.
        /// </summary>
        Notable,

        /// <summary>
        /// Keystone node offering build-defining, game-altering mechanics or transmutations.
        /// </summary>
        Keystone,

        /// <summary>
        /// Gateway/Bridge node unlocking connected constellation clusters or prestige tiers.
        /// </summary>
        Gateway
    }

    [Serializable]
    public struct SkillNodePrerequisite
    {
        [SerializeField] private SkillNodeDefinition _requiredNode;
        [SerializeField] private int _requiredRank;

        public SkillNodeDefinition RequiredNode => _requiredNode;
        public int RequiredRank => _requiredRank <= 0 ? 1 : _requiredRank;

        public SkillNodePrerequisite(SkillNodeDefinition node, int rank = 1)
        {
            _requiredNode = node;
            _requiredRank = rank;
        }
    }
}
