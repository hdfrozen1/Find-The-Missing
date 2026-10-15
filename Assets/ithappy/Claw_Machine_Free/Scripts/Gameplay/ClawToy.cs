using UnityEngine;
namespace ithappy {
 [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody))]
 public sealed class ClawToy : MonoBehaviour {
  public ClawMachine owner;
  public string displayName;
  public Rigidbody Body { get; private set; }
  public bool Awarded { get; private set; }
  Transform grip; Vector3 offset; Quaternion rotation; bool oldGravity, oldKinematic; RigidbodyInterpolation oldInterpolation;
  public bool Held => grip != null;
  void Awake() { Body=GetComponent<Rigidbody>(); }
  public Vector3 Center => Body ? Body.worldCenterOfMass : transform.position;
  public void Hold(Transform anchor) {
   if(Held || Awarded) return;
   oldGravity=Body.useGravity; oldKinematic=Body.isKinematic; oldInterpolation=Body.interpolation;
   offset=anchor.InverseTransformPoint(Body.position); rotation=Quaternion.Inverse(anchor.rotation)*Body.rotation;
   Body.linearVelocity=Vector3.zero; Body.angularVelocity=Vector3.zero; Body.useGravity=false; Body.isKinematic=true; grip=anchor;
  }
  public void Follow() { if(grip) { Body.position=grip.TransformPoint(offset); Body.rotation=grip.rotation*rotation; } }
  public void Release() {
   if(!Held) return; Follow(); grip=null; Body.isKinematic=oldKinematic; Body.useGravity=oldGravity; Body.interpolation=oldInterpolation;
   if(!Body.isKinematic) { Body.linearVelocity=Vector3.zero; Body.angularVelocity=Vector3.zero; Body.WakeUp(); }
  }
  public void Present() { Release(); Awarded=true; Body.isKinematic=true; Body.detectCollisions=false; }
  void OnDisable() { if(Body) Release(); }
 }
}
