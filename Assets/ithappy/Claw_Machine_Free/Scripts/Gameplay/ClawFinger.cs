using UnityEngine;
namespace ithappy {
 public sealed class ClawFinger : MonoBehaviour {
  public Vector3 hingeAxis=Vector3.right;
  public Quaternion restRotation=Quaternion.identity;
  public float openAngle=-26, closedAngle=23, degreesPerSecond=55;
  [Range(.1f,1)] public float sweepStepDegrees=.35f;
  public float radius=.0075f;
  // Centreline measured from the imported curved finger, including its flared tip.
  public Vector3[] surface={new Vector3(0,-.018f,.028f),new Vector3(0,-.10f,.069f),new Vector3(0,-.125f,.059f),new Vector3(0,-.145f,.048f),new Vector3(0,-.166f,.025f),new Vector3(0,-.183f,.011f)};
  public float Angle {get;private set;}
  public bool Stopped {get;private set;}
  public ClawToy Contact {get;private set;}
  public Vector3 ContactPoint {get;private set;}
  ClawMachine machine;
  public void Initialize(ClawMachine m) { machine=m; SetAngle(openAngle); }
  void SetAngle(float a) { Angle=a; transform.localRotation=restRotation*Quaternion.AngleAxis(a,hingeAxis); }
  public void BeginClose() { Stopped=false; Contact=null; }
  public bool Obstructed(float angle,Vector3 displacement,out Collider hit,out Vector3 point) {
   var q=transform.parent.rotation*restRotation*Quaternion.AngleAxis(angle,hingeAxis);
   var scale=transform.lossyScale; float r=radius*Mathf.Max(scale.x,Mathf.Max(scale.y,scale.z));
   for(int i=0;i<surface.Length-1;i++) {
    var a=transform.position+displacement+q*Vector3.Scale(surface[i],scale);
    var b=transform.position+displacement+q*Vector3.Scale(surface[i+1],scale);
    foreach(var c in Physics.OverlapCapsule(a,b,r,~0,QueryTriggerInteraction.Ignore)) {
     var toy=c.GetComponentInParent<ClawToy>();
     if((toy && toy.owner==machine && !toy.Awarded) || ClawObstacle.BlocksClaw(c,machine.transform)) {
      hit=c; point=c.ClosestPoint((a+b)*.5f); return true;
     }
    }
   }
   hit=null; point=default; return false;
  }
  public void CloseStep(float dt) {
   if(Stopped) return;
   float target=Mathf.MoveTowards(Angle,closedAngle,degreesPerSecond*dt);
   int steps=Mathf.Max(1,Mathf.CeilToInt(Mathf.Abs(target-Angle)/sweepStepDegrees)); float start=Angle;
   for(int i=1;i<=steps;i++) {
    float next=Mathf.Lerp(start,target,(float)i/steps);
    if(Obstructed(next,Vector3.zero,out var hit,out var point)) { Stopped=true; Contact=hit.GetComponentInParent<ClawToy>(); ContactPoint=point; return; }
    SetAngle(next);
   }
   if(Mathf.Abs(Angle-closedAngle)<.01f) Stopped=true;
  }
  public bool OpenStep(float dt) { SetAngle(Mathf.MoveTowards(Angle,openAngle,degreesPerSecond*dt)); Contact=null; Stopped=false; return Mathf.Abs(Angle-openAngle)<.01f; }
  public bool Supports(ClawToy toy) {
   if(Contact!=toy) return false;
   // Revalidate stopped contact: a toy that fell away cannot produce a stale grasp.
   foreach(var c in toy.GetComponentsInChildren<Collider>()) if(c.enabled && Vector3.Distance(c.ClosestPoint(ContactPoint),ContactPoint)<.018f) return true;
   return false;
  }
  void OnDrawGizmosSelected() { Gizmos.color=Color.yellow; for(int i=0;i<surface.Length-1;i++) {Gizmos.DrawLine(transform.TransformPoint(surface[i]),transform.TransformPoint(surface[i+1]));Gizmos.DrawWireSphere(transform.TransformPoint(surface[i+1]),radius);} }
 }
}
