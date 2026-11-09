using UnityEngine;
var sb = new System.Text.StringBuilder();
var root = GameObject.Find("Demonstration").transform;
foreach (Transform ch in root) { var n = ch.name; if (n.StartsWith("Car_")||n.StartsWith("Van")||n.StartsWith("Futuristic")||n.StartsWith("Bus")) sb.Append($"{n} {ch.position.x:0.0},{ch.position.z:0.0}; "); }
return sb.ToString();
