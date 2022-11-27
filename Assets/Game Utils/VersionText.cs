using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class VersionText : MonoBehaviour {

    void Start() {
        GetComponent<Text>().text = "Version " + Application.version;
    }

}
