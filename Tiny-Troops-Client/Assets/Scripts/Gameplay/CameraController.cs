using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    #region Variables

    [Header("Camera")]
    [SerializeField] private Transform defaultCamPosition;
    [Space]
    [SerializeField] private GameObject cam;

    [Header("Movement")]
    [SerializeField] private float translateSpeed;
    [SerializeField] private float zoomToMovementMultiplier = 10f;
    [Space]
    [SerializeField] private Vector3 maxCamPos;
    [SerializeField] private Vector3 minCamPos;

    [Header("Zoom")]
    [SerializeField] private float minCameraDist = 3f;
    [SerializeField] private float maxCameraDist = 8f;
    [Space]
    [SerializeField] private float currentZoom = 0.5f;
    [SerializeField] private float zoomStep = 0.05f;
    
    [Header("Match States")]
    [SerializeField] private Transform lobbyCameraPosition;
    [SerializeField] private float lobbyToGameLerpSpeed = 1f;

    private float lobbyLerpProgress;

    // If the game state is inGame and the camera should be at the in game camera position
    private bool gameCamera = false;
    private bool lerpToGameCamPos = false;

    #endregion

    #region Core

    private void Start() {
        if (MatchManager.instance.MatchState == MatchState.InGame | MatchManager.instance.MatchState == MatchState.Paused)
            gameCamera = true;
        else if (MatchManager.instance.MatchState == MatchState.Lobby) {
            cam.transform.position = lobbyCameraPosition.position;
            cam.transform.rotation = lobbyCameraPosition.rotation;
        }
    }

    private void Update() {
        if (gameCamera) {
            Zoom();
            Move();
        }

        if (lerpToGameCamPos) {
            lobbyLerpProgress += Mathf.Clamp(lobbyToGameLerpSpeed * Time.deltaTime, 0, 1);

            cam.transform.position = Vector3.Lerp(lobbyCameraPosition.position, Vector3.Lerp(defaultCamPosition.position, transform.position, currentZoom), lobbyLerpProgress);
            cam.transform.rotation = Quaternion.Lerp(lobbyCameraPosition.rotation, defaultCamPosition.rotation, lobbyLerpProgress);

            if (lobbyLerpProgress >= 1) {
                lobbyLerpProgress = 0;
                lerpToGameCamPos = false;
                gameCamera = true;
            }
        }
    }

    private void OnEnable() {
        MatchManager.OnMatchStateChanged += OnMatchStateChanged;
    }

    private void OnDisable() {
        MatchManager.OnMatchStateChanged -= OnMatchStateChanged;
    }

    #endregion

    #region Camera Movement

    private void Move() {
        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 targetPos = transform.position + movement * Time.deltaTime * translateSpeed * (Mathf.Abs(1 - currentZoom) * zoomToMovementMultiplier);
        targetPos = new Vector3(Mathf.Clamp(targetPos.x, minCamPos.x, maxCamPos.x), transform.position.y, Mathf.Clamp(targetPos.z, minCamPos.z, maxCamPos.z));
        transform.position = targetPos;

    }

    private void Zoom() {
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");

        float previousCurrentZoom = currentZoom;
        if (scroll > 0) currentZoom = Mathf.Clamp(currentZoom + zoomStep, minCameraDist / maxCameraDist, 1);
        if (scroll < 0) currentZoom = Mathf.Clamp(currentZoom - zoomStep, minCameraDist / maxCameraDist, 1);

        Vector3 targetPos = Vector3.Lerp(defaultCamPosition.position, transform.position, currentZoom);

        if (Vector3.Distance(transform.position, targetPos) <= maxCameraDist && Vector3.Distance(transform.position, targetPos) >= minCameraDist) {
            cam.transform.position = targetPos;
        } else {
            currentZoom = previousCurrentZoom;
        }
    }

    #endregion

    #region Management Functions

    public void LerpLobbyToGameCamPos() {
        lerpToGameCamPos = true;
    }

    public void ResetCamera() {
        gameCamera = false;
        lerpToGameCamPos = false;

        cam.transform.position = lobbyCameraPosition.position;
    }

    #endregion

    #region Callbacks

    private void OnMatchStateChanged(MatchState _matchState) {
        if (_matchState == MatchState.InGame) {
            lerpToGameCamPos = true;
        }
        if (_matchState == MatchState.Lobby) {
            gameCamera = false;
        }
    }

    #endregion
}
