using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USNL {
    public class SyncedObject : MonoBehaviour {
        #region Variables

        [Header("Synced Object")]
        [Tooltip("This is used to spawn this Synced Object on the Client")]
        [SerializeField] private string syncedObjectTag;
        [Space]
        [Tooltip("If you want a Synced Object to Update it's position at it's own rate (Not the one set in Synced Object Manager).\nGame restart needed to take effect.")]
        [SerializeField] private bool useLocalChangeValues = false;
        [Tooltip("Minimum ammount a Synced Object needs to move before it is updated on the Clients.")]
        [SerializeField] private float minPosChange = 0.001f;
        [Tooltip("Minimum ammount a Synced Object needs to rotate before it is updated on the Clients.")]
        [SerializeField] private float minRotChange = 0.001f;
        [Tooltip("Minimum ammount a Synced Object needs to change in scale before it is updated on the Clients.")]
        [SerializeField] private float minScaleChange = 0.001f;

        [Header("Interpolation Toggles")]
        [SerializeField] private bool interpolatePosition = true;
        [SerializeField] private bool interpolateRotation = false;
        [SerializeField] private bool interpolateScale = true;

        private int syncedObjectUUID;

        // If value has not changed since last Synced Object Update
        private bool positionStable = false;
        private bool rotationStable = false;
        private bool scaleStable = false;

        // Rate of change per second values
        private Vector3 positionInterpolation;
        private Vector3 rotationInterpolation;
        private Vector3 scaleInterpolation;

        // Previous frames difference in position, rotation, scale
        private List<Vector3> prevPositionsDiffs = new List<Vector3>();
        private List<Vector3> prevRotationsDiffs = new List<Vector3>();
        private List<Vector3> prevScalesDiffs = new List<Vector3>();

        // Previous positions, last fixed update and last synced object update
        private Vector3 prevUpdatePos;
        private Vector3 prevUpdateRot;
        private Vector3 prevUpdateScale;

        private Vector3 prevSOUpdatePos;
        private Vector3 prevSOUpdateRot;
        private Vector3 prevSOUpdateScale;

        public string SyncedObjectTag { get => syncedObjectTag; set => syncedObjectTag = value; }
        public int SyncedObjectUUID { get => syncedObjectUUID; set => syncedObjectUUID = value; }

        public Vector3 PositionInterpolation { get => positionInterpolation; set => positionInterpolation = value; }
        public Vector3 RotationInterpolation { get => rotationInterpolation; set => rotationInterpolation = value; }
        public Vector3 ScaleInterpolation { get => scaleInterpolation; set => scaleInterpolation = value; }

        #endregion

        #region Core

        void Awake() {
            syncedObjectUUID = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0); // Generate UUID

            prevUpdatePos = transform.position;
            prevUpdateRot = transform.eulerAngles;
            prevUpdateScale = transform.lossyScale;

            prevSOUpdatePos = transform.position;
            prevSOUpdateRot = transform.eulerAngles;
            prevSOUpdateScale = transform.lossyScale;

            SyncedObjectManager.instance.InstantiateSyncedObject(this);
        }

        private void Start() {
            if (!useLocalChangeValues) {
                minPosChange = SyncedObjectManager.instance.MinPosChange;
                minRotChange = SyncedObjectManager.instance.MinRotChange;
                minScaleChange = SyncedObjectManager.instance.MinScaleChange;
            }

            prevSOUpdatePos = transform.position;
            prevSOUpdateRot = transform.eulerAngles;
            prevSOUpdateScale = transform.lossyScale;
        }

        private void FixedUpdate() {
            SetPreviousDiffValues();
        }

        private void SetPreviousDiffValues() {
            prevPositionsDiffs.Add((transform.position - prevUpdatePos) / Time.deltaTime);
            prevRotationsDiffs.Add((transform.eulerAngles - prevUpdateRot) / Time.deltaTime);
            prevScalesDiffs.Add((transform.lossyScale - prevUpdateScale) / Time.deltaTime);

            prevUpdatePos = transform.position;
            prevUpdateRot = transform.eulerAngles;
            prevUpdateScale = transform.lossyScale;
        }

        // This is called from Synced Object Manager
        public void UpdateSyncedObject() {
            if (SyncedObjectManager.instance.Vector2Mode) {
                Vector2SyncedObjectUpdate();
            } else {
                Vector3SyncedObjectUpdate();
            }

            prevSOUpdatePos = transform.position;
            prevSOUpdateRot = transform.eulerAngles;
            prevSOUpdateScale = transform.lossyScale;
        }

        public void UpdateInterpolation() {
            SetInterpolationValues();
        }

        private void SetInterpolationValues() {
            if (prevPositionsDiffs.Count > 0) {
                if (interpolatePosition & SyncedObjectManager.instance.InterpolatePosition & !positionStable) positionInterpolation = GetMedian(prevPositionsDiffs);
                else positionInterpolation = Vector3.zero;

                if (interpolateRotation & SyncedObjectManager.instance.InterpolateRotation & !rotationStable) rotationInterpolation = GetMedian(prevRotationsDiffs);
                else rotationInterpolation = Vector3.zero;

                if (interpolateScale & SyncedObjectManager.instance.InterpolateScale & !scaleStable) scaleInterpolation = GetMedian(prevScalesDiffs);
                else scaleInterpolation = Vector3.zero;
            } else {
                positionInterpolation = Vector3.zero;
                rotationInterpolation = Vector3.zero;
                ScaleInterpolation = Vector3.zero;
            }

            prevPositionsDiffs.Clear();
            prevRotationsDiffs.Clear();
            prevScalesDiffs.Clear();
        }

        private void OnDestroy() {
            SyncedObjectManager.instance.DestroySyncedObject(this);
        }

        #endregion

        #region Send Synced Object Updates

        private void Vector2SyncedObjectUpdate() {
            if (CheckValuesAgainstMinAbs(minPosChange, transform.position.x - prevSOUpdatePos.x, transform.position.y - prevSOUpdatePos.y)) {
                positionStable = false;
                SyncedObjectManager.instance.soVec2PosUpdate.Add(this);
                if (!SyncedObjectManager.instance.soVec2PosInterpolation.Contains(this))
                    SyncedObjectManager.instance.soVec2PosInterpolation.Add(this);
            } else {
                CheckValueStable(ref positionStable, ref SyncedObjectManager.instance.soVec2PosUpdate, ref SyncedObjectManager.instance.soVec2PosInterpolation);
            }

            if (CheckValuesAgainstMinAbs(minRotChange, transform.eulerAngles.z - prevSOUpdateRot.z)) {
                rotationStable = false;
                SyncedObjectManager.instance.soRotZUpdate.Add(this);
                if (!SyncedObjectManager.instance.soRotZInterpolation.Contains(this))
                    SyncedObjectManager.instance.soRotZInterpolation.Add(this);
            } else {
                CheckValueStable(ref rotationStable, ref SyncedObjectManager.instance.soRotZUpdate, ref SyncedObjectManager.instance.soRotZInterpolation);
            }


            if (CheckValuesAgainstMinAbs(minScaleChange, transform.lossyScale.x - prevSOUpdateScale.x, transform.lossyScale.y - prevSOUpdateScale.y)) {
                scaleStable = false;
                SyncedObjectManager.instance.soVec2ScaleUpdate.Add(this);
                if (!SyncedObjectManager.instance.soVec2ScaleInterpolation.Contains(this))
                    SyncedObjectManager.instance.soVec2ScaleInterpolation.Add(this);
            } else {
                CheckValueStable(ref scaleStable, ref SyncedObjectManager.instance.soVec2ScaleUpdate, ref SyncedObjectManager.instance.soVec2ScaleInterpolation);
            }
        }

        private void Vector3SyncedObjectUpdate() {
            if (CheckValuesAgainstMinAbs(minPosChange, transform.position.x - prevSOUpdatePos.x, transform.position.y - prevSOUpdatePos.y, transform.position.z - prevSOUpdatePos.z)) {
                positionStable = false;
                SyncedObjectManager.instance.soVec3PosUpdate.Add(this);
                if (!SyncedObjectManager.instance.soVec3PosInterpolation.Contains(this))
                    SyncedObjectManager.instance.soVec3PosInterpolation.Add(this);
            } else {
                CheckValueStable(ref positionStable, ref SyncedObjectManager.instance.soVec3PosUpdate, ref SyncedObjectManager.instance.soVec3PosInterpolation);
            }

            if (CheckValuesAgainstMinAbs(minRotChange, transform.eulerAngles.x - prevSOUpdateRot.x, transform.eulerAngles.y - prevSOUpdateRot.y, transform.eulerAngles.z - prevSOUpdateRot.z)) {
                rotationStable = false;
                SyncedObjectManager.instance.soRotUpdate.Add(this);
                if (!SyncedObjectManager.instance.soRotInterpolation.Contains(this))
                    SyncedObjectManager.instance.soRotInterpolation.Add(this);
            } else {
                CheckValueStable(ref rotationStable, ref SyncedObjectManager.instance.soRotUpdate, ref SyncedObjectManager.instance.soRotInterpolation);
            }

            if (CheckValuesAgainstMinAbs(minScaleChange, transform.lossyScale.x - prevSOUpdateScale.x, transform.lossyScale.y - prevSOUpdateScale.y, transform.lossyScale.z - prevSOUpdateScale.z)) {
                scaleStable = false;
                SyncedObjectManager.instance.soVec3ScaleUpdate.Add(this);
                if (!SyncedObjectManager.instance.soVec3ScaleInterpolation.Contains(this))
                    SyncedObjectManager.instance.soVec3ScaleInterpolation.Add(this);
            } else {
                CheckValueStable(ref scaleStable, ref SyncedObjectManager.instance.soVec3ScaleUpdate, ref SyncedObjectManager.instance.soVec3ScaleInterpolation);
            }
        }

        #endregion

        #region Util Functions

        // https://stackoverflow.com/questions/4140719/calculate-median-in-c-sharp
        private float GetMedian(List<float> _values) {
            // Convert to array and sort
            float[] sortedValues = _values.ToArray();
            Array.Sort(sortedValues);

            // Get Median
            int size = sortedValues.Length;
            int mid = size / 2;
            float median = (size % 2 != 0) ? sortedValues[mid] : (sortedValues[mid] + sortedValues[mid - 1]) / 2;
            return median;
        }

        private Vector3 GetMedian(List<Vector3> _values) {
            List<float> x = new List<float>();
            List<float> y = new List<float>();
            List<float> z = new List<float>();

            for (int i = 0; i < _values.Count; i++) {
                x.Add(_values[i].x);
                y.Add(_values[i].y);
                z.Add(_values[i].z);
            }

            return new Vector3(GetMedian(x), GetMedian(y), GetMedian(z));
        }

        private bool CheckValuesAgainstMinAbs(float _min, float _value) {
            if (Mathf.Abs(_value) > _min)
                return true;
            return false;
        }

        private bool CheckValuesAgainstMinAbs(float _min, float _x, float _y) {
            if (Mathf.Abs(_x) > _min | Mathf.Abs(_y) > _min)
                return true;
            return false;
        }

        private bool CheckValuesAgainstMinAbs(float _min, float _x, float _y, float _z) {
            if (Mathf.Abs(_x) > _min | Mathf.Abs(_y) > _min | Mathf.Abs(_z) > _min)
                return true;
            return false;
        }

        // If position hasn't changed, set _stable to true and lock interpolation on client via 2 add calls.
        private void CheckValueStable(ref bool _stable, ref List<SyncedObject> _syncedObjectList, ref List<SyncedObject> _syncedObjectInterpolateList) {
            if (!_stable) {
                // Add value twice so it's sent twice, this way the rate of change (Client-side) is 0 and interpolation won't do anything
                _syncedObjectList.Add(this);
                _syncedObjectList.Add(this);
                if (!_syncedObjectInterpolateList.Contains(this))
                    _syncedObjectInterpolateList.Add(this);
                _stable = true;
            }
        }

        #endregion
    }
}
