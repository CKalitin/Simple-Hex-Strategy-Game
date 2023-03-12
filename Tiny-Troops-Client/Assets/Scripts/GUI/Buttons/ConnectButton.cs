using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectButton : MonoBehaviour {
    [Tooltip("This should be identical to what is shown in the Game Scene.")]
    [SerializeField] private GameObject connectingScreen;

    public void Connect() {
        connectingScreen.SetActive(true);
        SceneLoader.instance.LoadGameScene();
    }
}
