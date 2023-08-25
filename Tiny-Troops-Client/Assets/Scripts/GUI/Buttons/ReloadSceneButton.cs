using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadSceneButton : MonoBehaviour {
    public void ReloadScene() {
        SceneLoader.instance.ReloadScene();
    }
}
