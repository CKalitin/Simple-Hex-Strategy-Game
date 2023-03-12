using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USNL {
    public class SyncedObject : MonoBehaviour {
        [SerializeField] private bool interpolate = true;

        private int syncedObjectUuid;

        private Vector3 previousUpdatedPosition = new Vector3(-999999, -999999, -999999);
        private Vector3 positionRateOfChange = Vector3.zero; // Per Second
        private float positionUpdateReceivedTime = 0;

        private Vector3 rotationRateOfChange = Vector3.zero; // Per Second

        private Vector3 previousUpdatedScale = new Vector3(-999999, -999999, -999999);
        private Vector3 scaleRateOfChange = Vector3.zero; // Per Second
        private float scaleUpdateReceivedTime = 0;

        public int SyncedObjectUuid { get => syncedObjectUuid; set => syncedObjectUuid = value; }

        private void Update() {
            if (interpolate) {
                transform.position += positionRateOfChange * Time.deltaTime;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(transform.eulerAngles + (rotationRateOfChange * Time.deltaTime)), 99f);
                transform.localScale += scaleRateOfChange * Time.deltaTime;
            }
        }

        #region Local Interpolation

        public void PositionUpdate(Vector3 _updatedPosition) {
            if (SyncedObjectManager.instance.ClientSideInterpolation) {
                if (previousUpdatedPosition != new Vector3(-999999, -999999, -999999)) {
                    float timeBetweenUpdates = Time.realtimeSinceStartup - positionUpdateReceivedTime;

                    if (timeBetweenUpdates == 0) {
                        positionRateOfChange = Vector3.zero;
                    } else {
                        positionRateOfChange = (_updatedPosition - previousUpdatedPosition) / timeBetweenUpdates;
                    }
                }
                previousUpdatedPosition = _updatedPosition;
                positionUpdateReceivedTime = Time.realtimeSinceStartup;
            }
        }

        public void RotationUpdate(Vector3 _updateRotation) {
            // Add local rotation interpolation here if you want to
            /*if (SyncedObjectManager.instance.LocalInterpolation) {

            }*/
        }

        public void ScaleUpdate(Vector3 _updateScale) {
            if (SyncedObjectManager.instance.ClientSideInterpolation) {
                if (previousUpdatedScale != new Vector3(-999999, -999999, -999999)) {
                    float timeBetweenUpdates = Time.realtimeSinceStartup - scaleUpdateReceivedTime;

                    if (timeBetweenUpdates == 0) {
                        scaleRateOfChange = Vector3.zero;
                    } else {
                        scaleRateOfChange = (_updateScale - previousUpdatedScale) / timeBetweenUpdates;
                    }
                }

                previousUpdatedScale = _updateScale;
                scaleUpdateReceivedTime = Time.realtimeSinceStartup;
            }
        }

        #endregion

        #region Server Interpolation

        public void PositionInterpolationUpdate(Vector3 _interpolatePosition) {
            if (!SyncedObjectManager.instance.ClientSideInterpolation) {
                positionRateOfChange = _interpolatePosition;
            }
        }

        public void RotationInterpolationUpdate(Vector3 _interpolateRotation) {
            if (!SyncedObjectManager.instance.ClientSideInterpolation) {
                rotationRateOfChange = _interpolateRotation;
            }
        }

        public void ScaleInterpolationUpdate(Vector3 _interpolateScale) {
            if (!SyncedObjectManager.instance.ClientSideInterpolation) {
                scaleRateOfChange = _interpolateScale;
            }
        }

        #endregion
    }
}