using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chain : MonoBehaviour
{
    public GameObject chainLink;
    public int chainLength;
    public Vector3 linkStride;
    public Joint terminus;

    void Awake()
    {
        var prevLink = this.gameObject;
        for (int i = 0; i < chainLength; ++i)
        {
            var nextLink = Instantiate<GameObject>(
                chainLink,
                prevLink.transform.TransformPoint(linkStride),
                prevLink.transform.rotation);
            nextLink.transform.parent = prevLink.transform;
            nextLink.GetComponent<ConfigurableJoint>().connectedBody = prevLink.GetComponent<Rigidbody>();
            prevLink = nextLink;
        }
        terminus.connectedBody = prevLink.GetComponent<Rigidbody>();
        terminus.anchor = Vector3.zero;
        terminus.autoConfigureConnectedAnchor = false;
        terminus.connectedAnchor = Vector3.zero;
    }
}
