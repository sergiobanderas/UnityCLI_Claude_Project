using UnityEngine;
var rb = GameObject.Find("MyFirstCube").GetComponent<Rigidbody>();
var cam = Camera.main.transform;
var log = new System.Text.StringBuilder();
// Teleport-slide the cube +6 on X over ~1s, then sample the camera while it catches up.
float start = Time.time;
while (Time.time - start < 1f)
{
    rb.position += new Vector3(6f * Time.deltaTime, 0f, 0f);
    int f = Time.frameCount; await Wait.Until(() => Time.frameCount > f);
}
for (int i = 0; i < 4; i++)
{
    log.Append($"t+{i*0.5f:0.0}s cube.x={rb.position.x:0.00} cam.x={cam.position.x:0.00} | ");
    float t = Time.time; await Wait.Until(() => Time.time - t >= 0.5f);
}
return log.ToString();
