using System;
using System.Collections.Generic;
using UnityEngine;

namespace USNL {
    public class InputManager : MonoBehaviour {
        #region Variables

        public static InputManager instance;

        [Tooltip("Specify whether the Input Manager should detect input on its own.\nIf you set input yourself, it is wise to disable this.")]
        [SerializeField] private bool detectInput = true;

        private List<int> keycodesDown = new List<int>();
        private List<int> keycodesUp = new List<int>();

        // Event.keyDown is really keyPressed, so use this list to convert to keyDown
        private List<int> localKeycodesDown = new List<int>();

        private Dictionary<string, KeyCode> buttonNameToKeyCode = new Dictionary<string, KeyCode>() {
        { "Mouse0", (KeyCode)323},
        { "Mouse1", (KeyCode)324},
        { "Mouse2", (KeyCode)325},
        { "Mouse3", (KeyCode)326},
        { "Mouse4", (KeyCode)327},
        { "Mouse5", (KeyCode)328},
        { "Mouse6", (KeyCode)329},
    };

        public bool DetectInput { get => detectInput; set => detectInput = value; }

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

        private void Update() {
            SendClientInputPacket();
        }

        #endregion

        #region Input

        private void OnGUI() {
            if (!detectInput) { return; }

            if (Event.current.isKey | Event.current.isMouse) {
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode != KeyCode.None) {
                    if (!localKeycodesDown.Contains((int)Event.current.keyCode)) {
                        keycodesDown.Add((int)Event.current.keyCode);
                        localKeycodesDown.Add((int)Event.current.keyCode);
                    }
                } else if (Event.current.type == EventType.KeyUp && Event.current.keyCode != KeyCode.None) {
                    keycodesUp.Add((int)Event.current.keyCode);
                    localKeycodesDown.Remove((int)Event.current.keyCode);
                } else if (Event.current.type == EventType.MouseDown) {
                    // Convert Mouse Button Code to Key Code
                    // <6 because KeyCode only supports 7 Mouse Buttons while Mouse Button Codes supports 8, add 323 to convert to KeyCode
                    if ((int)Event.current.button < 7) {
                        keycodesDown.Add((int)Event.current.button + 323);
                    }
                } else if (Event.current.type == EventType.MouseUp) {
                    // Convert Mouse Button Code to Key Code
                    // <6 because KeyCode only supports 7 Mouse Buttons while Mouse Button Codes supports 8, add 323 to convert to KeyCode
                    if ((int)Event.current.button < 7) {
                        keycodesUp.Add((int)Event.current.button + 323);
                    }
                }
            }
        }

        private void SendClientInputPacket() {
            if (Package.Client.instance.IsConnected && (keycodesDown.Count > 0 || keycodesUp.Count > 0)) {
                USNL.Package.PacketSend.ClientInput(keycodesDown.ToArray(), keycodesUp.ToArray());

                // Clear lists to allow for new values and no duplicates next time this function is run
                keycodesDown.Clear();
                keycodesUp.Clear();
            }
        }

        #endregion

        #region Set Functions

        private void SetKeyDown(KeyCode _keyCode) {
            keycodesDown.Add((int)_keyCode);
        }

        private void SetKeyUp(KeyCode _keyCode) {
            keycodesUp.Add((int)_keyCode);
        }

        private void SetMouseButtonDown(string _buttonName) {
            keycodesDown.Add((int)buttonNameToKeyCode[_buttonName]);
        }

        private void SetMouseButtonUp(string _buttonName) {
            keycodesUp.Add((int)buttonNameToKeyCode[_buttonName]);
        }

        #endregion
    }
}
