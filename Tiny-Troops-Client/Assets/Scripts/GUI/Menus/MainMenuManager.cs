using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour {
    [SerializeField] private GameObject playMenuParent;
    [SerializeField] private GameObject setingsMenuParent;
    [SerializeField] private GameObject aboutMenuParent;
    [Space]
    [SerializeField] private GameObject defaultMenuParent;
    [SerializeField] private GameObject titleParent;

    private void Start() {
        BackButton();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            BackButton();
        }
    }

    public void BackButton() {
        ToggleParents(false, false, false, true, true);
    }

    public void PlayMenuButton() {
        ToggleParents(true, false, false, false, true);
    }

    public void SettingsMenuButton() {
        ToggleParents(false, true, false, false, true);
    }

    public void AboutMenuButton() {
        ToggleParents(false, false, true, false, true);
    }

    public void ToggleParents(bool playMenuToggle, bool settingsMenuToggle, bool aboutMenuToggle, bool defaultMenuToggle, bool titleToggle) {
        playMenuParent.SetActive(playMenuToggle);
        setingsMenuParent.SetActive(settingsMenuToggle);
        aboutMenuParent.SetActive(aboutMenuToggle);
        defaultMenuParent.SetActive(defaultMenuToggle);
        titleParent.SetActive(titleToggle);
    }
}
