using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateCanvasesButton : MonoBehaviour {
    [SerializeField] private bool gameCanvas;
    [SerializeField] private bool lobbyCanvas;
    [SerializeField] private bool gameEndedCanvas;

    public void OnButtonDown() {
        CanvasManager.instance.TogglePrimaryCanvases(gameCanvas, lobbyCanvas, gameEndedCanvas);
    }
}
