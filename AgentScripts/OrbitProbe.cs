using UnityEngine;
using System.Reflection;
using Unity.Cinemachine;
Application.runInBackground = true;
var car = GameObject.Find("PlayerCar");
var ctrl = car.GetComponent<CarController>();
var orbit = GameObject.Find("ThirdPersonCamera").GetComponent<CinemachineOrbitalFollow>();
var cam = Camera.main.transform;
var fl = BindingFlags.NonPublic | BindingFlags.Instance;
var spd = typeof(CarController).GetField("currentSpeed", fl);
var turn = typeof(CarController).GetField("turnInput", fl);
var log = new System.Text.StringBuilder();
async System.Threading.Tasks.Task Hold(float secs, float speed, float t) { float s = Time.time; while (Time.time - s < secs) { if (speed != 0) spd.SetValue(ctrl, speed); turn.SetValue(ctrl, t); int f = Time.frameCount; await Wait.Until(() => Time.frameCount > f, 20000); } }
// Angle of camera around the car, 0 = directly behind.
float CamAngle() { var d = car.transform.position - cam.position; d.y = 0; return Vector3.SignedAngle(car.transform.forward, d, Vector3.up); }
void Snap(string tag) => log.Append($"{tag}: carYaw={car.transform.eulerAngles.y:0} speed={ctrl.CurrentSpeed:0.0} camAngleFromBehind={CamAngle():0}\n");
await Hold(0.5f, 0, 0); Snap("start");
await Hold(1f, 0, 1); Snap("D while stopped (should not turn)");
await Hold(1f, 10, 1); Snap("D at speed 10 for 1s");
await Hold(1f, 10, -1); Snap("A at speed 10 for 1s");
await Hold(1f, -5, 1); Snap("D in reverse for 1s (inverted)");
await Hold(1.5f, 0, 0);
orbit.HorizontalAxis.Value = 90f; await Hold(0.3f, 0, 0); Snap("mouse orbit to 90");
await Hold(3.5f, 0, 0); Snap("after 3.5s (recentered?)");
Application.runInBackground = false;
return log.ToString();
