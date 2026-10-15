using UnityEngine;

namespace ithappy
{
    [DefaultExecutionOrder(-100), DisallowMultipleComponent]
    public sealed class ClawSwing : MonoBehaviour
    {
        public ClawMechanism mechanism;
        [Tooltip("Claw_Piston_01: suspension point, rod and lower claw swing together.")]
        public Transform suspension;
        [Min(.01f)] public float mass = .35f;
        [Min(.05f)] public float hangingLength = .3f;
        [Range(0, 15)] public float maxAngle = 5;
        [Min(0)] public float spring = .6f;
        [Min(0)] public float damping = .12f;
        [Min(0)] public float angularDrag = .1f;

        Rigidbody anchor, pendulum;
        ConfigurableJoint joint;
        Quaternion restRotation;
        GameObject physicsRoot;

        void OnEnable()
        {
            if (!mechanism || !suspension) return;
            restRotation = suspension.localRotation;
            // World-space physics bodies avoid inheriting the carriage's transform motion.
            physicsRoot = new GameObject("Claw swing physics (runtime)");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(physicsRoot, gameObject.scene);
            anchor = CreateBody("Moving suspension", true);
            pendulum = CreateBody("Pendulum", false);
            pendulum.mass = mass;
            pendulum.useGravity = true;
            pendulum.maxAngularVelocity = 8;
            pendulum.solverIterations = 16;
            pendulum.solverVelocityIterations = 8;
            joint = pendulum.gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = anchor;
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = joint.connectedAnchor = Vector3.zero;
            joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = joint.angularZMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.projectionMode = JointProjectionMode.PositionAndRotation;
            joint.projectionDistance = .001f;
            joint.projectionAngle = 1;
            ApplySettings();
        }

        Rigidbody CreateBody(string name, bool kinematic)
        {
            var node = new GameObject(name);
            node.transform.SetParent(physicsRoot.transform, false);
            node.transform.SetPositionAndRotation(suspension.position, suspension.parent.rotation * restRotation);
            var body = node.AddComponent<Rigidbody>();
            body.isKinematic = kinematic;
            body.useGravity = false;
            return body;
        }

        void ApplySettings()
        {
            float length = Mathf.Max(.05f, hangingLength + mechanism.Depth);
            pendulum.mass = Mathf.Max(.01f, mass);
            pendulum.centerOfMass = Vector3.down * length;
            pendulum.inertiaTensorRotation = Quaternion.identity;
            pendulum.inertiaTensor = Vector3.one * Mathf.Max(.001f, pendulum.mass * length * length);
            pendulum.angularDamping = angularDrag;
            joint.lowAngularXLimit = new SoftJointLimit { limit = -maxAngle };
            joint.highAngularXLimit = new SoftJointLimit { limit = maxAngle };
            joint.angularZLimit = new SoftJointLimit { limit = maxAngle };
            var drive = new JointDrive { positionSpring = spring, positionDamper = damping, maximumForce = 100 };
            joint.angularXDrive = joint.angularYZDrive = drive;
        }

        void FixedUpdate()
        {
            if (!pendulum || !suspension) return;
            ApplySettings();
            anchor.MovePosition(suspension.parent.TransformPoint(suspension.localPosition));
            anchor.MoveRotation(suspension.parent.rotation * restRotation);
            suspension.rotation = pendulum.rotation;
        }

        void OnDisable()
        {
            if (suspension && physicsRoot) suspension.localRotation = restRotation;
            if (physicsRoot) Destroy(physicsRoot);
            physicsRoot = null; anchor = pendulum = null; joint = null;
        }
    }
}
