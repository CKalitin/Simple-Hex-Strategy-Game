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

    private long previousId;

    private void Awake() {
        if (PlayerPrefs.HasKey("HostId1") && PlayerPrefs.HasKey("HostId2")) {
            previousId = doubleInt2long(PlayerPrefs.GetInt("HostId1"), PlayerPrefs.GetInt("HostId2"));
            hostIdField.text = previousId.ToString();
        }

        invalidIdIndicator.SetActive(false);
    }

    private void Update() {
        UpdateIdAndCheckInvalid();
    }

    private void UpdateIdAndCheckInvalid() {
        invalidIdIndicator.SetActive(false);

        try {
            long id = hostIdField.text.Length > 0 ? long.Parse(hostIdField.text) : 0;

            if (id == previousId) return;

            previousId = id;

            if (hostIdField.text.Length <= 0) { return; }

            if (!ValidateIPv4(IDtoIP(id))) invalidIdIndicator.SetActive(true);
            else { 
                PlayerPrefs.SetInt("HostId1", long2doubleInt(id)[0]);
                PlayerPrefs.SetInt("HostId2", long2doubleInt(id)[1]);
            }
        } catch (Exception _ex) {
            Debug.Log("ID Input Field Error: " + _ex);
            invalidIdIndicator.SetActive(true);
        }
    }

    private void UpdateId() {
        invalidIdIndicator.SetActive(false);

        try {
            long id = hostIdField.text.Length > 0 ? long.Parse(hostIdField.text) : 0;

            if (id != previousId) {
                previousId = id;

                PlayerPrefs.SetInt("HostId1", long2doubleInt(id)[0]);
            }   PlayerPrefs.SetInt("HostId2", long2doubleInt(id)[1]);
        } catch (Exception _ex) {
            Debug.Log("ID Input Field Error: " + _ex);
            invalidIdIndicator.SetActive(true);
        }
    }

    public static bool ValidateIPv4(string ipString) {
        if (ipString.Count(c => c == '.') != 3) return false;
        IPAddress address;
        return IPAddress.TryParse(ipString, out address);
    }

    // https://stackoverflow.com/questions/6219614/convert-a-long-to-two-int-for-the-purpose-of-reconstruction
    public static int[] long2doubleInt(long a) {
        int a1 = (int)(a & uint.MaxValue);
        int a2 = (int)(a >> 32);
        return new int[] { a1, a2 };
    }

    public static long doubleInt2long(int a1, int a2) {
        long b = a2;
        b = b << 32;
        b = b | (uint)a1;
        return b;
    }
    
    public string IDtoIP(long _id) {
        // https://support.sumologic.com/hc/en-us/community/posts/5076590459927-convert-decimal-value-to-IP-address
        long[] ipOctets = new long[4];
        ipOctets[0] = (long)((_id / 16777216) % 256);
        ipOctets[1] = (long)((_id / 65536) % 256);
        ipOctets[2] = (long)((_id / 256) % 256);
        ipOctets[3] = (long)((_id / 1) % 256);

        return ipOctets[0] + "." + ipOctets[1] + "." + ipOctets[2] + "." + ipOctets[3];
    }

}