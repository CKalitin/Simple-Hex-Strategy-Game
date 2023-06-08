using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuManager : MonoBehaviour {
    [SerializeField] private GameObject setingsMenuParent;
    [Space]
    [SerializeField] private GameObject defaultMenuParent;
    [SerializeField] private GameObject titleParent;
    [Space]
    [SerializeField] private CanvasManager canvasManager;

    private void Start() {
        Close();
    }

    public bool IsActive() {
        if (setingsMenuParent.activeSelf == true) return true;
        if (defaultMenuParent.activeSelf == true) return true;
        if (titleParent.activeSelf == true) return true;
        return false;
    }

    public void Open() {
        BackButton();
    }

    public void Close() {
        ToggleParents(false, false, false);
        if (MatchManager.instance.MatchState == MatchState.Lobby) canvasManager.TogglePrimaryCanvases(false, true, false);
        else canvasManager.TogglePrimaryCanvases(true, false, false);
    }

    public void BackButton() {
        ToggleParents(false, true, true);
    }

    public void PlayMenuButton() {
        ToggleParents(false, false, true);
    }

    public void SettingsMenuButton() {
        ToggleParents(true, false, true);
    }

    public void ToggleParents(bool settingsMenuToggle, bool defaultMenuToggle, bool titleToggle) {
        setingsMenuParent.SetActive(settingsMenuToggle);
        defaultMenuParent.SetActive(defaultMenuToggle);
        titleParent.SetActive(titleToggle);
    }
}
