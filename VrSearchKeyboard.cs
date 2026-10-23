using UnityEngine;
using UnityEngine.UI;
namespace BobbysMods
{
 internal sealed class VrSearchKeyboard
 {
  internal readonly GameObject Root;
  internal VrSearchKeyboard(Font font,Color background,Color keyColour,Color textColour)
  {
   Root=new GameObject("Bobbys Search Keyboard",typeof(RectTransform),typeof(Canvas),typeof(Image));
   Object.DontDestroyOnLoad(Root);
   var canvas=Root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.overrideSorting=true;canvas.sortingOrder=110;
   var rect=Root.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(740,280);Root.transform.localScale=Vector3.one*.001f;
   var panel=Root.GetComponent<Image>();panel.color=background;panel.raycastTarget=false;
   foreach(var key in SearchKeyboardLayout.Keys)
   {
    var button=new GameObject(key.Label,typeof(RectTransform),typeof(Image));button.transform.SetParent(Root.transform,false);
    var br=button.GetComponent<RectTransform>();br.sizeDelta=new Vector2(key.Width,44);br.anchoredPosition=new Vector2(key.X,key.Y);
    var image=button.GetComponent<Image>();image.color=key.Value=="CLOSE"?new Color(.7f,.1f,.3f):keyColour;image.raycastTarget=false;
    var label=new GameObject("Label",typeof(RectTransform),typeof(Text));label.transform.SetParent(button.transform,false);
    var lr=label.GetComponent<RectTransform>();lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=lr.offsetMax=Vector2.zero;
    var text=label.GetComponent<Text>();text.font=font;text.text=key.Label;text.fontSize=22;text.color=textColour;text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;
   }
   Visible(false);
  }
  internal void Place(Vector3 position,Quaternion rotation){Root.transform.SetPositionAndRotation(position,rotation);}
  internal void Visible(bool visible){if(Root && Root.activeSelf!=visible)Root.SetActive(visible);}
  internal int Hit(Vector3 position){if(!Root || !Root.activeSelf)return -1;var local=Root.transform.InverseTransformPoint(position);return SearchKeyboardLayout.Hit(local.x,local.y,local.z);}
  internal void Destroy(){if(Root)Object.Destroy(Root);}
 }
 public sealed partial class Plugin
 {
  bool searchOpen;
  readonly ModSearch search=new ModSearch();
  readonly int[] searchMatches=new int[8];
  readonly MenuTouch keyboardTouch=new MenuTouch();
  int searchCount,searchPage;
  VrSearchKeyboard searchKeyboard;
  Vector3 searchPanelPosition;
  Quaternion searchPanelRotation;
  void BeginSearch()
  {
   var camera=ViewCamera();if(!camera)return;
   if(searchKeyboard==null)searchKeyboard=new VrSearchKeyboard(menuFont,C("menu_background","#090B1C"),buttonColour,C("menu_text_colour","#FFFFFF"));
   Vector3 forward=Vector3.ProjectOnPlane(camera.transform.forward,Vector3.up).normalized;
   if(forward.sqrMagnitude<.1f)forward=Vector3.forward;
   searchPanelRotation=Quaternion.LookRotation(forward,Vector3.up);
   Vector3 keyboardPosition=camera.transform.position+forward*.6f-Vector3.up*.28f;
   searchKeyboard.Place(keyboardPosition,searchPanelRotation);
   searchPanelPosition=keyboardPosition+Vector3.up*.46f+forward*.1f;
   search.Press("CLEAR");RefreshSearch();searchOpen=true;menuVisible=true;
   touch.Release();keyboardTouch.Release();searchKeyboard.Visible(true);
  }
  void EndSearch(bool closeMenu)
  {
   searchOpen=false;searchKeyboard?.Visible(false);keyboardTouch.Release();touch.Release();
   if(closeMenu){menuVisible=false;SetVisible(false);}
  }
  void RefreshSearch(){searchCount=search.Filter(searchMatches);searchPage=0;}
  int RowFeature(int row)
  {
   if(!searchOpen)return ModRules.FeatureIndex(menuPage,row);
   int index=searchPage*5+row-2;return index>=0 && index<searchCount?searchMatches[index]:-1;
  }
  bool HandleKeyboard()
  {
   if(!searchOpen)return false;
   searchKeyboard.Visible(true);
   int hovered=searchKeyboard.Hit(pointerTouchPosition);
   int selected=keyboardTouch.Enter(hovered,Time.unscaledTime);
   if(hovered>=0)touch.Release();
   if(selected>=0)
   {
    string value=SearchKeyboardLayout.Keys[selected].Value;
    if(search.Press(value)){EndSearch(true);return true;}
    RefreshSearch();return true;
   }
   return hovered>=0;
  }
 }
}
