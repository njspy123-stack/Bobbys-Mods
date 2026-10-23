using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
namespace BobbysMods
{
 public sealed partial class Plugin
 {
  int clipScene;bool clipping;GorillaLocomotion.GTPlayer clipPlayer;int clipMask;Rigidbody clipBody;bool clipGravity;
  Vector3 clipVelocity;float clipRadius,clipHeight;Vector3 clipEntry;Quaternion clipRotation;readonly List<Collider> disabledColliders=new List<Collider>();
  Material depthMaterial;GameObject hud;Text hudText;float nextHud;

  bool ClipPositionClear(Vector3 head)
  {
   int count=Physics.OverlapCapsuleNonAlloc(head-Vector3.up*Mathf.Max(0,clipHeight-clipRadius),head,clipRadius,overlapHits,clipMask,QueryTriggerInteraction.Ignore);
   if(count>=overlapHits.Length)return false;
   for(int i=0;i<count;i++)if(!IsOwnCollider(overlapHits[i]))return false;
   return true;
  }
  void UpdateNoclip()
  {
   bool active=Allowed && !searchOpen && (noclip || noclipFly) && Held(S("noclip_button","right_grip"));
   var player=GorillaLocomotion.GTPlayer.Instance;var camera=ViewCamera();
   if(clipping && (clipScene!=UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle || clipPlayer!=player)){StopNoclip();return;}
   if(!active || !player || !camera){StopNoclip();return;}
   if(!clipping)
   {
    if(!player.bodyCollider || !player.headCollider)return;
    clipScene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle;clipPlayer=player;
    clipMask=ModRules.MovementMask(player.locomotionEnabledLayers.value,GorillaLocomotion.GTPlayer.LocomotionEnabledLayers.value);
    clipRadius=Mathf.Clamp(player.bodyCollider.radius*player.scale,.12f,.4f);
    clipHeight=Mathf.Clamp(camera.transform.position.y-player.bodyCollider.bounds.min.y-.04f,.5f,2.5f);
    // The first fallback must be clear before collision is suspended.
    if(!ClipPositionClear(camera.transform.position)){clipPlayer=null;message="Move clear of geometry before using Noclip";return;}
    clipBody=player.GetComponent<Rigidbody>();clipGravity=clipBody && clipBody.useGravity;
    clipEntry=camera.transform.position;clipRotation=player.transform.rotation;clipVelocity=Vector3.zero;
    foreach(var c in player.GetComponentsInChildren<Collider>(true))if(c && c.enabled && !c.isTrigger){disabledColliders.Add(c);c.enabled=false;}
    clipping=true;
   }
   player.locomotionEnabledLayers=0;
   if(!noclipFly){if(clipBody){clipBody.linearVelocity=Vector3.zero;clipBody.angularVelocity=Vector3.zero;if(S("noclip_hover","true")=="true")clipBody.useGravity=false;}return;}
   if(clipBody){clipBody.useGravity=false;clipBody.linearVelocity=Vector3.zero;clipBody.angularVelocity=Vector3.zero;}
   if(ClipPositionClear(camera.transform.position)){clipEntry=camera.transform.position;clipRotation=player.transform.rotation;}
   Vector2 stick=LeftStick();
   float deadzone=N("noclip_deadzone",.18f,.05f,.5f);
   if(stick.magnitude<deadzone)stick=Vector2.zero;
   Vector3 forward=camera.transform.forward;
   if(noclipFly && HandPose(XRNode.RightHand,out Vector3 pos,out Quaternion aim))forward=aim*Vector3.forward;
   if(!noclipFly)forward=Vector3.ProjectOnPlane(forward,Vector3.up).normalized;
   Vector3 right=Vector3.Cross(Vector3.up,forward).normalized;
   Vector3 direction=forward*stick.y+right*stick.x;
   if(noclipFly)
   {
    if(S("noclip_fly_auto_forward","false")=="true")direction+=forward;
    if(Held(S("noclip_up_button","left_trigger")))direction+=Vector3.up;
    if(Held(S("noclip_down_button","left_grip")))direction+=Vector3.down;
   }
   Vector3 target=Vector3.ClampMagnitude(direction,1)*N("noclip_fly_speed",7,.2f,15);
   // Stop immediately on neutral input; ease acceleration only while a direction is requested.
   clipVelocity=target.sqrMagnitude<.0001f?Vector3.zero:Vector3.MoveTowards(clipVelocity,target,N("noclip_acceleration",20,1,50)*Time.fixedDeltaTime);
   if(clipVelocity.sqrMagnitude>.0001f)player.TeleportTo(camera.transform.position+clipVelocity*Time.fixedDeltaTime,player.transform.rotation,false,true);
  }
  void StopNoclip()
  {
   if(!clipping)return;
   try
   {
    var camera=ViewCamera();
    if(clipPlayer && camera && clipScene==UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle && !ClipPositionClear(camera.transform.position))
    {
     clipPlayer.TeleportTo(clipEntry,clipRotation,false,true);message="Returned to last clear Noclip position";
    }
   }
   finally
   {
    if(clipPlayer)clipPlayer.locomotionEnabledLayers=clipMask;
    foreach(var c in disabledColliders)if(c)c.enabled=true;
    disabledColliders.Clear();if(clipBody){clipBody.useGravity=clipGravity;}
    clipVelocity=Vector3.zero;clipping=false;clipPlayer=null;clipBody=null;
   }
  }
  void BuildDepth(Transform parent,Vector3 position,Vector3 size,Color tint)
  {
   if(!depthMaterial)depthMaterial=VisualMaterial.Transparent("Bobbys neon depth",tint);
   var box=GameObject.CreatePrimitive(PrimitiveType.Cube);box.name="Neon 3D bezel";box.layer=2;
   var collider=box.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
   box.transform.SetParent(parent,false);box.transform.localPosition=position;box.transform.localScale=size;
   var renderer=box.GetComponent<Renderer>();renderer.sharedMaterial=depthMaterial;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
  }
  void UpdateHud()
  {
   bool visible=(fpsDisplay || hudStatus || Extra(17)) && S("hud_enabled","true")=="true";
   var camera=ViewCamera();if(!visible || !camera){if(hud)hud.SetActive(false);return;}
   if(!hud)
   {
    hud=new GameObject("Bobbys headset HUD",typeof(RectTransform),typeof(Canvas));DontDestroyOnLoad(hud);
    var canvas=hud.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.overrideSorting=true;canvas.sortingOrder=250;
    hud.GetComponent<RectTransform>().sizeDelta=new Vector2(360,210);
    var label=new GameObject("HUD text",typeof(RectTransform),typeof(Text));label.transform.SetParent(hud.transform,false);
    var rect=label.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
    hudText=label.GetComponent<Text>();hudText.font=menuFont;hudText.raycastTarget=false;hudText.supportRichText=false;
   }
   if(!hud.activeSelf)hud.SetActive(true);if(hud.transform.parent!=camera.transform)hud.transform.SetParent(camera.transform,false);
   float distance=N("hud_distance",1,.5f,3), panelWidth=2*distance*Mathf.Tan(N("hud_width_degrees",18,10,28)*.5f*Mathf.Deg2Rad);
   hud.transform.localPosition=new Vector3(Mathf.Tan(N("hud_angle_x",-12,-18,18)*Mathf.Deg2Rad)*distance,Mathf.Tan(N("hud_angle_y",10,-15,15)*Mathf.Deg2Rad)*distance,distance);
   hud.transform.localRotation=Quaternion.identity;hud.transform.localScale=Vector3.one*(panelWidth/360)*N("hud_scale",1,.5f,1.2f);
   if(Time.unscaledTime<nextHud)return;nextHud=Time.unscaledTime+.5f;
   hudText.fontSize=(int)N("hud_font_size",22,14,36);hudText.alignment=TextAnchor.UpperLeft;hudText.color=C("hud_colour","#39FF88");
   string value=fpsDisplay?fpsText:"";
   if(hudStatus)
   {
    if(S("hud_show_mode","true")=="true")value+="\n"+(Allowed?"MODS READY":"MODS LOCKED");
    if(S("hud_show_active_count","true")=="true"){int count=0;for(int i=0;i<18;i++)if(FeatureEnabled(i))count++;value+="\nActive: "+count;}
    if(S("hud_show_freeze","true")=="true")value+="\nGhost: "+(frozen?"FROZEN":freezeKey.Armed?"ARMED":"OFF");
   }
   if(Extra(17))value+="\nSession: "+System.TimeSpan.FromSeconds(Time.unscaledTime-sessionStarted).ToString(@"mm\:ss");
   if(hudText.text!=value)hudText.text=value;
  }
  void DestroyExtras(){StopNoclip();if(hud)Destroy(hud);if(depthMaterial)Destroy(depthMaterial);}
 }
}
