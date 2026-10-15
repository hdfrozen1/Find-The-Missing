using System.Collections.Generic;
using UnityEngine;
namespace ithappy {
 [RequireComponent(typeof(BoxCollider))]
 public sealed class ClawReceiver : MonoBehaviour {
  public ClawMachine machine; public ClawToy Expected {get;private set;} public bool Received {get;private set;}
  readonly Queue<ClawToy> pending = new Queue<ClawToy>();
  public bool HasPending => pending.Count > 0;
  public void Arm(ClawToy toy) {Expected=toy;Received=false;}
  // Clear only the current claw delivery; unrelated prizes must remain queued.
  public void Clear() {Expected=null;Received=false;}
  public bool TryTakePrize(out ClawToy toy) {
   while(pending.Count>0) {
    toy=pending.Dequeue();if(toy && toy.owner==machine) return true;
   }
   toy=null;return false;
  }
  void OnTriggerEnter(Collider c) {Accept(c);} void OnTriggerStay(Collider c) {Accept(c);}
  void Accept(Collider c) {
   var t=c.GetComponentInParent<ClawToy>();
   if(!machine || !machine.isActiveAndEnabled || !t || t.owner!=machine || t.Held || t.Awarded) return;
   if(t==Expected) Received=true;
   // Reserve immediately: multiple colliders and repeated Stay callbacks cannot count twice.
   // Waiting prizes leave physics so they cannot block the next toy in the receiver.
   t.Present();pending.Enqueue(t);
  }
  void OnDrawGizmos() {var c=GetComponent<BoxCollider>();if(!c)return;Gizmos.matrix=transform.localToWorldMatrix;Gizmos.color=Color.green;Gizmos.DrawWireCube(c.center,c.size);}
 }
}
