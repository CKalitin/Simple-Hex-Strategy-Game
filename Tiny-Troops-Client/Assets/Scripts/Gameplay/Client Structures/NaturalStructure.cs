using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NaturalStructure : MonoBehaviour {
    [SerializeField] private ResourceGain[] resources;

    public ResourceGain[] Resources { get => resources; set => resources = value; }

    [Serializable]
    public struct ResourceGain {
        [SerializeField] private GameResource resource;
        [SerializeField] private int amount;

        public GameResource Resource { get => resource; set => resource = value; }
        public int Amount { get => amount; set => amount = value; }
    }
}
