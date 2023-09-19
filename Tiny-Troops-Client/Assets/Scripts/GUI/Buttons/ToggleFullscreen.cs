using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToggleFullscreen : MonoBehaviour {
    public static ToggleFullscreen instance;

    private TMP_Dropdown dropdown;

    public TMP_Dropdown Dropdown { get => dropdown; set => dropdown = value; }

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.Log($"Toggle Fullscreen instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }

        dropdown = GetComponent<TMP_Dropdown>();

        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow) dropdown.value = 0;
        else if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen) dropdown.value = 1;
        else if (Screen.fullScreenMode == FullScreenMode.Windowed) dropdown.value = 2;
    }

    public void ToggleFullScreen() {
        switch (dropdown.value) {
            case 0:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }
    }
}
