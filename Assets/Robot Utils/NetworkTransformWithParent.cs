using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
public class NetworkTransformWithParent : NetworkTransformBase
{
    protected override Transform targetComponent => transform;
    
    public override bool OnSerialize(NetworkWriter writer, bool initialState) {
        writer.WriteNetworkIdentity(targetComponent.parent?.GetComponent<NetworkIdentity>());
        
        return base.OnSerialize(writer, initialState);
    }
    
    public override void OnDeserialize(NetworkReader reader, bool initialState) {
        targetComponent.parent = reader.ReadNetworkIdentity()?.transform;
        
        base.OnDeserialize(reader, initialState);
    }
}
