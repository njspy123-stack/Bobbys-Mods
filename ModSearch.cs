using System;
using System.Collections.Generic;
namespace BobbysMods
{
 internal sealed class ModSearch
 {
  internal string Query {get;private set;}="";
  internal const int MaxLength=32;
  static readonly string[] Keywords={
   "freeze avatar ghost frozen keybind", "grip platforms platform", "trigger platforms platform", "teleport tp aim warp", "smart arms long reach", "low gravity float", "grip noclip phase walls", "fps display frames performance", "noclip fly flight phase", "no finger movement fingers lock", "fly flight", "speed boost", "hud status display", "high jump", "zero gravity", "iron man thruster", "slide control ice", "session timer time display"
  };
  internal bool Press(string key)
  {
   if(key=="CLOSE")return true;
   if(key=="CLEAR"){Query="";return false;}
   if(key=="BACK"){if(Query.Length>0)Query=Query.Substring(0,Query.Length-1);return false;}
   string value=key=="SPACE"?" ":key;
   if(value!=null && value.Length==1 && (value==" " || char.IsLetter(value[0])) && Query.Length<MaxLength)Query+=value.ToUpperInvariant();
   return false;
  }
  internal int Filter(int[] matches)
  {
   var terms=Query.Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);
   int count=0;
   for(int i=0;i<Keywords.Length;i++)
   {
    bool match=true;
    foreach(var term in terms)if(Keywords[i].IndexOf(term,StringComparison.OrdinalIgnoreCase)<0){match=false;break;}
    if(match && count<matches.Length)matches[count++]=i;
   }
   return count;
  }
 }
 internal readonly struct SearchKey
 {
  internal readonly string Value,Label;
  internal readonly float X,Y,Width;
  internal SearchKey(string value,string label,float x,float y,float width){Value=value;Label=label;X=x;Y=y;Width=width;}
 }
 internal static class SearchKeyboardLayout
 {
  internal static readonly SearchKey[] Keys=Create();
  static SearchKey[] Create()
  {
   var keys=new List<SearchKey>();
   string[] rows={"QWERTYUIOP","ASDFGHJKL","ZXCVBNM"};
   for(int row=0;row<rows.Length;row++)for(int col=0;col<rows[row].Length;col++)keys.Add(new SearchKey(rows[row][col].ToString(),rows[row][col].ToString(),(col-(rows[row].Length-1)*.5f)*64,70-row*52,56));
   keys.Add(new SearchKey("SPACE","SPACE",-230,-88,180));keys.Add(new SearchKey("BACK","BACK",-70,-88,110));
   keys.Add(new SearchKey("CLEAR","CLEAR",70,-88,110));keys.Add(new SearchKey("CLOSE","X CLOSE",230,-88,130));return keys.ToArray();
  }
  internal static int Hit(float x,float y,float z)
  {
   if(Math.Abs(z)>22)return -1;
   for(int i=0;i<Keys.Length;i++)if(Math.Abs(x-Keys[i].X)<=Keys[i].Width*.5f && Math.Abs(y-Keys[i].Y)<=22)return i;
   return -1;
  }
 }
}
