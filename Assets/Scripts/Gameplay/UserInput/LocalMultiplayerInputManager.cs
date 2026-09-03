using System;
using System.Collections.Generic;
using Unity.BossRoom.Gameplay.Actions;
using Unity.BossRoom.Gameplay.Configuration;
using Unity.BossRoom.Gameplay.GameplayObjects;
using Unity.BossRoom.Gameplay.GameplayObjects.Character;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.BossRoom.Gameplay.UserInput
{
    /// <summary>
    /// Manages input for multiple local players sharing the same device (keyboard, mouse, gamepads).
    /// This enables couch co-op / local multiplayer where 2+ players can play on the same PC instance.
    /// </summary>
    public class LocalMultiplayerInputManager : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("List of input configurations for each local player")]
        public List<LocalPlayerInputConfig> m_PlayerInputConfigs = new List<LocalPlayerInputConfig>();

        [Header("References")]
        [Tooltip("Reference to the ServerCharacter component this manager controls")]
        [SerializeField]
        ServerCharacter m_ServerCharacter;

        /// <summary>
        /// Which player number (0-based) this input manager is for
        /// </summary>
        public int PlayerNumber { get; private set; } = -1;

        /// <summary>
        /// The input configuration for this player
        /// </summary>
        public LocalPlayerInputConfig InputConfig { get; private set; }

        /// <summary>
        /// Reference to the ClientInputSender that will receive our input
        /// </summary>
        ClientInputSender m_ClientInputSender;

        /// <summary>
        /// Whether this input manager is currently active and sending input
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Event fired when this player's input wants to move to a position
        /// </summary>
        public event Action<Vector3> MoveInputEvent;

        /// <summary>
        /// Event fired when this player triggers an action
        /// </summary>
        public event Action<ActionRequestData> ActionInputEvent;

        void Awake()
        {
            if (m_ServerCharacter == null)
            {
                m_ServerCharacter = GetComponent<ServerCharacter>();
            }
        }

        /// <summary>
        /// Initialize this input manager for a specific player number with the given configuration
        /// </summary>
        public void Initialize(int playerNumber, LocalPlayerInputConfig config)
        {
            PlayerNumber = playerNumber;
            InputConfig = config;
            IsActive = true;

            SetupInputBindings();
        }

        /// <summary>
        /// Setup all input bindings from the configuration
        /// </summary>
        void SetupInputBindings()
        {
            if (InputConfig == null || !IsActive)
            {
                enabled = false;
                return;
            }

            enabled = true;

            // Subscribe to all input actions from the config
            if (InputConfig.Skill1Action != null)
            {
                InputConfig.Skill1Action.action.started += OnSkill1Started;
                InputConfig.Skill1Action.action.canceled += OnSkill1Canceled;
            }

            if (InputConfig.Skill2Action != null)
            {
                InputConfig.Skill2Action.action.started += OnSkill2Started;
                InputConfig.Skill2Action.action.canceled += OnSkill2Canceled;
            }

            if (InputConfig.Skill3Action != null)
            {
                InputConfig.Skill3Action.action.started += OnSkill3Started;
                InputConfig.Skill3Action.action.canceled += OnSkill3Canceled;
            }

            if (InputConfig.Emote1Action != null)
            {
                InputConfig.Emote1Action.action.performed += OnEmote1Performed;
            }

            if (InputConfig.Emote2Action != null)
            {
                InputConfig.Emote2Action.action.performed += OnEmote2Performed;
            }

            if (InputConfig.Emote3Action != null)
            {
                InputConfig.Emote3Action.action.performed += OnEmote3Performed;
            }

            if (InputConfig.Emote4Action != null)
            {
                InputConfig.Emote4Action.action.performed += OnEmote4Performed;
            }

            if (InputConfig.TargetAction != null)
            {
                InputConfig.TargetAction.action.started += OnTargetStarted;
            }

            if (InputConfig.PointAction != null)
            {
                InputConfig.PointAction.action.Enable();
            }
        }

        /// <summary>
        /// Cleanup all input bindings
        /// </summary>
        void CleanupInputBindings()
        {
            if (InputConfig == null) return;

            if (InputConfig.Skill1Action != null)
            {
                InputConfig.Skill1Action.action.started -= OnSkill1Started;
                InputConfig.Skill1Action.action.canceled -= OnSkill1Canceled;
            }

            if (InputConfig.Skill2Action != null)
            {
                InputConfig.Skill2Action.action.started -= OnSkill2Started;
                InputConfig.Skill2Action.action.canceled -= OnSkill2Canceled;
            }

            if (InputConfig.Skill3Action != null)
            {
                InputConfig.Skill3Action.action.started -= OnSkill3Started;
                InputConfig.Skill3Action.action.canceled -= OnSkill3Canceled;
            }

            if (InputConfig.Emote1Action != null)
            {
                InputConfig.Emote1Action.action.performed -= OnEmote1Performed;
            }

            if (InputConfig.Emote2Action != null)
            {
                InputConfig.Emote2Action.action.performed -= OnEmote2Performed;
            }

            if (InputConfig.Emote3Action != null)
            {
                InputConfig.Emote3Action.action.performed -= OnEmote3Performed;
            }

            if (InputConfig.Emote4Action != null)
            {
                InputConfig.Emote4Action.action.performed -= OnEmote4Performed;
            }

            if (InputConfig.TargetAction != null)
            {
                InputConfig.TargetAction.action.started -= OnTargetStarted;
            }
        }

        void OnDestroy()
        {
            CleanupInputBindings();
        }

        void OnEnable()
        {
            if (InputConfig != null && IsActive)
            {
                EnableAllActions();
            }
        }

        void OnDisable()
        {
            if (InputConfig != null)
            {
                DisableAllActions();
            }
        }

        void EnableAllActions()
        {
            if (InputConfig.Skill1Action != null) InputConfig.Skill1Action.action.Enable();
            if (InputConfig.Skill2Action != null) InputConfig.Skill2Action.action.Enable();
            if (InputConfig.Skill3Action != null) InputConfig.Skill3Action.action.Enable();
            if (InputConfig.Emote1Action != null) InputConfig.Emote1Action.action.Enable();
            if (InputConfig.Emote2Action != null) InputConfig.Emote2Action.action.Enable();
            if (InputConfig.Emote3Action != null) InputConfig.Emote3Action.action.Enable();
            if (InputConfig.Emote4Action != null) InputConfig.Emote4Action.action.Enable();
            if (InputConfig.TargetAction != null) InputConfig.TargetAction.action.Enable();
            if (InputConfig.PointAction != null) InputConfig.PointAction.action.Enable();
        }

        void DisableAllActions()
        {
            if (InputConfig.Skill1Action != null) InputConfig.Skill1Action.action.Disable();
            if (InputConfig.Skill2Action != null) InputConfig.Skill2Action.action.Disable();
            if (InputConfig.Skill3Action != null) InputConfig.Skill3Action.action.Disable();
            if (InputConfig.Emote1Action != null) InputConfig.Emote1Action.action.Disable();
            if (InputConfig.Emote2Action != null) InputConfig.Emote2Action.action.Disable();
            if (InputConfig.Emote3Action != null) InputConfig.Emote3Action.action.Disable();
            if (InputConfig.Emote4Action != null) InputConfig.Emote4Action.action.Disable();
            if (InputConfig.TargetAction != null) InputConfig.TargetAction.action.Disable();
            if (InputConfig.PointAction != null) InputConfig.PointAction.action.Disable();
        }

        #region Input Handlers

        void OnSkill1Started(InputAction.CallbackContext context)
        {
            if (!IsActive || m_ServerCharacter == null || m_ServerCharacter.CharacterClass == null) return;
            RequestAction(m_ServerCharacter.CharacterClass.Skill1.ActionID, ClientInputSender.SkillTriggerStyle.Keyboard);
        }

        void OnSkill1Canceled(InputAction.CallbackContext context)
        {
            if (!IsActive || m_ServerCharacter == null || m_ServerCharacter.CharacterClass == null) return;
            RequestAction(m_ServerCharacter.CharacterClass.Skill1.ActionID, ClientInputSender.SkillTriggerStyle.KeyboardRelease);
        }

        void OnSkill2Started(InputAction.CallbackContext context)
        {
            if (!IsActive || m_ServerCharacter == null || m_ServerCharacter.CharacterClass == null) return;
            
            if (GameDataSource.Instance.TryGetActionPrototypeByID(m_ServerCharacter.CharacterClass.Skill2.ActionID, out var skill2))
            {
                RequestAction(skill2.ActionID, ClientInputSender.SkillTriggerStyle.Keyboard);
            }
        }

        void OnSkill2Canceled(InputAction.CallbackContext context)
        {
            if (!IsActive || m_ServerCharacter == null || m_ServerCharacter.CharacterClass == null) return;
            
            if (GameDataSource.Instance.TryGetActionPrototypeByID(m_ServerCharacter.CharacterClass.Skill2.ActionID, out var skill2))
            {
                RequestAction(skill2.ActionID, ClientInputSender.SkillTriggerStyle.KeyboardRelease);
            }
        }

        void OnSkill3Started(InputAction.CallbackContext context)
        {
            if (!IsActive || m_ServerCharacter == null || m_ServerCharacter.CharacterClass == null) return;
            
            if (GameDataSource.Instance.TryGetActionPrototypeByID(m_ServerCharacter.CharacterClass.Skill3.ActionID, out var skill3))
            {
                RequestAction(skill3.ActionID, ClientInputSender.SkillTriggerStyle.Keyboard);
            }
        }

        void OnSkill3Canceled(InputAction.CallbackContext context)
        {
            if (!IsActive || m_ServerCharacter == null || m_ServerCharacter.CharacterClass == null) return;
            
            if (GameDataSource.Instance.TryGetActionPrototypeByID(m_ServerCharacter.CharacterClass.Skill3.ActionID, out var skill3))
            {
                RequestAction(skill3.ActionID, ClientInputSender.SkillTriggerStyle.KeyboardRelease);
            }
        }

        void OnEmote1Performed(InputAction.CallbackContext context)
        {
            if (!IsActive || m_ServerCharacter == null) return;
            RequestAction(GameDataSource.Instance.Emote1ActionPrototype.ActionID, ClientInputSender.SkillTriggerStyle.Keyboard);
        }

        void OnEmote2Performed(InputAction.CallbackContext context)
        {
            if (!IsActive || m_ServerCharacter == null) return;
            RequestAction(GameDataSource.Instance.Emote2ActionPrototype.ActionID, ClientInputSender.SkillTriggerStyle.Keyboard);
        }

        void OnEmote3Performed(InputAction.CallbackContext context)
        {
            if (!IsActive || m_ServerCharacter == null) return;
            RequestAction(GameDataSource.Instance.Emote3ActionPrototype.ActionID, ClientInputSender.SkillTriggerStyle.Keyboard);
        }

        void OnEmote4Performed(InputAction.CallbackContext context)
        {
            if (!IsActive || m_ServerCharacter == null) return;
            RequestAction(GameDataSource.Instance.Emote4ActionPrototype.ActionID, ClientInputSender.SkillTriggerStyle.Keyboard);
        }

        void OnTargetStarted(InputAction.CallbackContext context)
        {
            if (!IsActive || m_ServerCharacter == null) return;
            RequestAction(GameDataSource.Instance.GeneralTargetActionPrototype.ActionID, ClientInputSender.SkillTriggerStyle.MouseClick);
        }

        #endregion

        /// <summary>
        /// Request an action to be performed
        /// </summary>
        void RequestAction(ActionID actionID, ClientInputSender.SkillTriggerStyle triggerStyle, ulong targetId = 0)
        {
            var requestData = new ActionRequestData
            {
                ActionID = actionID,
                TargetIds = targetId != 0 ? new ulong[] { targetId } : Array.Empty<ulong>()
            };

            ActionInputEvent?.Invoke(requestData);

            if (m_ServerCharacter != null && m_ServerCharacter.IsSpawned)
            {
                m_ServerCharacter.ServerPlayActionRpc(requestData);
            }
        }

        /// <summary>
        /// Set whether this input manager is actively sending input
        /// </summary>
        public void SetActive(bool active)
        {
            IsActive = active;
            if (active)
            {
                EnableAllActions();
            }
            else
            {
                DisableAllActions();
            }
        }
    }
}
