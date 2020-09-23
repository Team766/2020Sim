using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkManager))]
public class StartNetwork : MonoBehaviour {
    void Start() {
        if (!NetworkClient.active) {
            if (Application.platform == RuntimePlatform.WebGLPlayer) {
                var hostname = new Uri(Application.absoluteURL).Host;
                var manager = GetComponent<NetworkManager>();
                Debug.Log("Connecting to " + hostname);
                manager.networkAddress = hostname;
                manager.StartClient();
            }
        }
    }
}
