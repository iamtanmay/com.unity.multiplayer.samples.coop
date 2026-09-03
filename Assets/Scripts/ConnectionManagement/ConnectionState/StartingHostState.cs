using System;
using Unity.BossRoom.Infrastructure;
using Unity.BossRoom.UnityServices.Sessions;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Unity.BossRoom.ConnectionManagement
{
    /// <summary>
    /// Connection state corresponding to a host starting up. Starts the host when entering the state. If successful,
    /// transitions to the Hosting state, if not, transitions back to the Offline state.
    /// </summary>
    class StartingHostState : OnlineState
    {
        [Inject]
        MultiplayerServicesFacade m_MultiplayerServicesFacade;
        [Inject]
        LocalSession m_LocalSession;
        ConnectionMethodBase m_ConnectionMethod;

        public StartingHostState Configure(ConnectionMethodBase baseConnectionMethod)
        {
            m_ConnectionMethod = baseConnectionMethod;
            return this;
        }

        public override void Enter()
        {
            StartHost();
        }

        public override void Exit() { }

        public override void OnServerStarted()
        {
            Debug.Log("[StartingHostState] Server started. Transitioning to Lobby/CharSelect logic.");
            
            // CRITICAL FIX FOR SINGLE PLAYER HOST:
            // The server must load the scene and seat itself, as it won't receive the "ClientLoadedScene" callback.
            
            m_ConnectStatusPublisher.Publish(ConnectStatus.Success);
            
            // 1. Load the Character Select scene for the server immediately
            var networkManager = m_ConnectionManager.NetworkManager;
            if (networkManager != null)
            {
                Debug.Log("[StartingHostState] Loading CharSelect scene for host...");
                networkManager.SceneManager.LoadScene("CharSelect", UnityEngine.SceneManagement.LoadSceneMode.Single);
                
                // 2. Seat the host player immediately so they can select a character
                // We delay this slightly to ensure the scene has started loading
                m_ConnectionManager.StartCoroutine(SeatHostAfterDelay());
            }
            
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Hosting);
        }

        private System.Collections.IEnumerator SeatHostAfterDelay()
        {
            // Wait 1 frame to ensure scene context is ready
            yield return null; 
            
            // Find the ServerCharSelectState in the loaded scene
            var charSelectState = FindObjectOfType<ServerCharSelectState>();
            if (charSelectState != null)
            {
                Debug.Log("[StartingHostState] Manually seating host player.");
                charSelectState.SeatHostPlayer();
                yield break;
            }
            
            // Try again in case state hasn't updated yet
            yield return new WaitForSeconds(0.5f);
            
            charSelectState = FindObjectOfType<ServerCharSelectState>();
            if (charSelectState != null)
            {
                Debug.Log("[StartingHostState] Manually seating host player (delayed).");
                charSelectState.SeatHostPlayer();
                yield break;
            }
            
            Debug.LogWarning("[StartingHostState] Could not find ServerCharSelectState to seat host.");
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;

            // This happens when starting as a host, before the end of the StartHost call. In that case, we simply approve ourselves.
            if (clientId == m_ConnectionManager.NetworkManager.LocalClientId)
            {
                var payload = System.Text.Encoding.UTF8.GetString(connectionData);
                var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload); // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html

                SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId, connectionPayload.playerId,
                    new SessionPlayerData(clientId, connectionPayload.playerName, new NetworkGuid(), 0, true));

                // connection approval will create a player object for you
                response.Approved = true;
                response.CreatePlayerObject = true;
            }
        }

        public override void OnServerStopped()
        {
            StartHostFailed();
        }

        void StartHost()
        {
            try
            {
                m_ConnectionMethod.SetupHostConnection();

                // NGO's StartHost launches everything
                if (!m_ConnectionManager.NetworkManager.StartHost())
                {
                    StartHostFailed();
                }
            }
            catch (Exception)
            {
                StartHostFailed();
                throw;
            }
        }

        void StartHostFailed()
        {
            m_ConnectStatusPublisher.Publish(ConnectStatus.StartHostFailed);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }
    }
}
