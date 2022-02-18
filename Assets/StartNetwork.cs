using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkManager))]
public class StartNetwork : MonoBehaviour {
    void Update() {
        if (!NetworkClient.active) {
            if (Application.platform == RuntimePlatform.WebGLPlayer) {
                var hostname = new Uri(Application.absoluteURL).Host;
                var manager = GetComponent<NetworkManager>();
                var websocketUri = new Uri($"ws://{hostname}:80/ws");
                Debug.Log("Connecting to " + websocketUri);
                manager.StartClient(websocketUri);
            }
        }
    }
}
