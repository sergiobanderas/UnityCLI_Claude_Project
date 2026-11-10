using System.Reflection;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor builder: A/D steering on the PlayerCar prefab + mouse-orbit third-person camera.
public static class SetupOrbitCamera
{
    private const string PlayerCarPrefab = "Assets/Prefabs/PlayerCar.prefab";
    private static readonly string[] Scenes = { "Assets/Scenes/City.unity", "Assets/Scenes/Level1.unity" };

    public static string Run()
    {
        var log = new System.Text.StringBuilder();

        // 1. Re-apply the default input bindings on the prefab (A/D turn instead of mouse).
        var root = PrefabUtility.LoadPrefabContents(PlayerCarPrefab);
        var controller = root.GetComponent<CarController>();
        typeof(CarController).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(controller, null);
        PrefabUtility.SaveAsPrefabAsset(root, PlayerCarPrefab);
        PrefabUtility.UnloadPrefabContents(root);

        // 2. Orbital camera in every scene.
        foreach (string path in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var camGo = GameObject.Find("ThirdPersonCamera");
            if (camGo == null) { log.Append($"{path}: no camera\n"); continue; }

            // Make sure the scene's car instance has no leftover overrides on the input fields.
            var car = Object.FindFirstObjectByType<CarController>();
            if (car != null && PrefabUtility.IsPartOfPrefabInstance(car))
            {
                PrefabUtility.RevertObjectOverride(car, InteractionMode.AutomatedAction);
            }

            // Replace the fixed follow with an orbit around the car.
            if (camGo.TryGetComponent(out CinemachineFollow oldFollow)) Object.DestroyImmediate(oldFollow);
            if (!camGo.TryGetComponent(out CinemachineOrbitalFollow orbit)) orbit = camGo.AddComponent<CinemachineOrbitalFollow>();

            orbit.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
            orbit.Radius = 10f;
            orbit.TargetOffset = Vector3.zero;

            var tracker = orbit.TrackerSettings;
            tracker.BindingMode = BindingMode.LockToTargetWithWorldUp; // Orbit angle 0 = behind the car.
            tracker.PositionDamping = new Vector3(0.3f, 0.8f, 0.4f);
            tracker.RotationDamping = new Vector3(0f, 0.6f, 0f);
            orbit.TrackerSettings = tracker;

            // Horizontal: full 360 orbit; after you stop moving the mouse it swings back behind the car.
            orbit.HorizontalAxis.Range = new Vector2(-180f, 180f);
            orbit.HorizontalAxis.Wrap = true;
            orbit.HorizontalAxis.Center = 0f;
            orbit.HorizontalAxis.Value = 0f;
            orbit.HorizontalAxis.Recentering = new InputAxis.RecenteringSettings { Enabled = true, Wait = 1.5f, Time = 1f };

            // Vertical: from slightly above the road to a high view; starts at 25 degrees.
            orbit.VerticalAxis.Range = new Vector2(5f, 60f);
            orbit.VerticalAxis.Wrap = false;
            orbit.VerticalAxis.Center = 25f;
            orbit.VerticalAxis.Value = 25f;
            orbit.VerticalAxis.Recentering = new InputAxis.RecenteringSettings { Enabled = true, Wait = 1.5f, Time = 1f };

            // Mouse wheel zoom: 0.6x (closer) to 1.6x (further) of the radius.
            orbit.RadialAxis.Range = new Vector2(0.6f, 1.6f);
            orbit.RadialAxis.Center = 1f;
            orbit.RadialAxis.Value = 1f;

            // Mouse input for the orbit (Cinemachine fills in its default "Look" mouse/gamepad actions on add).
            if (!camGo.TryGetComponent(out CinemachineInputAxisController input))
            {
                input = camGo.AddComponent<CinemachineInputAxisController>();
            }
            input.SynchronizeControllers();

            foreach (var c in input.Controllers)
            {
                log.Append($"{path}: axis '{c.Name}' action={(c.Input.InputAction != null ? c.Input.InputAction.name : "none")} gain={c.Input.Gain}\n");
            }

            EditorUtility.SetDirty(camGo);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        EditorSceneManager.OpenScene(Scenes[0], OpenSceneMode.Single);
        return log.ToString();
    }
}
