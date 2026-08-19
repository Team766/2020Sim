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
        if (Application.platform == RuntimePlatform.WebGLPlayer) {
            if (!NetworkClient.active) {
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
        } else {
            if (!NetworkServer.active) {
                if (Utils.IsHeadless()) {
                    Debug.Log("StartNetwork server");
                    manager.StartServer();
                }
                else {
                    Debug.Log("StartNetwork host");
                    NetworkServer.listen = false;
                    manager.StartHost();
                }
            }
        }
    }

    void OnDestroy() {
        var manager = NetworkManager.singleton;
        if (!manager) {
            return;
        }
        Debug.Log("StartNetwork stop");
        // Should work properly regardless if we have are host or server-only.
        manager.StopHost();
    }
}
