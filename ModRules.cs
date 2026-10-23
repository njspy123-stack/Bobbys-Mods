namespace BobbysMods
{
 internal static class ModRules
 {
  internal static bool AvatarBoundsValid(float size,float scale)=>!float.IsNaN(size) && !float.IsInfinity(size) && size>0 && size<=8*scale;
  internal static float GravityCompensation(float multiplier)=>1-System.Math.Max(.1f,System.Math.Min(1f,multiplier));
  internal static int FramesPerSecond(int frames,float seconds)=>seconds>0?(int)System.Math.Round(frames/seconds):0;
  internal static int FeatureIndex(int page,int row)=>page*5+(row-2);
  internal static int MovementMask(int active,int fallback)=>active!=0 ? active : fallback;
  internal static bool AnalogPressed(float amount)=>amount>=.55f;
  internal static int SolidLayer(int mask)
  {
   for(int layer=0;layer<32;layer++)if(layer!=2 && (mask&(1<<layer))!=0)return layer;
   return -1;
  }
 }
 internal sealed class FreezeKey
 {
  internal bool Armed { get; private set; }
  internal void Toggle(){Armed=!Armed;}
  internal void SetArmed(bool value){Armed=value;}
  internal void Reset(){Armed=false;}
  internal bool Respond(bool pressed)=>Armed && pressed;
 }
}



