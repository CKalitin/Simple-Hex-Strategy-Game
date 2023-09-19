 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateInDirectionOfMovement : MonoBehaviour {
    private Vector2 previousPosition;

    private Quaternion targetRotation;

    private void LateUpdate() {
        if (Mathf.Abs(previousPosition.x - transform.position.x) > 0.01f || Mathf.Abs(previousPosition.y - transform.position.z) > 0.01f) {
            Vector2 direction = new Vector2(transform.position.x, transform.position.z) - previousPosition;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y), Vector3.up);

            previousPosition = new Vector2(transform.position.x, transform.position.z);
        }
        transform.rotation = targetRotation;
    }
}
