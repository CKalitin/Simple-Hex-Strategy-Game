using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetGameButton : MonoBehaviour {
    public void ResetGame() {
        GameController.instance.ResetGame();
        CameraController.instance.ResetCamera();
    }
}
