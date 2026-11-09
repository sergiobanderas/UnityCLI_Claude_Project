using UnityEngine;
using UnityEditor.SceneManagement;
var s = EditorSceneManager.OpenPreviewScene("Assets/ithappy/Cartoon_City_Free/Scenes/URP_Scenes/Demonstration.unity");
var sb = new System.Text.StringBuilder();
Bounds BoundsOf(Transform t){ var rs=t.GetComponentsInChildren<Renderer>(); var b=rs[0].bounds; foreach(var r in rs) b.Encapsulate(r.bounds); return b; }
var counts = new System.Collections.Generic.Dictionary<string,int>();
foreach (var root in s.GetRootGameObjects()) {
  if (root.name=="Demonstration"||root.name=="Overview") sb.Append(root.name+" bounds="+BoundsOf(root.transform)+"\n");
  if (root.name=="Main Camera") sb.Append("cam pos="+root.transform.position+" rot="+root.transform.eulerAngles+"\n");
  if (root.name!="Demonstration") continue;
  foreach (Transform ch in root.transform) { var key = System.Text.RegularExpressions.Regex.Replace(ch.name, @"\s*\(\d+\)$",""); counts[key]=counts.TryGetValue(key,out var n)?n+1:1; }
}
foreach (var kv in counts) sb.Append(kv.Key+" x"+kv.Value+"; ");
// Road pieces positions sample
foreach (var root in s.GetRootGameObjects()) if (root.name=="Demonstration") { int i=0; foreach (Transform ch in root.transform) if (ch.name.ToLower().Contains("road") && i++<60) sb.Append("\n"+ch.name+" "+ch.position+" rotY="+ch.eulerAngles.y.ToString("0")+" col="+(ch.GetComponentInChildren<Collider>()?.GetType().Name)); }
EditorSceneManager.ClosePreviewScene(s);
return sb.ToString();
