using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using TMPro;

public class HostIdField : MonoBehaviour {
    [SerializeField] private TMP_InputField hostIdField;
    [Space]
    [SerializeField] private GameObject invalidIdIndicator;

    private int previousId;

    private void Awake() {
        if (PlayerPrefs.HasKey("HostId")) {
            hostIdField.text = PlayerPrefs.GetInt("HostId").ToString();
            previousId = PlayerPrefs.GetInt("HostId");
        }
        
        invalidIdIndicator.SetActive(false);
    }

    private void Update() {
        UpdateId();
    }

    private void UpdateIdAndCheckInvalid() {
        invalidIdIndicator.SetActive(false);

        try {
            int id = hostIdField.text.Length > 0 ? int.Parse(hostIdField.text) : 0;

            if (id == previousId) {
                previousId = id;
                invalidIdIndicator.SetActive(true);
                return;
            }

            previousId = id;

            if (hostIdField.text.Length <= 0) return;

            if (!ValidateIPv4(IDtoIP(id))) invalidIdIndicator.SetActive(true);
            else PlayerPrefs.SetInt("HostId", id);

            Debug.Log(IDtoIP(id) + ", " + ValidateIPv4(IDtoIP(id)));
        } catch {
            Debug.Log("Error");
            invalidIdIndicator.SetActive(true);
        }
    }

    private void UpdateId() {
        invalidIdIndicator.SetActive(false);

        try {
            int id = hostIdField.text.Length > 0 ? int.Parse(hostIdField.text) : 0;

            if (id != previousId) {
                previousId = id;
                PlayerPrefs.SetInt("HostId", id);
            }
        } catch {
            invalidIdIndicator.SetActive(true);
        }
    }
    
    public string IDtoIP(int _id) {
        // https://support.sumologic.com/hc/en-us/community/posts/5076590459927-convert-decimal-value-to-IP-address
        int[] ipOctets = new int[4];
        ipOctets[0] = (int)((_id / 16777216) % 256);
        ipOctets[1] = (int)((_id / 65536) % 256);
        ipOctets[2] = (int)((_id / 256) % 256);
        ipOctets[3] = (int)((_id / 1) % 256);

        return ipOctets[0] + "." + ipOctets[1] + "." + ipOctets[2] + "." + ipOctets[3];
    }
    
    public static bool ValidateIPv4(string ipString) {
        if (ipString.Count(c => c == '.') != 3) return false;
        IPAddress address;
        return IPAddress.TryParse(ipString, out address);
    }
}
