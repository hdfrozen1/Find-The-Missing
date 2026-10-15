using UnityEngine;
namespace ithappy {
 public sealed class ClawObstacle : MonoBehaviour {
  [Tooltip("Ignore this obstacle in claw descent and finger contact checks. Toy collisions remain enabled.")]
  public bool ignoreClaw;
  public static bool BlocksClaw(Collider collider,Transform machine) {
   var obstacle=collider.GetComponentInParent<ClawObstacle>();
   return obstacle && !obstacle.ignoreClaw && collider.transform.IsChildOf(machine);
  }
 }
}
