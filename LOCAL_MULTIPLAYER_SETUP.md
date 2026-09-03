# Local Multiplayer (Couch Co-op) Setup Instructions

## What Was Done

### 1. Added Quick Start Button Support
- Modified `IPHostingUI.cs` to add a "Quick Start" button that bypasses the character selection waiting period
- Added `OnQuickStartClick()` method that immediately starts the game after hosting

### 2. Updated ServerCharSelectState.cs
- Modified `CloseSessionIfReady()` to allow starting when `AllowLocalMultiplayerStart` is true
- The game now starts as soon as at least one player locks in their character (instead of waiting for all players)

### 3. ConnectionManager Already Configured
- `AllowLocalMultiplayerStart` flag is already present and set to `true` by default
- `MaxConnectedPlayers` is set to 8 by default (you can change this to 2 for 2-player co-op)

## What You Need to Do in Unity Editor

### Step 1: Add Quick Start Button to UI
1. Open the `Assets/Prefabs/UI/IPPopup.prefab` prefab
2. Find the IPHostingUI panel
3. Duplicate the existing "Create" button
4. Rename it to "Quick Start" or "Start Game"
5. Position it appropriately on the UI
6. In the Button component's OnClick() event:
   - Drag the IPHostingUI GameObject into the object slot
   - Select `IPHostingUI.OnQuickStartClick()` function

### Step 2: Configure ConnectionManager
1. Find the ConnectionManager GameObject in your scene (likely in MainMenu or Startup scene)
2. Verify these settings:
   - **AllowLocalMultiplayerStart**: ✓ (checked/enabled) - already default
   - **MaxConnectedPlayers**: Set to `2` for 2-player co-op, or keep at `8` for more players

### Step 3: Set Up Local Player Input (Already Done)
The following scripts were already created for local multiplayer input:
- `LocalPlayerInputConfig.cs` - Stores input mappings for each local player
- `LocalMultiplayerInputManager.cs` - Manages multiple local players on same device

You still need to:
1. Create input configuration for Player 2 (e.g., Arrow Keys + Numpad, or Gamepad)
2. Hook up the LocalMultiplayerInputManager to spawn multiple player characters
3. Modify character selection to allow multiple local players to choose characters

### Step 4: Test
1. Click "Host" or "Quick Start" in the IP Hosting UI
2. The game should now start immediately without waiting for another PC to connect
3. For full 2-player local co-op, both players should be able to select characters and control them with different inputs

## Notes

- The "Quick Start" button will host the game and automatically start when you lock in your character
- Without the Quick Start button, the regular "Create" button still works but goes through character selection
- The `AllowLocalMultiplayerStart` flag ensures the game doesn't wait for remote players
