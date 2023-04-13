using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverMenu : MonoBehaviour {
    [SerializeField] private GameObject menuParent;
    [Space]
    [SerializeField] private float openDelay;

    Coroutine openMenuCoroutine;

    public void OnPointerEnter() {
        openMenuCoroutine = StartCoroutine(OpenMenu());
    }

    public void OnPointerExit() {
        menuParent.SetActive(false);
        if (openMenuCoroutine != null) StopCoroutine(openMenuCoroutine);
    }

    private void OnDisable() {
        menuParent.SetActive(false);
        if (openMenuCoroutine != null) StopCoroutine(openMenuCoroutine);
    }

    private IEnumerator OpenMenu() {
        yield return new WaitForSecondsRealtime(openDelay);
        menuParent.SetActive(true);
        openMenuCoroutine = null;
    }
}
