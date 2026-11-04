using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using FiniteSkillTreeIncremental.Core;
using FiniteSkillTreeIncremental.Currencies;

namespace FiniteSkillTreeIncremental.Testing
{
    public class DebugCheatUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _time1xButton;
        [SerializeField] private Button _time5xButton;
        [SerializeField] private Button _time20xButton;
        [SerializeField] private Button _time100xButton;

        [SerializeField] private Button _addGold1MButton;
        [SerializeField] private Button _addGold1BButton;
        [SerializeField] private Button _addPrestigeShardsButton;
        [SerializeField] private Button _fastForward1HourButton;
        [SerializeField] private Button _unlockAllSkillsButton;
        [SerializeField] private Button _respecSkillsButton;
        [SerializeField] private Button _forcePrestigeButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _wipeSaveButton;
        [SerializeField] private Button _closeButton;

        [Header("Settings")]
        [SerializeField] private Key _toggleKey = Key.F1;
        [SerializeField] private Key _alternateToggleKey = Key.Backquote; // Tilde

        private DebugCheatManager _cheatManager;

        private void Start()
        {
            _cheatManager = DebugCheatManager.Instance;
            if (_cheatManager == null)
            {
                var go = new GameObject("DebugCheatManager");
                _cheatManager = go.AddComponent<DebugCheatManager>();
            }

            SetupButtons();
        }

        private void SetupButtons()
        {
            if (_time1xButton != null) _time1xButton.onClick.AddListener(() => _cheatManager.SetTimeScale(1f));
            if (_time5xButton != null) _time5xButton.onClick.AddListener(() => _cheatManager.SetTimeScale(5f));
            if (_time20xButton != null) _time20xButton.onClick.AddListener(() => _cheatManager.SetTimeScale(20f));
            if (_time100xButton != null) _time100xButton.onClick.AddListener(() => _cheatManager.SetTimeScale(100f));

            if (_addGold1MButton != null) _addGold1MButton.onClick.AddListener(() => _cheatManager.GrantCurrency("gold", new BigNumber(1000000)));
            if (_addGold1BButton != null) _addGold1BButton.onClick.AddListener(() => _cheatManager.GrantCurrency("gold", new BigNumber(1000000000)));
            if (_addPrestigeShardsButton != null) _addPrestigeShardsButton.onClick.AddListener(() => _cheatManager.GrantCurrency("prestige_shards", new BigNumber(100)));
            if (_fastForward1HourButton != null) _fastForward1HourButton.onClick.AddListener(() => _cheatManager.FastForwardSeconds(3600f));

            if (_unlockAllSkillsButton != null) _unlockAllSkillsButton.onClick.AddListener(() => _cheatManager.UnlockAllSkills());
            if (_respecSkillsButton != null) _respecSkillsButton.onClick.AddListener(() => _cheatManager.RespecAllSkills());
            if (_forcePrestigeButton != null) _forcePrestigeButton.onClick.AddListener(() => _cheatManager.ForcePrestige());
            if (_saveButton != null) _saveButton.onClick.AddListener(() => _cheatManager.SaveNow());
            if (_wipeSaveButton != null) _wipeSaveButton.onClick.AddListener(() => _cheatManager.WipeSave());

            if (_closeButton != null) _closeButton.onClick.AddListener(TogglePanel);
        }

        private void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current[_toggleKey].wasPressedThisFrame ||
                    Keyboard.current[_alternateToggleKey].wasPressedThisFrame)
                {
                    TogglePanel();
                }
            }
        }

        public void TogglePanel()
        {
            if (_panel != null)
            {
                _panel.SetActive(!_panel.activeSelf);
            }
        }
    }
}
