namespace BobbysMods
{
 internal static class MenuCatalog
 {
  internal static readonly string[] Names={"Movement","Platforms","Avatar","Displays","Settings"};
  static readonly int[][] Features={new[]{3,5,6,8,10,11,13,14,15,16},new[]{1,2},new[]{0,4,9},new[]{7,12,17},new[]{18,20,19}};
  internal static int Pages(int category)=>category<0?1:(Features[category].Length+4)/5;
  internal static int Move(int page,int delta,int pages)=>pages<1?0:(page+delta+pages)%pages;
  internal static int Feature(int category,int page,int slot)
  {
   if(slot<0 || slot>=5)return -1;
   if(category<0)return 100+slot;
   if(category>=Features.Length)return -1;
   int index=page*5+slot;return index>=0 && index<Features[category].Length?Features[category][index]:-1;
  }
 }
 internal sealed class FingerLock
 {
  bool captured;int fingers;
  internal int Apply(int packed){if(!captured){fingers=packed%1000000;captured=true;}return packed-packed%1000000+fingers;}
  internal void Reset(){captured=false;fingers=0;}
 }
}
