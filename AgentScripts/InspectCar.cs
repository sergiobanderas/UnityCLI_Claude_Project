using UnityEngine;
using UnityEditor;
var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Hodaart/LowPoly Vehicle Collection 2/Prefabs/Car 01.prefab");
var sb = new System.Text.StringBuilder();
void Walk(Transform t, int d) {
  sb.Append(new string(' ', d*2) + t.name + " pos=" + t.localPosition + " rot=" + t.localEulerAngles + " scale=" + t.localScale + " [");
  foreach (var c in t.GetComponents<Component>()) sb.Append(c.GetType().Name + ",");
  sb.Append("]\n");
  foreach (Transform ch in t) Walk(ch, d+1);
}
Walk(go.transform, 0);
var b = new Bounds(go.transform.position, Vector3.zero);
foreach (var r in go.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
sb.Append("bounds=" + b);
var cube = GameObject.Find("MyFirstCube");
sb.Append("\ncube pos=" + cube.transform.position + " scale=" + cube.transform.localScale);
return sb.ToString();
