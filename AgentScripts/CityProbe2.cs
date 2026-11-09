using UnityEngine;
using System.Reflection;
var car = GameObject.Find("PlayerCar");
var ctrl = car.GetComponent<CarController>();
var flags = BindingFlags.NonPublic | BindingFlags.Instance;
var speedF = typeof(CarController).GetField("currentSpeed", flags);
var jumpF = typeof(CarController).GetField("jumpRequested", flags);
var log = new System.Text.StringBuilder();
async System.Threading.Tasks.Task Drive(float s, float speed) { float t = Time.time; while (Time.time - t < s) { speedF.SetValue(ctrl, speed); int f = Time.frameCount; await Wait.Until(() => Time.frameCount > f, 20000); } }
void Snap(string tag) => log.Append($"{tag}: pos={car.transform.position} yaw={car.transform.eulerAngles.y:0}\n");
Snap("start");
await Drive(2f, 12f); Snap("drive 2s");
speedF.SetValue(ctrl, 12f);
jumpF.SetValue(ctrl, true); await Drive(0.35f, 12f); Snap("jump +0.35s");
await Drive(1.5f, 12f); Snap("landed");
await Drive(4f, 12f); Snap("drive 4s more (crossroad/edge?)");
return log.ToString();
