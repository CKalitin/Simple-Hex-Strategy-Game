using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private float defaultZoom;

    #endregion

    #region Core

    private void Awake() {
        defaultZoom = currentZoom;
    }

    private void Start() {
        if (MatchManager.instance.MatchState == MatchState.Lobby) {
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
        GameController.OnGameInitialized += OnGameInitialized;
    }

    private void OnDisable() {
        MatchManager.OnMatchStateChanged -= OnMatchStateChanged;
        GameController.OnGameInitialized -= OnGameInitialized;
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
        currentZoom = defaultZoom;
        lobbyLerpProgress = 0f;

        cam.transform.position = Vector3.Lerp(lobbyCameraPosition.position, Vector3.Lerp(defaultCamPosition.position, transform.position, currentZoom), lobbyLerpProgress);
        cam.transform.rotation = Quaternion.Lerp(lobbyCameraPosition.rotation, defaultCamPosition.rotation, lobbyLerpProgress);
    }

    #endregion

    #region Callbacks

    private void OnMatchStateChanged(MatchState _matchState) {
        if (_matchState == MatchState.InGame) {
            lerpToGameCamPos = true;
        }
        if (_matchState == MatchState.Lobby) {
            ResetCamera();
            cam.transform.position = lobbyCameraPosition.position;
            cam.transform.rotation = lobbyCameraPosition.rotation;
        }
    }

    private void OnGameInitialized() {
        Vector3 pos = GetPositionOfPlayerBase();
        if (pos == Vector3.zero) return;

        transform.position = new Vector3(pos.x, transform.position.y, pos.z);
        lobbyCameraPosition.position = new Vector3(pos.x, lobbyCameraPosition.position.y, pos.z - 5);
        
        cam.transform.localPosition = new Vector3(lobbyCameraPosition.position.x - transform.position.x, lobbyCameraPosition.position.y - transform.position.y, lobbyCameraPosition.position.z - transform.position.z);
        cam.transform.rotation = lobbyCameraPosition.rotation;
    }

    private Vector3 GetPositionOfPlayerBase() {
        Vector3 position = Vector3.zero;

        Dictionary<Vector2Int, TileInfo> tiles = TileManagement.instance.GetTiles;
        for (int i = 0; i < tiles.Count; i++) {
            if (tiles.ElementAt(i).Value.Tile.Structures.Count <= 0) continue;
            if (!tiles.ElementAt(i).Value.Tile.Structures[0].GetComponent<ClientBase>()) continue;
            if (tiles.ElementAt(i).Value.Tile.Structures[0].PlayerID == MatchManager.instance.PlayerID){
                position = tiles.ElementAt(i).Value.Tile.transform.position;
                break;
            }
        }

        return position;
    }

    #endregion
}
