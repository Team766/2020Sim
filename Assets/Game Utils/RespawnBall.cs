using UnityEngine;
using System.Collections;

public class RespawnBall : MonoBehaviour {

    public Bounds bounds;

    void Update () {
        foreach(var ball in GameObject.FindGameObjectsWithTag("Ball")) {
            var ballXf = ball.transform;
            if (ballXf.position.y < bounds.min.y) {
                var respawnPoint = bounds.ClosestPoint(ballXf.position);
                respawnPoint.y = bounds.max.y;
                respawnPoint.z = respawnPoint.z > 0 ? bounds.max.z : bounds.min.z;
                ballXf.position = respawnPoint;
                ball.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = new Color(1, 1, 0, 0.75F);
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
