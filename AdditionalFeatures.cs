using UnityEngine;
namespace BobbysMods
{
 public sealed partial class Plugin
 {
  bool lowGravity,fpsDisplay,noclip,noclipFly,hudStatus;
  int menuPage,categoryIndex=-1;
  Material platformMaterial;
  float fpsElapsed,fpsRefresh;int fpsFrames;string fpsText="";
  void ApplyPhysicsFeatures()
  {
   UpdateNoclip();
   UpdateMovement();
   if(!lowGravity || Extra(14) || !Allowed || clipping || Extra(10))return;
   var player=GorillaLocomotion.GTPlayer.Instance;var body=player?player.GetComponent<Rigidbody>():null;
   if(body && body.useGravity && !body.isKinematic)body.AddForce(-Physics.gravity*ModRules.GravityCompensation(N("gravity_multiplier",.5f,.1f,1)),ForceMode.Acceleration);
  }
  void UpdateExtras(bool lobbyChanged)
  {
   if(fpsDisplay){fpsElapsed+=Time.unscaledDeltaTime;fpsFrames++;if(Time.unscaledTime>=fpsRefresh){fpsText=ModRules.FramesPerSecond(fpsFrames,fpsElapsed)+" FPS";fpsFrames=0;fpsElapsed=0;fpsRefresh=Time.unscaledTime+.5f;}}
   else {fpsFrames=0;fpsElapsed=0;fpsText="";}
  }
  bool TryWarp(Vector3 headTarget,Quaternion rotation)
  {
   var player=GorillaLocomotion.GTPlayer.Instance;var camera=ViewCamera();
   if(!player || !camera || !player.bodyCollider)return false;
   int mask=ModRules.MovementMask(player.locomotionEnabledLayers.value,GorillaLocomotion.GTPlayer.LocomotionEnabledLayers.value);
   float height=Mathf.Clamp(camera.transform.position.y-player.bodyCollider.bounds.min.y,.6f,2);
   float radius=Mathf.Clamp(player.bodyCollider.radius*player.scale,.12f,.35f);
   Vector3 feet=headTarget-Vector3.up*height;
   int count=Physics.OverlapCapsuleNonAlloc(feet+Vector3.up*(radius+.02f),headTarget,radius,overlapHits,mask,QueryTriggerInteraction.Ignore);
   bool blocked=count>=overlapHits.Length;
   for(int i=0;i<count;i++)if(!IsOwnCollider(overlapHits[i])){blocked=true;break;}
   if(blocked){message="Destination is blocked";return false;}
   // A teleport destination must still have a supporting surface.
   if(!Physics.Raycast(feet+Vector3.up*.15f,Vector3.down,.6f,mask,QueryTriggerInteraction.Ignore)){message="No ground at destination";return false;}
   player.TeleportTo(headTarget,rotation,false,true);return true;
  }

  bool FeatureEnabled(int index)=>(SelectionMask()&(1<<index))!=0 && index<18;
  string FeatureLabel(int index)
  {
   switch(index)
   {
    case 0:return "Freeze Keybind: "+S("freeze_button","right_b")+(freezeKey.Armed?" [ARMED]":" [OFF]");
    case 1:return S("label_grip","Grip Platforms");case 2:return S("label_trigger","Trigger Platforms");case 3:return S("label_teleport","Teleport");case 4:return S("label_arms","Smart Arms");
    case 5:return "Low Gravity";case 6:return "Grip Noclip (matinence)";case 7:return "FPS Display";case 8:return "Noclip Fly (matinence)";
    case 9:return "No Finger Movement";case 10:return "Fly";case 11:return "Speed Boost";case 12:return "HUD Status";case 13:return "High Jump";case 14:return "Zero Gravity";case 15:return "Iron Man";case 16:return "Slide Control";case 17:return "Session Timer";case 18:return "Reload Config";case 20:return "Disable All";default:return "Build / Test Info";
   }
  }
 }
}
