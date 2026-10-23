using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;
namespace BobbysMods
{
 public sealed partial class Plugin
 {
  const int ExtraFeatureMask=(1<<9)|(1<<10)|(1<<11)|(1<<13)|(1<<14)|(1<<15)|(1<<16)|(1<<17);
  bool fingerSupported;int extraMask;float sessionStarted;bool movementSaved;GorillaLocomotion.GTPlayer modifiedPlayer;
  float originalJump,originalMaxJump,originalSlide;readonly FingerLock fingerLock=new FingerLock();
  readonly List<Transform> fingerBones=new List<Transform>();readonly List<Quaternion> fingerRotations=new List<Quaternion>();VRRig fingerRig;
  bool Extra(int feature)=>(extraMask&(1<<feature))!=0;
  VRRig LocalRig(){var tagger=GorillaTagger.Instance;return tagger?OfflineRigField?.GetValue(tagger) as VRRig:null;}
  void PatchFingerAnimation()
  {
   sessionStarted=Time.unscaledTime;
   harmony.Patch(AccessTools.DeclaredMethod(typeof(GorillaLocomotion.GTPlayer),"FixedUpdate"),postfix:new HarmonyMethod(typeof(Plugin),nameof(AfterPlayerPhysics)));
   harmony.Patch(AccessTools.DeclaredMethod(typeof(GorillaLocomotion.GTPlayer),"CollisionsSphereCast"),prefix:new HarmonyMethod(typeof(Plugin),nameof(NoclipSphereCast)));
   harmony.Patch(AccessTools.DeclaredMethod(typeof(GorillaLocomotion.GTPlayer),"NonAllocRaycast"),prefix:new HarmonyMethod(typeof(Plugin),nameof(NoclipRaycast)));
   var late=AccessTools.DeclaredMethod(typeof(VRRig),"PostTick");
   if(late!=null){try{harmony.Patch(late,postfix:new HarmonyMethod(typeof(Plugin),nameof(KeepFingersStill)));fingerSupported=true;}catch(System.Exception e){Logger.LogWarning("Finger lock unavailable: "+e.Message);}}
  }
  static void KeepFingersStill(VRRig __instance)
  {
   var plugin=Current;if(!plugin || !plugin.Allowed || !plugin.Extra(9) || __instance!=plugin.LocalRig())return;
   plugin.MaintainFingers(__instance);
  }
  void ClearFingerLock(){fingerLock.Reset();fingerRig=null;fingerBones.Clear();fingerRotations.Clear();}
  void MaintainFingers(VRRig rig)
  {
   if(fingerRig!=rig)
   {
    ClearFingerLock();fingerRig=rig;
    object[] maps={rig.leftThumb,rig.leftIndex,rig.leftMiddle,rig.rightThumb,rig.rightIndex,rig.rightMiddle};
    foreach(var map in maps)if(map!=null)for(int i=1;i<=3;i++)
    {
     var field=AccessTools.Field(map.GetType(),"fingerBone"+i);var bone=field?.GetValue(map) as Transform;
     if(bone){fingerBones.Add(bone);fingerRotations.Add(bone.localRotation);}
    }
   }
   for(int i=0;i<fingerBones.Count;i++)if(fingerBones[i])fingerBones[i].localRotation=fingerRotations[i];
  }
  Vector2 LeftStick()
  {
   var poller=ControllerInputPoller.instance;
   if(poller)return poller.leftControllerPrimary2DAxis;
   InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primary2DAxis,out Vector2 value);return value;
  }
  static void AfterPlayerPhysics(GorillaLocomotion.GTPlayer __instance)
  {
   var plugin=Current;if(plugin && plugin.isActiveAndEnabled && __instance==GorillaLocomotion.GTPlayer.Instance)plugin.ApplyPhysicsFeatures();
  }
  static bool NoclipSphereCast(GorillaLocomotion.GTPlayer __instance,Vector3 __0,Vector3 __2,ref Vector3 __3,ref RaycastHit __4,ref bool __result)
  {
   var plugin=Current;if(!plugin || !plugin.isActiveAndEnabled || !plugin.Allowed || !plugin.clipping || __instance!=GorillaLocomotion.GTPlayer.Instance)return true;
   __3=__0+__2;__4=default;__result=false;return false;
  }
  static bool NoclipRaycast(GorillaLocomotion.GTPlayer __instance,ref int __result)
  {
   var plugin=Current;if(!plugin || !plugin.isActiveAndEnabled || !plugin.Allowed || !plugin.clipping || __instance!=GorillaLocomotion.GTPlayer.Instance)return true;
   __result=0;return false;
  }
  void RestoreMovement()
  {
   if(movementSaved && modifiedPlayer){modifiedPlayer.jumpMultiplier=originalJump;modifiedPlayer.maxJumpSpeed=originalMaxJump;modifiedPlayer.slideControl=originalSlide;}
   movementSaved=false;modifiedPlayer=null;
  }
  void UpdateMovement()
  {
   var player=GorillaLocomotion.GTPlayer.Instance;var camera=ViewCamera();
   if(!Allowed || !player){RestoreMovement();return;}
   bool modifiers=Extra(11)||Extra(13)||Extra(16);
   if(movementSaved && modifiedPlayer!=player)RestoreMovement();
   if(modifiers && !movementSaved){modifiedPlayer=player;originalJump=player.jumpMultiplier;originalMaxJump=player.maxJumpSpeed;originalSlide=player.slideControl;movementSaved=true;}
   if(modifiers){player.jumpMultiplier=Extra(11)?originalJump*N("speed_boost_multiplier",1.3f,1,2):originalJump;player.maxJumpSpeed=Extra(13)?originalMaxJump*N("high_jump_multiplier",1.4f,1,2):originalMaxJump;player.slideControl=Extra(16)?N("slide_control",1,0,1):originalSlide;}else RestoreMovement();
   var body=player.GetComponent<Rigidbody>();if(!body || body.isKinematic || clipping)return;
   bool fly=Extra(10) && !searchOpen && Held(S("fly_button","right_a"));
   if(body.useGravity && (Extra(14)||fly))body.AddForce(-Physics.gravity,ForceMode.Acceleration);
   if(fly && camera)
   {
    Vector2 stick=LeftStick();
    Vector3 direction=camera.transform.forward*stick.y+camera.transform.right*stick.x;
    if(stick.sqrMagnitude<.04f && S("fly_auto_forward","true")=="true")direction=camera.transform.forward;
    if(Held(S("fly_up_button","left_trigger")))direction+=Vector3.up;
    if(Held(S("fly_down_button","left_grip")))direction-=Vector3.up;
    Vector3 target=Vector3.ClampMagnitude(direction,1)*N("fly_speed",3,.2f,10);
    body.linearVelocity=Vector3.MoveTowards(body.linearVelocity,target,N("fly_acceleration",10,1,30)*Time.fixedDeltaTime);
   }
   if(Extra(15) && !menuVisible && !searchOpen)for(int i=0;i<2;i++)
   {
    string hand=i==0?"left":"right";
    if(Held(S("ironman_"+hand+"_button",hand+"_grip")) && HandPose(i==0?XRNode.LeftHand:XRNode.RightHand,out Vector3 pos,out Quaternion rotation))body.AddForce(-(rotation*Vector3.up)*N("ironman_acceleration",8,1,20),ForceMode.Acceleration);
   }
  }
 }
}
