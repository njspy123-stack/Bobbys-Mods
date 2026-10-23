using UnityEngine;
using UnityEngine.Rendering;
namespace BobbysMods
{
 internal static class VisualMaterial
 {
  internal static Material Transparent(string name,Color colour)
  {
   var shader=Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default") ?? Shader.Find("GUI/Text Shader");
   var material=new Material(shader){name=name,renderQueue=(int)RenderQueue.Transparent};
   if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",colour);
   if(material.HasProperty("_Color"))material.SetColor("_Color",colour);
   if(material.HasProperty("_Surface"))material.SetFloat("_Surface",1);
   if(material.HasProperty("_Blend"))material.SetFloat("_Blend",0);
   if(material.HasProperty("_SrcBlend"))material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);
   if(material.HasProperty("_DstBlend"))material.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
   if(material.HasProperty("_ZWrite"))material.SetFloat("_ZWrite",0);
   if(material.HasProperty("_Cull"))material.SetFloat("_Cull",(float)CullMode.Off);
   material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.SetOverrideTag("RenderType","Transparent");return material;
  }
 }
}
