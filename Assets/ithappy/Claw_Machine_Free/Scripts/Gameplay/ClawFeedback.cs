using UnityEngine;
namespace ithappy {
 public sealed class ClawFeedback : MonoBehaviour {
  public AudioSource effects,motor,music; public AudioClip button,horizontal,vertical,fingers,win,miss,background,toyDrop,toyStuck;
  public Light[] lamps; public Color idleColor=new Color(1,.7f,.3f),activeColor=Color.cyan,winColor=new Color(1,.3f,.65f);
  public float intensity=.7f,sequenceSpeed=3;
  public void Play(AudioClip c) {if(c && effects) effects.PlayOneShot(c);}
  public void Motor(AudioClip c,bool moving) {if(!motor)return;if(!moving || !c){motor.Stop();return;}if(motor.clip!=c){motor.Stop();motor.clip=c;}if(!motor.isPlaying)motor.Play();}
  void Start() {if(background && music){music.clip=background;music.Play();}}
  public void Animate(ClawMachine.Phase phase) {for(int i=0;i<lamps.Length;i++) if(lamps[i]) {lamps[i].color=phase==ClawMachine.Phase.Ready?idleColor:phase==ClawMachine.Phase.PresentingPrize?winColor:activeColor;lamps[i].intensity=intensity*(.35f+.65f*(.5f+.5f*Mathf.Sin(Time.time*sequenceSpeed-i*1.2f)));}}
  void OnDisable() {if(motor)motor.Stop();if(music)music.Stop();}
 }
}
