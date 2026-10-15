using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ithappy
{
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class ClawHud : MonoBehaviour
    {
        public ClawMachine machine;
        public Text status, prizes, message;
        public Graphic statusLight;
        public CanvasGroup movement, capture, kick;
        public ClawHudShape[] keycaps;
        public KeyCode[] keys;
        public ClawHudShape mouse;
        public Color keyColor = new Color(1, .93f, .81f), pressedColor = new Color(1, .75f, .32f);
        [Header("Panel appearance")]
        public Color backgroundColor = new Color(.16f, .30f, .55f, .82f);
        [SerializeField, HideInInspector] ClawHudShape[] backgroundPanels;
        public void ConfigurePanelBackgrounds()
        {
            var panels = new List<ClawHudShape>();
            foreach (var shape in GetComponentsInChildren<ClawHudShape>(true))
                if (shape.name == "Status pill" || shape.name == "Prize counter" || shape.name.EndsWith(" hint")) panels.Add(shape);
            backgroundPanels = panels.ToArray();
            ApplyBackgroundColor();
        }
        void ApplyBackgroundColor()
        {
            if (backgroundPanels == null) return;
            foreach (var panel in backgroundPanels) if (panel) panel.color = backgroundColor;
        }
        void OnValidate() { ApplyBackgroundColor(); }
        void Update()
        {
            ApplyBackgroundColor();
            if (!machine) return;
            bool ready = machine.IsReady;
            if (status) status.text = Status(machine.State);
            if (prizes) prizes.text = "Prizes: " + machine.Wins;
            if (statusLight) statusLight.color = ready ? new Color(.5f, 1, .28f) : machine.State == ClawMachine.Phase.PresentingPrize ? new Color(1, .4f, .7f) : new Color(.25f, .9f, 1);
            if (movement) movement.alpha = ready ? 1 : .5f;
            if (capture) capture.alpha = ready ? 1 : .5f;
            if (kick) kick.alpha = machine.State == ClawMachine.Phase.PresentingPrize ? .5f : 1;
            if (message) {
                var result = machine.LastResult;
                message.text = ready && result != "Position the claw" ? result : "";
            }
            if (keycaps != null && keys != null) for (int i = 0; i < Mathf.Min(keycaps.Length, keys.Length); i++)
                if (keycaps[i]) keycaps[i].color = Application.isPlaying && Input.GetKey(keys[i]) ? pressedColor : keyColor;
            if (mouse) {
                var tint = Application.isPlaying && machine.controls && machine.controls.Dragging ? new Color(.25f, .9f, 1) : keyColor;
                if (mouse.borderColor != tint) { mouse.borderColor = tint; mouse.SetVerticesDirty(); }
            }
        }
        static string Status(ClawMachine.Phase state)
        {
            switch (state) {
                case ClawMachine.Phase.Ready: return "Ready to play";
                case ClawMachine.Phase.Descending: return "Lowering claw";
                case ClawMachine.Phase.Closing: return "Grabbing toy";
                case ClawMachine.Phase.Lifting: return "Lifting claw";
                case ClawMachine.Phase.MovingToDrop: return "Delivering toy";
                case ClawMachine.Phase.Releasing: return "Dropping toy";
                case ClawMachine.Phase.WaitingForPrize: return "Waiting for prize";
                case ClawMachine.Phase.PresentingPrize: return "You won!";
                default: return "Returning claw";
            }
        }
    }
}
