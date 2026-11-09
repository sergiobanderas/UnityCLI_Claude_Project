using UnityEngine;
using UnityEditor.SceneManagement;
var s = EditorSceneManager.OpenPreviewScene("Assets/ithappy/Cartoon_City_Free/Scenes/URP_Scenes/Demonstration.unity");
var sb = new System.Text.StringBuilder();
var b = new Bounds(); bool init=false; int colliders=0, cams=0;
foreach (var root in s.GetRootGameObjects()) {
  sb.Append(root.name + " children=" + root.transform.childCount + " [");
  foreach (var c in root.GetComponents<Component>()) sb.Append(c.GetType().Name+",");
  sb.Append("]\n");
  foreach (var r in root.GetComponentsInChildren<Renderer>()) { if(!init){b=r.bounds;init=true;} else b.Encapsulate(r.bounds);}
  colliders += root.GetComponentsInChildren<Collider>().Length; cams += root.GetComponentsInChildren<Camera>().Length;
}
sb.Append($"bounds={b} colliders={colliders} cams={cams}");
EditorSceneManager.ClosePreviewScene(s);
return sb.ToString();
