using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor builder: replaces the cube with the Car 01 prefab and retargets the camera.
public static class SetupCar
{
    private const string CarPrefabPath = "Assets/Hodaart/LowPoly Vehicle Collection 2/Prefabs/Car 01.prefab";

    public static string Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity", OpenSceneMode.Single);

        // 1. Spawn the car where the cube was (on the ground).
        var cube = GameObject.Find("MyFirstCube");
        Vector3 spawn = cube != null ? new Vector3(cube.transform.position.x, 0.05f, cube.transform.position.z) : new Vector3(0f, 0.05f, 0f);

        var car = GameObject.Find("Car01");
        if (car == null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CarPrefabPath);
            if (prefab == null) return "Car prefab not found";
            car = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            car.name = "Car01";
            Undo.RegisterCreatedObjectUndo(car, "Create Car01");
        }
        car.transform.SetPositionAndRotation(spawn, Quaternion.identity);
        car.tag = "Player";

        // 2. Box collider fitted to the visual bounds (the prefab has none).
        var bounds = new Bounds(car.transform.position, Vector3.zero);
        foreach (var r in car.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
        if (!car.TryGetComponent(out BoxCollider box)) box = car.AddComponent<BoxCollider>();
        box.center = car.transform.InverseTransformPoint(bounds.center);
        box.size = bounds.size;

        // 3. Rigidbody + controller (Reset() runs on AddComponent and sets defaults).
        if (!car.TryGetComponent(out Rigidbody rb)) rb = car.AddComponent<Rigidbody>();
        rb.mass = 1000f;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        if (car.GetComponent<CarController>() == null) car.AddComponent<CarController>();

        // 4. Remove the old cube.
        if (cube != null) Object.DestroyImmediate(cube);

        // 5. Camera: follow behind the car and turn with it (smoothly).
        var camGo = GameObject.Find("ThirdPersonCamera");
        if (camGo == null) return "ThirdPersonCamera not found";
        var cmCam = camGo.GetComponent<CinemachineCamera>();
        cmCam.Target.TrackingTarget = car.transform;

        var follow = camGo.GetComponent<CinemachineFollow>();
        follow.FollowOffset = new Vector3(0f, 4.5f, -9f);
        var tracker = follow.TrackerSettings;
        tracker.BindingMode = BindingMode.LockToTargetWithWorldUp; // Stay behind the car when it turns.
        tracker.PositionDamping = new Vector3(0.3f, 0.8f, 0.4f);
        tracker.RotationDamping = new Vector3(0f, 0.6f, 0f); // Yaw lag: the camera swings smoothly into turns.
        follow.TrackerSettings = tracker;

        var composer = camGo.GetComponent<CinemachineRotationComposer>();
        composer.TargetOffset = new Vector3(0f, 1.2f, 0f);

        camGo.transform.position = car.transform.TransformPoint(follow.FollowOffset);
        camGo.transform.LookAt(car.transform.position + composer.TargetOffset);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return $"OK collider center={box.center} size={box.size}";
    }
}
