using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace BobbysMods
{
 internal sealed class FrozenAvatar
 {
  GameObject root;
  readonly List<Mesh> meshes=new List<Mesh>();
  internal void Show(Component rig)
  {
   Clear(); var avatar=rig as VRRig;
   if(!avatar || !avatar.mainSkin || !avatar.mainSkin.sharedMesh)return;
   var source=avatar.mainSkin;
   var mesh=new Mesh { name="Bobbys frozen main skin" };
   try
   {
    source.BakeMesh(mesh,false);
    // BakeMesh returns renderer-local vertices. Move them into an unscaled world-aligned root once.
    Vector3 anchor=rig.transform.position;
    var matrix=source.transform.localToWorldMatrix;
    var vertices=mesh.vertices;
    for(int i=0;i<vertices.Length;i++)vertices[i]=matrix.MultiplyPoint3x4(vertices[i])-anchor;
    mesh.vertices=vertices;
    var normals=mesh.normals;var normalMatrix=matrix.inverse.transpose;
    for(int i=0;i<normals.Length;i++)normals[i]=normalMatrix.MultiplyVector(normals[i]).normalized;
    if(normals.Length==vertices.Length)mesh.normals=normals;
    mesh.RecalculateBounds();
    float scale=Mathf.Max(.25f,avatar.scaleFactor);
    if(!ModRules.AvatarBoundsValid(mesh.bounds.size.magnitude,scale))throw new InvalidOperationException("Frozen main-skin bounds are invalid; snapshot suppressed");
    root=new GameObject("Bobbys frozen main avatar - local only");
    root.transform.position=anchor;UnityEngine.Object.DontDestroyOnLoad(root);
    var part=new GameObject("Frozen gorilla skin",typeof(MeshFilter),typeof(MeshRenderer));
    part.transform.SetParent(root.transform,false);
    part.GetComponent<MeshFilter>().sharedMesh=mesh;meshes.Add(mesh);
    var renderer=part.GetComponent<MeshRenderer>();renderer.sharedMaterials=source.sharedMaterials;
    var block=new MaterialPropertyBlock();source.GetPropertyBlock(block);renderer.SetPropertyBlock(block);
    renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
   }
   catch { if(!meshes.Contains(mesh))UnityEngine.Object.Destroy(mesh);Clear();throw; }
  }
  internal void Clear()
  {
   if(root)UnityEngine.Object.Destroy(root);root=null;
   foreach(var mesh in meshes)if(mesh)UnityEngine.Object.Destroy(mesh);
   meshes.Clear();
  }
 }
}

