using UnityEngine;
using Mirror;

public class BallSpawner : NetworkBehaviour
{
    public GameObject ballPrefab;
    public Vector3 spawnImpulse;

    [Command(requiresAuthority = false)]
    public void cmdSpawnBall()
    {
        SpawnBall();
    }

    [Server]
    Rigidbody SpawnBall()
    {
        var obj = Instantiate(ballPrefab, this.transform.position, this.transform.rotation);
        NetworkServer.Spawn(obj);
        var rigidbody = obj.GetComponent<Rigidbody>();
        rigidbody.AddForce(this.transform.TransformVector(spawnImpulse), ForceMode.Impulse);
        return rigidbody;
    }
}
