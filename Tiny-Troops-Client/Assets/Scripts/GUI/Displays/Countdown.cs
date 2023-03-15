using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Countdown : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI countdownText;
    [Space]
    [SerializeField] private string countdownTag = "";
    [Space]
    [SerializeField] private bool isTimer = false;

    private DateTime startTime;
    private DateTime endTime;

    private float timePassed = 0f;

    private bool countdownRunning = false;

    private void Update() {
        timePassed += Time.deltaTime;
        
        DateTime currentTime = startTime.AddSeconds(timePassed);
        
        if (isTimer) countdownText.text = (currentTime - startTime).ToString(@"hh\:mm\:ss");
        else countdownText.text = (endTime - currentTime).ToString(@"hh\:mm\:ss");

        if (currentTime.CompareTo(endTime) > 0) countdownText.text = "";
        else if (countdownRunning == false) countdownText.text = "";
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnCountdownPacket += OnCountdownPacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnCountdownPacket -= OnCountdownPacket;
    }

    private void OnCountdownPacket(object _packetObject) {
        USNL.CountdownPacket packet = (USNL.CountdownPacket)_packetObject;

        if (packet.CountdownTag != countdownTag) return;
        
        if (packet.Duration < 0) {
            startTime = endTime.AddSeconds(5f);
            return;
        }

        startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, packet.StartTimeArray[0], packet.StartTimeArray[1], packet.StartTimeArray[2], packet.StartTimeArray[3]);
        endTime = DateTime.Now.AddSeconds(packet.Duration);
        timePassed = 0f;
        countdownRunning = true;
    }
}
