using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
public class NetworkHierarchyParent : NetworkBehaviour
{
    public override void OnSerialize(NetworkWriter writer, bool initialState) {
        writer.WriteNetworkIdentity(transform.parent?.GetComponent<NetworkIdentity>());

        base.OnSerialize(writer, initialState);
    }
    
    public override void OnDeserialize(NetworkReader reader, bool initialState) {
        transform.parent = reader.ReadNetworkIdentity()?.transform;

        base.OnDeserialize(reader, initialState);
    }
}
