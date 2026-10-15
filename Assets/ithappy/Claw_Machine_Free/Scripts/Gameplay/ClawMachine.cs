using UnityEngine;
namespace ithappy {
 public sealed class ClawMachine : MonoBehaviour {
  public enum Phase {Ready,Descending,Closing,Lifting,MovingToDrop,Releasing,WaitingForPrize,PresentingPrize,Returning}
  public ClawMechanism mechanism;public ClawInput controls;public ClawReceiver receiver;public ClawPresentation presentation;public ClawFeedback feedback;
  public Transform toyRoot;public ClawToy[] refillPrefabs;public Vector3[] refillPositions;
  public bool autoRefill=true,scatterRefill;public float phaseTimeout=12,closingTimeout=2,deliveryTimeout=5;
  [Header("Random toy spawn (Q)")]
  public Vector3 spawnAreaMin=new Vector3(-.46f,1.35f,-.38f),spawnAreaMax=new Vector3(.46f,2.02f,.36f);
  [Min(.01f)] public float spawnedToyScale=.65f;
  [Min(1)] public int spawnAttempts=80;
  public Phase State {get;private set;}=Phase.Ready;public bool IsReady=>enabled && State==Phase.Ready && (!receiver || !receiver.HasPending);
  public ClawToy Carried {get;private set;} public int Wins {get;private set;} public string LastResult {get;private set;}="Position the claw";
  float elapsed;bool released;int refillIndex;
  void Start(){mechanism.Initialize(this);receiver.Clear();}
  public bool TryStart(){if(!IsReady)return false;controls.PressVisual();feedback.Play(feedback.button);Carried=null;receiver.Clear();SetPhase(Phase.Descending);return true;}
  void SetPhase(Phase p){State=p;elapsed=0;feedback.Motor(null,false);}
  void Update(){feedback.Animate(State);}
  void FixedUpdate(){
   float dt=Time.fixedDeltaTime;elapsed+=dt;bool moving=false;AudioClip loop=null;
   switch(State){
    case Phase.Ready:
     if(TryBeginPrize(false)) break;
     Vector2 cmd=controls.Command;moving=mechanism.MoveHorizontal(mechanism.Position-new Vector2(cmd.x,cmd.y)*mechanism.horizontalSpeed*dt,dt);loop=feedback.horizontal;
     if(autoRefill && elapsed>.5f) RefillIfEmpty();break;
    case Phase.Descending:
     moving=mechanism.MoveVertical(mechanism.travel,dt,true);loop=feedback.vertical;
     if(!moving || elapsed>phaseTimeout){foreach(var f in mechanism.fingers)f.BeginClose();SetPhase(Phase.Closing);}break;
    case Phase.Closing:
     bool done=true;foreach(var f in mechanism.fingers){f.CloseStep(dt);done &= f.Stopped;}moving=!done;loop=feedback.fingers;
     if(done || elapsed>closingTimeout){Carried=mechanism.FindSupportedToy();if(Carried)Carried.Hold(mechanism.grip);else {LastResult="No grip — try again";feedback.Play(feedback.miss);}SetPhase(Phase.Lifting);}break;
    case Phase.Lifting:
     moving=mechanism.MoveVertical(0,dt,false);loop=feedback.vertical;
     if(!moving)SetPhase(Carried?Phase.MovingToDrop:Phase.Returning);else if(elapsed>phaseTimeout)Recover();break;
    case Phase.MovingToDrop:
     if(!Carried || !Carried.Held){Fail();break;}
     moving=mechanism.MoveHorizontal(mechanism.drop,dt);loop=feedback.horizontal;
     if(Vector2.Distance(mechanism.Position,mechanism.drop)<.001f){released=false;SetPhase(Phase.Releasing);}else if(elapsed>phaseTimeout)Recover();break;
    case Phase.Releasing:
     if(!released){receiver.Arm(Carried);if(Carried){Carried.Release();feedback.Play(feedback.toyDrop);}released=true;}
     bool opened=true;foreach(var f in mechanism.fingers)opened &= f.OpenStep(dt);moving=!opened;loop=feedback.fingers;
     // Start physical reception immediately; fingers finish opening during the fall.
     SetPhase(Phase.WaitingForPrize);break;
    case Phase.WaitingForPrize:
     moving=ReturnToHome(dt,out loop);
     if(receiver.Received){Carried=null;receiver.Clear();if(!TryBeginPrize(false))SetPhase(Phase.Returning);}
     else if(elapsed>deliveryTimeout || !Carried)Fail();break;
    case Phase.PresentingPrize:
     moving=ReturnToHome(dt,out loop);
     if(presentation.Step(dt) && !TryBeginPrize(true))SetPhase(Phase.Returning);break;
    case Phase.Returning:
     moving=ReturnToHome(dt,out loop);
     if(TryBeginPrize(false)) break;
     if(!moving){Carried=null;receiver.Clear();SetPhase(Phase.Ready);}else if(elapsed>phaseTimeout)Recover();break;
   }
   if(Carried && Carried.Held) {
    Carried.Follow();
    if(State==Phase.Lifting || State==Phase.MovingToDrop)mechanism.TightenGrip(Carried,dt);
   }
   feedback.Motor(loop,moving);
  }
  bool TryBeginPrize(bool additionalPrize) {
   if(!receiver.TryTakePrize(out var toy)) return false;
   Wins++;LastResult="Won: "+toy.displayName;feedback.Play(feedback.win);
   presentation.Begin(toy,additionalPrize);SetPhase(Phase.PresentingPrize);return true;
  }
  bool ReturnToHome(float dt,out AudioClip loop){
   bool open=true;foreach(var f in mechanism.fingers)open &= f.OpenStep(dt);
   loop=open?feedback.horizontal:feedback.fingers;
   // Clear the released toy before moving away; return overlaps physical delivery and presentation.
   return !open || mechanism.MoveHorizontal(mechanism.home,dt);
  }
  void Fail(){if(Carried)Carried.Release();Carried=null;receiver.Clear();LastResult="No delivery — try again";feedback.Play(feedback.toyStuck?feedback.toyStuck:feedback.miss);SetPhase(Phase.Returning);}
  public void Recover(){if(Carried)Carried.Release();Carried=null;presentation.Finish();receiver.Clear();mechanism.RecoverPose();controls.ResetInput();SetPhase(Phase.Returning);}
  public bool TrySpawnRandomToy(){
   if(!isActiveAndEnabled || !toyRoot || refillPrefabs==null || refillPrefabs.Length==0)return false;
   Physics.SyncTransforms();
   int first=Random.Range(0,refillPrefabs.Length);
   for(int attempt=0;attempt<Mathf.Max(1,spawnAttempts);attempt++){
    var prefab=refillPrefabs[(first+attempt%refillPrefabs.Length)%refillPrefabs.Length];if(!prefab)continue;
    var filter=prefab.GetComponent<MeshFilter>();if(!filter || !filter.sharedMesh)continue;
    var bounds=filter.sharedMesh.bounds;var localRotation=Random.rotationUniform;
    var scaledExtents=bounds.extents*spawnedToyScale;
    var matrix=Matrix4x4.Rotate(localRotation);
    var extents=new Vector3(
     Mathf.Abs(matrix.m00)*scaledExtents.x+Mathf.Abs(matrix.m01)*scaledExtents.y+Mathf.Abs(matrix.m02)*scaledExtents.z,
     Mathf.Abs(matrix.m10)*scaledExtents.x+Mathf.Abs(matrix.m11)*scaledExtents.y+Mathf.Abs(matrix.m12)*scaledExtents.z,
     Mathf.Abs(matrix.m20)*scaledExtents.x+Mathf.Abs(matrix.m21)*scaledExtents.y+Mathf.Abs(matrix.m22)*scaledExtents.z);
    var min=spawnAreaMin+extents+Vector3.one*.01f;var max=spawnAreaMax-extents-Vector3.one*.01f;
    if(min.x>max.x || min.y>max.y || min.z>max.z)continue;
    var center=new Vector3(Random.Range(min.x,max.x),Random.Range(min.y,max.y),Random.Range(min.z,max.z));
    // Keep new toys over the solid floor, away from the delivery shaft.
    if(center.x+extents.x>.03f && center.z+extents.z>.06f)continue;
    var world=transform.TransformPoint(center);var rotation=transform.rotation*localRotation;
    var worldExtents=Vector3.Scale(scaledExtents,transform.lossyScale)+Vector3.one*.005f;
    if(Physics.CheckBox(world,worldExtents,rotation,~0,QueryTriggerInteraction.Ignore))continue;
    if(mechanism && mechanism.grip && Vector3.Distance(world,mechanism.grip.position)<extents.magnitude+.17f)continue;
    var toy=Instantiate(prefab,toyRoot);toy.owner=this;toy.transform.localScale=Vector3.one*spawnedToyScale;
    toy.transform.SetPositionAndRotation(world-rotation*(bounds.center*spawnedToyScale),rotation);
    toy.Body.isKinematic=false;toy.Body.useGravity=true;toy.Body.detectCollisions=true;
    toy.Body.linearVelocity=Vector3.zero;toy.Body.angularVelocity=Vector3.zero;toy.Body.WakeUp();
    LastResult="Added: "+toy.displayName;return true;
   }
   LastResult="No room for another toy";return false;
  }
  public void RefillIfEmpty(){
   foreach(var t in toyRoot.GetComponentsInChildren<ClawToy>())if(!t.Awarded && transform.InverseTransformPoint(t.transform.position).y>1.05f)return;
   if(refillPrefabs==null || refillPrefabs.Length==0)return;
   foreach(var p in refillPositions){var prefab=refillPrefabs[refillIndex++%refillPrefabs.Length];if(!prefab)continue;
    var world=transform.TransformPoint(p);var rotation=transform.rotation;
    if(scatterRefill)rotation*=Quaternion.Euler((refillIndex*71)%280,(refillIndex*137)%360,(refillIndex*53)%300);
    var mesh=prefab.GetComponent<MeshFilter>().sharedMesh;var scale=prefab.transform.localScale;
    var extent=Vector3.Scale(mesh.bounds.extents,scale);
    var center=scatterRefill?world:world+rotation*Vector3.Scale(mesh.bounds.center,scale);
    if(Physics.CheckBox(center,extent,rotation,~0,QueryTriggerInteraction.Ignore))continue;
    var toy=Instantiate(prefab,toyRoot);toy.transform.SetPositionAndRotation(scatterRefill?world-rotation*Vector3.Scale(mesh.bounds.center,scale):world,rotation);toy.owner=this;
   }elapsed=0;
  }
  void OnDisable(){if(Carried)Carried.Release();if(controls)controls.ResetInput();if(presentation)presentation.Finish();if(feedback)feedback.Motor(null,false);}
 }
}
