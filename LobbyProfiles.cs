using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
namespace BobbysMods
{
 internal sealed class LobbyProfiles
 {
  readonly Dictionary<string,int> masks=new Dictionary<string,int>(StringComparer.Ordinal);
  internal static string Key(string room,string mode)=>Convert.ToBase64String(Encoding.UTF8.GetBytes(room))+"."+Convert.ToBase64String(Encoding.UTF8.GetBytes(mode));
  internal int Get(string key)=>key!=null && masks.TryGetValue(key,out int value)?value:0;
  internal void Set(string key,int mask){if(key!=null)masks[key]=mask&255;}
  internal void Read(string text)
  {
   masks.Clear();
   foreach(string raw in text.Split('\n'))
   {
    string line=raw.Trim();if(line.Length==0 || line.StartsWith("#"))continue;
    int split=line.LastIndexOf('=');if(split<=0)continue;
    if(int.TryParse(line.Substring(split+1),NumberStyles.Integer,CultureInfo.InvariantCulture,out int value) && value>=0 && value<=255)masks[line.Substring(0,split)]=value;
   }
  }
  internal string Write()
  {
   var text=new StringBuilder("# Bobbys Mods saved lobby selections. Do not store active frozen poses.\n");
   foreach(var pair in masks)text.Append(pair.Key).Append('=').Append(pair.Value.ToString(CultureInfo.InvariantCulture)).Append('\n');
   return text.ToString();
  }
 }
}

