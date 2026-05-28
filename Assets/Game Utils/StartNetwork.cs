using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkManager))]
[RequireComponent(typeof(Mirror.SimpleWeb.SimpleWebTransport))]
public class StartNetwork : MonoBehaviour {
    void Start() {
        var manager = NetworkManager.singleton;
        if (!manager) {
            return;
        }
        if (!NetworkClient.active) {
            if (Application.platform == RuntimePlatform.WebGLPlayer) {
                var uri = new Uri(Application.absoluteURL);
                var websocketUri = new UriBuilder();
                websocketUri.Host = uri.Host;
                websocketUri.Port = uri.Port;
                websocketUri.Path = "/ws";
                if (uri.Scheme == Uri.UriSchemeHttps) {
                    websocketUri.Scheme = "wss";
                } else {
                    websocketUri.Scheme = "ws";
                }
                Debug.Log("StartNetwork client to " + websocketUri.Uri);
                manager.StartClient(websocketUri.Uri);
            }
        }
        Debug.Log("StartNetwork " + NetworkServer.active);
        if (!NetworkServer.active) {
            if (Utils.IsHeadless()) {
                manager.StartServer();
            }
            else {
                NetworkServer.listen = false;
                manager.StartHost();
            }
        }
    }
}
