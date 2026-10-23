using UnityEngine;
using UnityEngine.SceneManagement;
namespace BobbysMods
{
 public sealed partial class Plugin
 {
  bool lowGravity, checkpointArmed, fpsDisplay, hasCheckpoint;
  int menuPage, checkpointScene;
  Vector3 checkpointHead;
  Quaternion checkpointRotation;
  Material platformMaterial;
  float fpsElapsed, fpsRefresh;
  int fpsFrames;
  string fpsText="";
  void FixedUpdate()
  {
   if(!lowGravity || !Allowed)return;
   var player=GorillaLocomotion.GTPlayer.Instance;
   var body=player ? player.GetComponent<Rigidbody>() : null;
   if(body && body.useGravity && !body.isKinematic)
    body.AddForce(-Physics.gravity*ModRules.GravityCompensation(N("gravity_multiplier",.5f,.1f,1)),ForceMode.Acceleration);
  }
  void UpdateExtras(bool lobbyChanged)
  {
   bool save=Pressed("checkpoint_save_button","right_a"), recall=Pressed("checkpoint_return_button","left_y");
   if(hasCheckpoint && SceneManager.GetActiveScene().handle!=checkpointScene)hasCheckpoint=false;
   if(fpsDisplay)
   {
    fpsElapsed+=Time.unscaledDeltaTime;fpsFrames++;
    if(Time.unscaledTime>=fpsRefresh)
    {
     fpsText=ModRules.FramesPerSecond(fpsFrames,fpsElapsed)+" FPS";
     fpsFrames=0;fpsElapsed=0;fpsRefresh=Time.unscaledTime+.5f;
    }
   }
   else {fpsFrames=0;fpsElapsed=0;fpsText="";}
   if(!checkpointArmed || !Allowed || lobbyChanged)return;
   var player=GorillaLocomotion.GTPlayer.Instance;var camera=ViewCamera();
   if(!player || !camera)return;
   if(save)
   {
    checkpointHead=camera.transform.position;checkpointRotation=player.transform.rotation;
    checkpointScene=SceneManager.GetActiveScene().handle;hasCheckpoint=true;message="Checkpoint saved";
   }
   if(recall)
   {
    if(!hasCheckpoint){message="Save a checkpoint with "+S("checkpoint_save_button","right_a");return;}
    TryWarp(checkpointHead,checkpointRotation);
   }
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
   // Check that the checkpoint still has a supporting surface after a map/scene change.
   if(!Physics.Raycast(feet+Vector3.up*.15f,Vector3.down,.6f,mask,QueryTriggerInteraction.Ignore)){message="No ground at destination";return false;}
   player.TeleportTo(headTarget,rotation,false,true);return true;
  }
  bool FeatureEnabled(int index)
  {
   switch(index)
   {
    case 0:return freezeKey.Armed;case 1:return gripPlatforms;case 2:return triggerPlatforms;case 3:return teleport;case 4:return smartArms;
    case 5:return lowGravity;case 6:return checkpointArmed;case 7:return fpsDisplay;default:return false;
   }
  }
  string FeatureLabel(int index)
  {
   switch(index)
   {
    case 0:return "Freeze Keybind: "+S("freeze_button","right_b")+(freezeKey.Armed?" [ARMED]":" [OFF]");
    case 1:return S("label_grip","Grip Platforms");case 2:return S("label_trigger","Trigger Platforms");case 3:return S("label_teleport","Teleport");case 4:return S("label_arms","Smart Arms");
    case 5:return "Low Gravity";case 6:return "Checkpoint Keybinds";case 7:return "FPS Display";case 8:return "Reload Config";default:return "Build / Test Info";
   }
  }
 }
}
