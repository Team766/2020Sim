using UnityEngine;
//using Coherence;
//using Coherence.Toolkit;

//[RequireComponent(typeof(CoherenceSync))]
public class BallSpawner : MonoBehaviour
{
    public GameObject ballPrefab;
    public Vector3 spawnImpulse;

    //[Command]
    public void cmdSpawnBall()
    {
        //    var sync = GetComponent<CoherenceSync>();
        //    sync.SendCommand<BallSpawner>(
        //        nameof(SpawnBall),
        //        MessageTarget.StateAuthorityOnly);
        //}

        //// Server-only
        //private Rigidbody SpawnBall()
        //{

        var obj = Instantiate(ballPrefab, this.transform.position, this.transform.rotation);
        var rigidbody = obj.GetComponent<Rigidbody>();
        rigidbody.AddForce(this.transform.TransformVector(spawnImpulse), ForceMode.Impulse);
        //return rigidbody;
    }
}
