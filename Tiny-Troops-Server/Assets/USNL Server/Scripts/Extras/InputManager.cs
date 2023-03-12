using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USNL {
    public struct ClientInput {
        private int clientId;

        private List<KeyCode> keycodesDown;
        private List<KeyCode> keycodesUp;
        private List<KeyCode> keycodesPressed;

        private Dictionary<string, KeyCode> buttonNameToKeyCode;

        public int ClientId { get => clientId; set => clientId = value; }
        public List<KeyCode> KeycodesDown { get => keycodesDown; set => keycodesDown = value; }
        public List<KeyCode> KeycodesUp { get => keycodesUp; set => keycodesUp = value; }
        public List<KeyCode> KeycodesPressed { get => keycodesPressed; set => keycodesPressed = value; }

        public ClientInput(int clientId) {
            this.clientId = clientId;

            this.keycodesDown = new List<KeyCode>();
            this.keycodesUp = new List<KeyCode>();
            this.keycodesPressed = new List<KeyCode>();

            this.buttonNameToKeyCode = new Dictionary<string, KeyCode>() {
            { "Mouse0", (KeyCode)323},
            { "Mouse1", (KeyCode)324},
            { "Mouse2", (KeyCode)325},
            { "Mouse3", (KeyCode)326},
            { "Mouse4", (KeyCode)327},
            { "Mouse5", (KeyCode)328},
            { "Mouse6", (KeyCode)329},
        };
        }

        #region Get Functions

        public bool GetKeyDown(KeyCode _keyCode) {
            if (keycodesDown.Contains(_keyCode)) {
                return true;
            }
            return false;
        }

        public bool GetKeyUp(KeyCode _keyCode) {
            if (keycodesUp.Contains(_keyCode)) {
                return true;
            }
            return false;
        }

        public bool GetKey(KeyCode _keyCode) {
            if (keycodesPressed.Contains(_keyCode)) {
                return true;
            }
            return false;
        }

        public bool GetMouseButtonDown(string _buttonName) {
            if (keycodesDown.Contains(buttonNameToKeyCode[_buttonName])) {
                return true;
            }
            return false;
        }

        public bool GetMouseButtonUp(string _buttonName) {
            if (keycodesUp.Contains(buttonNameToKeyCode[_buttonName])) {
                return true;
            }
            return false;
        }

        public bool GetMouseButton(string _buttonName) {
            if (keycodesPressed.Contains(buttonNameToKeyCode[_buttonName])) {
                return true;
            }
            return false;
        }

        #endregion
    }

    public class InputManager : MonoBehaviour {
        #region Variables

        public static InputManager instance;

        private ClientInput[] clientInputs = null;
        private List<ClientInput> modifiedClientInputs = new List<ClientInput>();

        // This is a list because a packet may be received after another script has gone through update, because keycodes are reset in LateUpdate a keycode may be received but the user may never see it
        private List<USNL.Package.ClientInputPacket> receivedClientInputPackets = new List<USNL.Package.ClientInputPacket>();

        public ClientInput[] ClientInputs { get => clientInputs; set => clientInputs = value; }

        #endregion

        #region Core

        private void Awake() {
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Debug.Log("Input Manager instance already exists, destroying object.");
                Destroy(this);
            }
        }

        private void LateUpdate() {
            // Reset Keycodes Down and Up
            for (int i = 0; i < modifiedClientInputs.Count; i++) {
                modifiedClientInputs[i].KeycodesDown.Clear();
                modifiedClientInputs[i].KeycodesUp.Clear();
            }
            modifiedClientInputs.Clear();

            // Handle Client Input Packets so data is ready for the next update, this can at worst delay input by a frame
            HandleClientInputPackets();
        }

        private void OnEnable() {
            USNL.CallbackEvents.OnClientInputPacket += OnClientInputPacket;
            USNL.CallbackEvents.OnClientDisconnected += OnClientDisconnected;
        }

        private void OnDisable() {
            USNL.CallbackEvents.OnClientInputPacket -= OnClientInputPacket;
            USNL.CallbackEvents.OnClientDisconnected -= OnClientDisconnected;
        }

        public void Initialize() {
            clientInputs = new ClientInput[Package.Server.MaxClients];
            for (int i = 0; i < Package.Server.MaxClients; i++) {
                clientInputs[i] = new ClientInput(i);
            }
        }

        #endregion

        #region Input Manager

        private void HandleClientInputPackets() {
            for (int i = 0; i < receivedClientInputPackets.Count; i++) {
                modifiedClientInputs.Add(clientInputs[receivedClientInputPackets[i].FromClient]);

                for (int x = 0; x < receivedClientInputPackets[i].KeycodesDown.Length; x++) {
                    clientInputs[receivedClientInputPackets[i].FromClient].KeycodesDown.Add((KeyCode)receivedClientInputPackets[i].KeycodesDown[x]);
                    clientInputs[receivedClientInputPackets[i].FromClient].KeycodesPressed.Add((KeyCode)receivedClientInputPackets[i].KeycodesDown[x]);
                }

                for (int x = 0; x < receivedClientInputPackets[i].KeycodesUp.Length; x++) {
                    clientInputs[receivedClientInputPackets[i].FromClient].KeycodesUp.Add((KeyCode)receivedClientInputPackets[i].KeycodesUp[x]);
                    clientInputs[receivedClientInputPackets[i].FromClient].KeycodesPressed.Remove((KeyCode)receivedClientInputPackets[i].KeycodesUp[x]);
                }
            }
            receivedClientInputPackets.Clear();
        }

        private void OnClientInputPacket(object _packetObject) {
            receivedClientInputPackets.Add((USNL.Package.ClientInputPacket)_packetObject);
        }

        private void OnClientDisconnected(object _clientIdObject) {
            int _clientId = (int)_clientIdObject;

            clientInputs[_clientId].KeycodesDown.Clear();
            clientInputs[_clientId].KeycodesUp.Clear();
            clientInputs[_clientId].KeycodesPressed.Clear();
        }

        public ClientInput GetClientInput(int _clientId) {
            return clientInputs[_clientId];
        }

        #endregion
    }
}