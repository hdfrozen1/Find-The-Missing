using UnityEngine;
namespace ithappy {
 public sealed class ClawMechanism : MonoBehaviour {
  public Transform machine, rail, carriage, piston, rodVisual, grip;
  public ClawFinger[] fingers;
  public Vector2 xLimits=new Vector2(-.32f,.32f), zLimits=new Vector2(-.25f,.24f);
  public Vector2 home=Vector2.zero, drop=new Vector2(.2596f,.2258f);
  public float travel=.65f, horizontalSpeed=.286f, verticalSpeed=.18f, descentSpeed=.216f, graspRadius=.17f;
  Vector3 railHome,carriageHome,pistonHome,rodScale; Vector2 referencePosition; float depth;bool initialized;
  public float Depth=>depth;
  public Vector2 Position { get {if(initialized)return referencePosition+new Vector2(carriage.localPosition.x-carriageHome.x,rail.localPosition.z-railHome.z);var p=machine.InverseTransformPoint(grip.position);return new Vector2(p.x,p.z);} }
  public void Initialize(ClawMachine owner) {
   initialized=false;railHome=rail.localPosition; carriageHome=carriage.localPosition; pistonHome=piston.localPosition; rodScale=rodVisual.localScale;referencePosition=Position;initialized=true;
   foreach(var f in fingers) f.Initialize(owner);
  }
  public bool MoveHorizontal(Vector2 target,float dt) {
   var before=Position; target=new Vector2(Mathf.Clamp(target.x,xLimits.x,xLimits.y),Mathf.Clamp(target.y,zLimits.x,zLimits.y));
   var next=Vector2.MoveTowards(before,target,horizontalSpeed*dt);
   if((next-before).sqrMagnitude<1e-12f)return false;
   rail.localPosition=railHome+Vector3.forward*(next.y-referencePosition.y);
   carriage.localPosition=carriageHome+Vector3.right*(next.x-referencePosition.x);
   return (Position-before).sqrMagnitude>1e-10f;
  }
  public bool MoveVertical(float target,float dt,bool testObstacles) {
   float next=Mathf.MoveTowards(depth,Mathf.Clamp(target,0,travel),(target>depth?descentSpeed:verticalSpeed)*dt);
   int steps=Mathf.Max(1,Mathf.CeilToInt(Mathf.Abs(next-depth)/.002f)); float start=depth;
   for(int i=1;i<=steps;i++) {
    float d=Mathf.Lerp(start,next,(float)i/steps); var delta=piston.parent.TransformVector(Vector3.down*(d-depth));
    if(testObstacles) {
     foreach(var f in fingers) if(f.Obstructed(f.Angle,delta,out _,out _)) return false;
     foreach(var c in Physics.OverlapSphere(grip.position+delta+machine.up*.035f,.04f*machine.lossyScale.x,~0,QueryTriggerInteraction.Ignore)) {
      var toy=c.GetComponentInParent<ClawToy>();if((toy && toy.owner==GetComponent<ClawMachine>() && !toy.Awarded) || ClawObstacle.BlocksClaw(c,machine))return false;
     }
    }
    depth=d; piston.localPosition=pistonHome+Vector3.down*depth;
    rodVisual.localScale=new Vector3(rodScale.x,rodScale.y*(1+depth/.1174f),rodScale.z);
   }
   return Mathf.Abs(depth-target)>.0001f;
  }
  public void RecoverPose() { depth=0;piston.localPosition=pistonHome;rodVisual.localScale=rodScale; }
  public void TightenGrip(ClawToy heldToy,float dt) {
   if(!heldToy || !heldToy.Held || heldToy.Awarded) return;
   // Follow has moved the held toy into its current pose before these collision queries.
   Physics.SyncTransforms();
   foreach(var f in fingers) {
    // A stop against a neighbouring toy is temporary once the grasp leaves the pile.
    // Retry the same swept closure; the held toy and other obstacles still stop each finger.
    f.BeginClose(); f.CloseStep(dt);
   }
  }
  public ClawToy FindSupportedToy() {
   foreach(var f in fingers) {
    var toy=f.Contact; if(!toy || toy.Awarded || !f.Supports(toy)) continue;
    Vector3 p=grip.InverseTransformPoint(toy.Center);
    if(new Vector2(p.x,p.z).magnitude>graspRadius || p.y < -.23f || p.y>.04f) continue;
    foreach(var other in fingers) if(other!=f && other.Supports(toy)) {
     Vector3 a=Vector3.ProjectOnPlane(f.transform.position-grip.position,machine.up);
     Vector3 b=Vector3.ProjectOnPlane(other.transform.position-grip.position,machine.up);
     if(Vector3.Angle(a,b)>=75) return toy;
    }
   }
   return null;
  }
  void OnDrawGizmosSelected() {
   if(!machine) return; Gizmos.matrix=machine.localToWorldMatrix; Gizmos.color=Color.cyan;
   Gizmos.DrawWireCube(new Vector3((xLimits.x+xLimits.y)/2,1.81f,(zLimits.x+zLimits.y)/2),new Vector3(xLimits.y-xLimits.x,0,zLimits.y-zLimits.x));
   Gizmos.DrawLine(new Vector3(home.x,1.81f,home.y),new Vector3(home.x,1.81f-travel,home.y));
   Gizmos.color=Color.green;Gizmos.DrawWireSphere(new Vector3(drop.x,1.8f,drop.y),.035f); Gizmos.DrawWireSphere(new Vector3(home.x,1.8f,home.y),.025f);
   Gizmos.matrix=Matrix4x4.identity;if(grip) {Gizmos.color=Color.yellow;Gizmos.DrawWireSphere(grip.position-grip.up*.1f,graspRadius);}
  }
 }
}
