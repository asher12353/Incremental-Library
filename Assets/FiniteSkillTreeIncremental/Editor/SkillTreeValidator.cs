using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FiniteSkillTreeIncremental.SkillTree;

namespace FiniteSkillTreeIncremental.Editor
{
    public class SkillTreeValidator : EditorWindow
    {
        private SkillTreeDefinition _targetTree;
        private Vector2 _scrollPos;
        private readonly List<string> _validationMessages = new List<string>();

        [MenuItem("Tools/Incremental/Skill Tree Validator")]
        public static void ShowWindow()
        {
            var window = GetWindow<SkillTreeValidator>("Skill Tree Validator");
            window.minSize = new Vector2(450, 400);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Skill Tree Dependency & Graph Validator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _targetTree = (SkillTreeDefinition)EditorGUILayout.ObjectField("Target Skill Tree", _targetTree, typeof(SkillTreeDefinition), false);

            EditorGUILayout.Space();

            if (GUILayout.Button("Validate Skill Tree Graph", GUILayout.Height(30)))
            {
                RunValidation();
            }

            EditorGUILayout.Space();
            GUILayout.Label("Validation Results:", EditorStyles.boldLabel);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            if (_validationMessages.Count == 0)
            {
                EditorGUILayout.HelpBox("Select a Skill Tree and click 'Validate Skill Tree Graph'.", MessageType.Info);
            }
            else
            {
                foreach (var msg in _validationMessages)
                {
                    if (msg.StartsWith("[ERROR]"))
                    {
                        EditorGUILayout.HelpBox(msg, MessageType.Error);
                    }
                    else if (msg.StartsWith("[WARNING]"))
                    {
                        EditorGUILayout.HelpBox(msg, MessageType.Warning);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(msg, MessageType.Info);
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void RunValidation()
        {
            _validationMessages.Clear();

            if (_targetTree == null)
            {
                _validationMessages.Add("[ERROR] No Skill Tree Definition selected.");
                return;
            }

            if (_targetTree.Nodes.Count == 0)
            {
                _validationMessages.Add("[WARNING] The selected Skill Tree contains 0 nodes.");
                return;
            }

            var nodeSet = new HashSet<SkillNodeDefinition>();
            var idSet = new HashSet<string>();

            // 1. Check for duplicates and null entries
            for (int i = 0; i < _targetTree.Nodes.Count; i++)
            {
                var node = _targetTree.Nodes[i];
                if (node == null)
                {
                    _validationMessages.Add($"[ERROR] Node index {i} is null in the tree list.");
                    continue;
                }

                if (string.IsNullOrEmpty(node.Id))
                {
                    _validationMessages.Add($"[ERROR] Node '{node.name}' has an empty or null NodeId.");
                }
                else if (idSet.Contains(node.Id))
                {
                    _validationMessages.Add($"[ERROR] Duplicate NodeId found: '{node.Id}' on asset '{node.name}'.");
                }
                else
                {
                    idSet.Add(node.Id);
                }

                nodeSet.Add(node);

                if (node.CostCurrency == null)
                {
                    _validationMessages.Add($"[WARNING] Node '{node.Title}' ({node.Id}) has no CostCurrency assigned.");
                }

                if (node.MaxRank <= 0)
                {
                    _validationMessages.Add($"[ERROR] Node '{node.Title}' ({node.Id}) MaxRank is <= 0.");
                }
            }

            // 2. Check for missing prerequisites outside graph
            foreach (var node in nodeSet)
            {
                if (node == null) continue;

                foreach (var prereq in node.Prerequisites)
                {
                    if (prereq.RequiredNode == null)
                    {
                        _validationMessages.Add($"[ERROR] Node '{node.Title}' ({node.Id}) has a null prerequisite reference.");
                    }
                    else if (!nodeSet.Contains(prereq.RequiredNode))
                    {
                        _validationMessages.Add($"[WARNING] Node '{node.Title}' ({node.Id}) depends on '{prereq.RequiredNode.Title}' which is not in this tree definition.");
                    }
                    else if (prereq.RequiredRank > prereq.RequiredNode.MaxRank)
                    {
                        _validationMessages.Add($"[ERROR] Node '{node.Title}' requires Rank {prereq.RequiredRank} of '{prereq.RequiredNode.Title}', but its MaxRank is {prereq.RequiredNode.MaxRank}.");
                    }
                }
            }

            // 3. Cycle Detection (DFS)
            var visited = new Dictionary<SkillNodeDefinition, int>(); // 0: unvisited, 1: visiting, 2: visited
            bool hasCycle = false;

            foreach (var node in nodeSet)
            {
                if (node == null) continue;
                if (!visited.ContainsKey(node))
                {
                    if (DetectCycleDFS(node, visited, new List<SkillNodeDefinition>()))
                    {
                        hasCycle = true;
                    }
                }
            }

            if (!hasCycle && _validationMessages.Count == 0)
            {
                _validationMessages.Add($"[SUCCESS] Skill Tree '{_targetTree.TreeName}' validated successfully! ({_targetTree.Nodes.Count} nodes, no cycles or errors detected)");
            }
        }

        private bool DetectCycleDFS(SkillNodeDefinition current, Dictionary<SkillNodeDefinition, int> visited, List<SkillNodeDefinition> path)
        {
            visited[current] = 1; // Visiting
            path.Add(current);

            foreach (var prereq in current.Prerequisites)
            {
                var next = prereq.RequiredNode;
                if (next == null) continue;

                if (visited.TryGetValue(next, out int state))
                {
                    if (state == 1) // Cycle detected
                    {
                        path.Add(next);
                        string cyclePath = string.Join(" -> ", path.ConvertAll(n => n.Title));
                        _validationMessages.Add($"[ERROR] Circular dependency detected in prerequisite chain: {cyclePath}");
                        return true;
                    }
                }
                else
                {
                    if (DetectCycleDFS(next, visited, path))
                    {
                        return true;
                    }
                }
            }

            visited[current] = 2; // Visited
            path.RemoveAt(path.Count - 1);
            return false;
        }
    }
}
