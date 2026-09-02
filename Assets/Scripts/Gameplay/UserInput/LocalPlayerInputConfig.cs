using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.BossRoom.Gameplay.UserInput
{
    /// <summary>
    /// Configuration asset that defines input bindings for a specific player in local multiplayer.
    /// This allows multiple players to share the same device (keyboard, gamepad) with different bindings.
    /// </summary>
    [CreateAssetMenu(fileName = "LocalPlayerInputConfig", menuName = "BossRoom/Local Player Input Config")]
    public class LocalPlayerInputConfig : ScriptableObject
    {
        [Header("Player Identification")]
        [Tooltip("Which player number this config is for (0 = Player 1, 1 = Player 2, etc.)")]
        public int PlayerNumber;

        [Header("Movement Input")]
        [Tooltip("Input action for movement (WASD or gamepad stick)")]
        public InputActionReference MoveAction;

        [Tooltip("Input action for targeting/moving to position (Mouse or gamepad cursor)")]
        public InputActionReference PointAction;

        [Tooltip("Input action for selecting targets (Right click or gamepad button)")]
        public InputActionReference TargetAction;

        [Header("Skill Inputs")]
        [Tooltip("Input action for Skill 1 (Q or gamepad button)")]
        public InputActionReference Skill1Action;

        [Tooltip("Input action for Skill 2 (W or gamepad button)")]
        public InputActionReference Skill2Action;

        [Tooltip("Input action for Skill 3 (E or gamepad button)")]
        public InputActionReference Skill3Action;

        [Header("Emote Inputs")]
        [Tooltip("Input action for Emote 1")]
        public InputActionReference Emote1Action;

        [Tooltip("Input action for Emote 2")]
        public InputActionReference Emote2Action;

        [Tooltip("Input action for Emote 3")]
        public InputActionReference Emote3Action;

        [Tooltip("Input action for Emote 4")]
        public InputActionReference Emote4Action;

        [Header("UI Inputs")]
        [Tooltip("Input action for toggling emote bar")]
        public InputActionReference ToggleEmoteBarAction;

        [Tooltip("Input action for toggling network stats")]
        public InputActionReference ToggleNetworkStatsAction;
    }
}
