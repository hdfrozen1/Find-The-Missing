using UnityEngine;
namespace ithappy {
 public sealed class ClawInput : MonoBehaviour {
  public ClawMachine machine; public Camera view; public Transform lever,button; public Collider leverHit,buttonHit;
  public float maxTilt=22, dragPixels=70, deadZone=.08f, smoothing=.045f, buttonDepth=.012f;
  public Vector2 Command {get;private set;} public bool Dragging {get;private set;}
  Vector2 origin,shown; Quaternion leverRest; Vector3 buttonRest; float buttonTime; bool focused=true;
  void Awake() {leverRest=lever.localRotation;buttonRest=button.localPosition;}
  void Update() {
   if(focused && Input.GetKeyDown(KeyCode.Q))machine.TrySpawnRandomToy();
   Vector2 target=Vector2.zero;
   if(machine.IsReady && focused) {
    if(Input.GetKeyDown(KeyCode.Space)) machine.TryStart();
    if(Input.GetMouseButtonDown(0) && view) {
     var ray=view.ScreenPointToRay(Input.mousePosition);
     bool b=buttonHit.Raycast(ray,out var bh,100),l=leverHit.Raycast(ray,out var lh,100);
     if(b && (!l || bh.distance<lh.distance)) machine.TryStart();
     else if(l) {Dragging=true;origin=Input.mousePosition;}
    }
    if(!Input.GetMouseButton(0)) Dragging=false;
    if(Dragging) target=Vector2.ClampMagnitude(((Vector2)Input.mousePosition-origin)/Mathf.Max(1,dragPixels),1);
    else target=new Vector2((Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.A)?1:0),(Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.S)?1:0));
    target=Vector2.ClampMagnitude(target,1);float mag=target.magnitude;target=mag<=deadZone?Vector2.zero:target.normalized*((mag-deadZone)/(1-deadZone));
   } else Dragging=false;
   // Motion stops immediately on release; only the visual handle eases to centre.
   Command=machine.IsReady?target:Vector2.zero;
   shown=Vector2.Lerp(shown,Command,1-Mathf.Exp(-Time.deltaTime/Mathf.Max(.001f,smoothing)));
   lever.localRotation=leverRest*Quaternion.Euler(-shown.y*maxTilt,0,shown.x*maxTilt);
   buttonTime=Mathf.Max(0,buttonTime-Time.deltaTime);
   button.localPosition=buttonRest+Vector3.down*(buttonDepth*Mathf.Sin(Mathf.Clamp01(buttonTime/.24f)*Mathf.PI));
  }
  public void PressVisual() {buttonTime=.24f;ResetInput();}
  public void ResetInput() {Dragging=false;Command=Vector2.zero;}
  void OnApplicationFocus(bool focus) {focused=focus;if(!focus) ResetInput();}
  void OnDisable() {ResetInput();shown=Vector2.zero;if(lever)lever.localRotation=leverRest;if(button)button.localPosition=buttonRest;}
 }
}
