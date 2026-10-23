using System;
namespace BobbysMods
{
 internal sealed class MenuTouch
 {
  bool touching;
  float nextPress;
  internal static int Hit(float x, float y, float z)
  {
   if (Math.Abs(z) > 16 || Math.Abs(x) > 195) return -1;
   int row = (int)Math.Round((260-y)/57f, MidpointRounding.ToEven);
   if(row==7 && Math.Abs(y-(260-row*57))<=24)return x<0?10:7;
   return row >= 1 && row <= 8 && Math.Abs(y-(260-row*57)) <= 24 ? row : -1;
  }
  internal void Release() { touching=false; }
  internal int Enter(int row,float now)
  {
   if(row<0){Release();return -1;}
   if(touching || now<nextPress)return -1;
   touching=true;nextPress=now+.3f;return row;
  }
 }
}

