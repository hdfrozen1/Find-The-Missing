using System;
using UnityEngine;

namespace ithappy
{
    [DisallowMultipleComponent]
    public sealed class ClawKick : MonoBehaviour
    {
        [Serializable]
        public sealed class Pattern
        {
            public string name = "Kick";
            [Min(.05f)] public float duration = .65f;
            [Tooltip("Local cabinet displacement in metres.")]
            public Vector3 shakePosition = new Vector3(0, 0, -.015f);
            [Tooltip("Local cabinet rotation in degrees.")]
            public Vector3 shakeRotation = new Vector3(1.2f, 0, 0);
            [Min(1)] public float oscillations = 3;
            [Tooltip("Toy velocity change in cabinet coordinates, metres/second.")]
            public Vector3 toyImpulse = new Vector3(0, .12f, -.4f);
            [Tooltip("Toy angular velocity change in cabinet coordinates, radians/second.")]
            public Vector3 toyTorque = new Vector3(.7f, 0, .3f);
            [Tooltip("Times of physical pulses as fractions of Duration. First pulse is immediate.")]
            public float[] extraPulseTimes = new float[0];
            [Range(0, 1)] public float extraPulseStrength = .5f;
        }

        public ClawMachine machine;
        public Transform shakeRoot;
        public KeyCode key = KeyCode.E;
        [Min(0)] public float strength = 1;
        [Min(0)] public float visualStrength = 1;
        [Min(0)] public float cooldown = .9f;
        public bool cyclePatterns = true;
        [Min(0)] public int selectedPattern;
        public AudioClip impact;
        public AudioClip rocking;
        [Range(0, 1)] public float impactVolume = .7f;
        public Pattern[] patterns =
        {
            new Pattern { name = "Front kick" },
            new Pattern { name = "Side rocking", duration = .8f,
                shakePosition = new Vector3(.018f, 0, 0), shakeRotation = new Vector3(0, 0, -1.6f),
                toyImpulse = new Vector3(.35f, .1f, -.08f), toyTorque = new Vector3(0, .3f, -.8f),
                extraPulseTimes = new[] { .28f }, extraPulseStrength = .65f },
            new Pattern { name = "Double kick", duration = .75f, oscillations = 4,
                shakePosition = new Vector3(0, .006f, -.012f), shakeRotation = new Vector3(.9f, 0, .5f),
                toyImpulse = new Vector3(-.1f, .18f, -.3f), toyTorque = new Vector3(.9f, .2f, .4f),
                extraPulseTimes = new[] { .32f }, extraPulseStrength = .8f }
        };

        Vector3 restPosition;
        Quaternion restRotation;
        Pattern active;
        bool[] pulses;
        float elapsed, nextKick;
        int nextPattern;
        bool focused = true;

        void Awake()
        {
            if (!machine) machine = GetComponent<ClawMachine>();
            if (shakeRoot) { restPosition = shakeRoot.localPosition; restRotation = shakeRoot.localRotation; }
        }

        void Update() { if (focused && Input.GetKeyDown(key)) TryKick(); }

        public bool TryKick()
        {
            if (!isActiveAndEnabled || !machine || !machine.enabled || !shakeRoot || active != null ||
                Time.time < nextKick || patterns == null || patterns.Length == 0 ||
                machine.State == ClawMachine.Phase.PresentingPrize) return false;
            int index = cyclePatterns ? nextPattern++ % patterns.Length : Mathf.Clamp(selectedPattern, 0, patterns.Length - 1);
            active = patterns[index];
            if (active == null) return false;
            restPosition = shakeRoot.localPosition;
            restRotation = shakeRoot.localRotation;
            elapsed = 0;
            pulses = new bool[active.extraPulseTimes == null ? 0 : active.extraPulseTimes.Length];
            nextKick = Time.time + Mathf.Max(cooldown, active.duration);
            Pulse(1);
            if (impact && machine.feedback && machine.feedback.effects)
                machine.feedback.effects.PlayOneShot(impact, impactVolume);
            if (rocking && machine.feedback && machine.feedback.effects)
                machine.feedback.effects.PlayOneShot(rocking, impactVolume * .5f);
            return true;
        }

        void FixedUpdate()
        {
            if (active == null) return;
            elapsed += Time.fixedDeltaTime;
            float progress = Mathf.Clamp01(elapsed / Mathf.Max(.05f, active.duration));
            for (int i = 0; i < pulses.Length; i++)
                if (!pulses[i] && progress >= Mathf.Clamp01(active.extraPulseTimes[i]))
                { pulses[i] = true; Pulse(active.extraPulseStrength); }
            if (progress >= 1) { Restore(); return; }
            float wave = Mathf.Sin(progress * Mathf.PI * 2 * active.oscillations) * (1 - progress) * (1 - progress);
            shakeRoot.localPosition = restPosition + active.shakePosition * (wave * visualStrength);
            shakeRoot.localRotation = restRotation * Quaternion.Euler(active.shakeRotation * (wave * visualStrength));
            // The collider cabinet rocks too. Toys remain independent world-space bodies.
            Physics.SyncTransforms();
        }

        void Pulse(float amount)
        {
            if (!machine.toyRoot) return;
            // Use the resting cabinet orientation, so subsequent pulses don't inherit its wobble.
            Quaternion rotation = shakeRoot.parent ? shakeRoot.parent.rotation * restRotation : restRotation;
            rotation *= machine.transform.localRotation;
            foreach (var toy in machine.toyRoot.GetComponentsInChildren<ClawToy>())
            {
                var body = toy.Body;
                if (!body || toy.owner != machine || toy.Held || toy.Awarded || body.isKinematic || !body.detectCollisions) continue;
                var local = machine.transform.InverseTransformPoint(toy.Center);
                if (Mathf.Abs(local.x) > .65f || local.y < .25f || local.y > 2.5f || Mathf.Abs(local.z) > .65f) continue;
                body.WakeUp();
                body.AddForce(rotation * active.toyImpulse * (strength * amount), ForceMode.VelocityChange);
                body.AddTorque(rotation * active.toyTorque * (strength * amount), ForceMode.VelocityChange);
            }
        }

        void Restore()
        {
            if (active == null) return;
            if (shakeRoot) { shakeRoot.localPosition = restPosition; shakeRoot.localRotation = restRotation; Physics.SyncTransforms(); }
            active = null;
        }
        void OnDisable() { Restore(); }
        void OnApplicationFocus(bool value) { focused = value; if (!value) Restore(); }
    }
}
