using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
Application.runInBackground = true;
var car = GameObject.Find("Car01");
var ctrl = car.GetComponent<CarController>();
var cam = Camera.main.transform;
var kb = Keyboard.current; var mouse = Mouse.current;
var log = new System.Text.StringBuilder();
async System.Threading.Tasks.Task Sec(float s) { float t = Time.time; await Wait.Until(() => Time.time - t >= s, 20000); }
void Snap(string tag) => log.Append($"{tag}: pos={car.transform.position} yaw={car.transform.eulerAngles.y:0} speed={ctrl.CurrentSpeed:0.0} camYaw={cam.eulerAngles.y:0}\n");
await Sec(0.5f); Snap("start");
InputSystem.QueueStateEvent(kb, new KeyboardState(Key.W)); await Sec(1.5f); Snap("W 1.5s");
// Steer right: mouse delta +x for ~0.5s while holding W
float t0 = Time.time;
while (Time.time - t0 < 0.5f) { InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(40, 0) }); int f = Time.frameCount; await Wait.Until(() => Time.frameCount > f); }
InputSystem.QueueStateEvent(mouse, new MouseState()); await Sec(1f); Snap("steer+1s");
InputSystem.QueueStateEvent(kb, new KeyboardState(Key.W, Key.Space)); await Sec(0.1f);
InputSystem.QueueStateEvent(kb, new KeyboardState(Key.W)); await Sec(0.25f); Snap("jump1");
InputSystem.QueueStateEvent(kb, new KeyboardState(Key.W, Key.Space)); await Sec(0.1f);
InputSystem.QueueStateEvent(kb, new KeyboardState(Key.W)); await Sec(0.25f); Snap("jump2");
InputSystem.QueueStateEvent(kb, new KeyboardState(Key.S)); await Sec(1.5f); Snap("brake 1.5s");
InputSystem.QueueStateEvent(kb, new KeyboardState());
return log.ToString();
