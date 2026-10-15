using UnityEngine;

namespace ithappy
{
    public sealed class ClawPresentation : MonoBehaviour
    {
        public enum Stage { Idle, OpeningDoor, ExitingHatch, Approaching, Showing, ClosingDoor }
        public Camera view;
        public Transform displayPoint, hatchDoor, hatchExit;
        public Vector3 doorAxis = Vector3.right;
        public float doorOpenAngle = -100;
        public float doorOpenDuration = .45f, hatchFlightDuration = .7f;
        public float approachDuration = .9f, doorCloseDuration = .4f, hatchClearance = .26f;
        public float duration = 4.5f, rise = .22f, turns = 1.4f, pullback = .5f, maxDisplaySize = .55f;
        public Vector3 exitDirection = new Vector3(-1.5f, .4f, 0);
        [Header("Queued prizes")]
        [Tooltip("Duration multiplier for the second and subsequent prizes in a batch.")]
        [Range(.1f, 1)] public float additionalPrizeDurationScale = .4f;
        public Stage CurrentStage { get; private set; }

        Vector3 cameraPosition, startCenter, localCenter, originalScale, passageScale, displayScale;
        Quaternion cameraRotation, doorRest, prizeRotation;
        ClawToy prize;
        bool active;
        float elapsed, animationDurationScale = 1;

        public void Begin(ClawToy toy, bool additionalPrize = false)
        {
            prize = toy;
            prize.Present(); // Called only after the physical receiver confirms this toy.
            cameraPosition = view.transform.position;
            cameraRotation = view.transform.rotation;
            if (hatchDoor) doorRest = hatchDoor.localRotation;
            elapsed = 0;
            animationDurationScale = additionalPrize ? Mathf.Clamp(additionalPrizeDurationScale, .1f, 1) : 1;
            active = true;
            originalScale = toy.transform.localScale;
            prizeRotation = toy.transform.rotation;
            var renderers = toy.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            startCenter = bounds.center;
            localCenter = toy.transform.InverseTransformPoint(startCenter);
            float size = Mathf.Max(.01f, Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z)));
            passageScale = originalScale * Mathf.Min(1, hatchClearance / size);
            displayScale = originalScale * maxDisplaySize / size;
            CurrentStage = Stage.OpeningDoor;
        }

        public bool Step(float dt)
        {
            if (!active) return true;
            elapsed += dt / animationDurationScale;
            float open = Mathf.Max(.01f, doorOpenDuration);
            float flight = Mathf.Max(.01f, hatchFlightDuration);
            float approach = Mathf.Max(.01f, approachDuration);
            float show = Mathf.Max(.1f, duration);
            float close = Mathf.Max(.01f, doorCloseDuration);
            float showStart = open + flight + approach;
            float closeStart = showStart + show;
            float total = closeStart + close;
            Vector3 outlet = hatchExit ? hatchExit.position : displayPoint.position;
            Vector3 display = displayPoint.position + displayPoint.up * rise;
            float cameraAmount = Ease(elapsed / open) * (1 - Ease((elapsed - closeStart) / close));
            view.transform.SetPositionAndRotation(cameraPosition - cameraRotation * Vector3.forward * (pullback * cameraAmount), cameraRotation);
            float doorAmount = Ease(elapsed / open) * (1 - Ease((elapsed - closeStart) / close));
            if (hatchDoor) hatchDoor.localRotation = doorRest * Quaternion.AngleAxis(doorOpenAngle * doorAmount, doorAxis);

            if (elapsed < open)
            {
                CurrentStage = Stage.OpeningDoor;
                Pose(startCenter, prizeRotation, Vector3.Lerp(originalScale, passageScale, Ease(elapsed / open)));
            }
            else if (elapsed < open + flight)
            {
                CurrentStage = Stage.ExitingHatch;
                float p = Ease((elapsed - open) / flight);
                Vector3 inside = outlet - (hatchExit ? hatchExit.forward : displayPoint.forward) * .43f;
                Vector3 center = p < .5f ? Vector3.Lerp(startCenter, inside, p * 2) : Vector3.Lerp(inside, outlet, (p - .5f) * 2);
                Pose(center, prizeRotation, passageScale);
            }
            else if (elapsed < showStart)
            {
                CurrentStage = Stage.Approaching;
                float p = Ease((elapsed - open - flight) / approach);
                Vector3 center = Vector3.Lerp(outlet, display, p) + displayPoint.up * (Mathf.Sin(p * Mathf.PI) * rise);
                Pose(center, Quaternion.Slerp(prizeRotation, displayPoint.rotation, p), Vector3.Lerp(passageScale, displayScale, p));
            }
            else if (elapsed < closeStart)
            {
                CurrentStage = Stage.Showing;
                float p = Mathf.Clamp01((elapsed - showStart) / show);
                float fly = Ease(Mathf.InverseLerp(.73f, 1, p));
                Pose(display + displayPoint.TransformDirection(exitDirection) * fly,
                    displayPoint.rotation * Quaternion.Euler(0, p * 360 * turns, 0), displayScale);
            }
            else
            {
                CurrentStage = Stage.ClosingDoor;
                if (prize) { Destroy(prize.gameObject); prize = null; }
            }
            if (elapsed >= total) { Finish(); return true; }
            return false;
        }

        static float Ease(float p) { return Mathf.SmoothStep(0, 1, Mathf.Clamp01(p)); }

        void Pose(Vector3 center, Quaternion rotation, Vector3 scale)
        {
            if (!prize) return;
            prize.transform.localScale = scale;
            prize.transform.rotation = rotation;
            prize.transform.position = center - prize.transform.TransformVector(localCenter);
        }

        public void Finish()
        {
            if (!active) return;
            if (view) view.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
            if (hatchDoor) hatchDoor.localRotation = doorRest;
            if (prize) Destroy(prize.gameObject);
            prize = null;
            active = false;
            CurrentStage = Stage.Idle;
        }

        void OnDisable() { Finish(); }
        void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            if (displayPoint) Gizmos.DrawWireSphere(displayPoint.position, maxDisplaySize * .5f);
            if (hatchExit)
            {
                Gizmos.DrawWireSphere(hatchExit.position, hatchClearance * .5f);
                if (displayPoint) Gizmos.DrawLine(hatchExit.position, displayPoint.position);
            }
        }
    }
}
