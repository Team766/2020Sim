using UnityEngine;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]

public class VersionRecorder {

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    void Start() {
        Debug.Log("Version " + Application.version);
    }

}
