using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor builder: creates a drivable City scene from the ithappy URP demo scene.
public static class SetupCity
{
    private const string SourceScene = "Assets/ithappy/Cartoon_City_Free/Scenes/URP_Scenes/Demonstration.unity";
    private const string CityScene = "Assets/Scenes/City.unity";
    private const string Level1Scene = "Assets/Scenes/Level1.unity";
    private const string PlayerCarPrefab = "Assets/Prefabs/PlayerCar.prefab";

    public static string Run()
    {
        // 1. Turn the configured car from Level1 into a reusable prefab (variant of Car 01).
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCarPrefab) == null)
        {
            var level1 = EditorSceneManager.OpenScene(Level1Scene, OpenSceneMode.Single);
            var levelCar = GameObject.Find("Car01");
            if (levelCar == null) return "Car01 not found in Level1";
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
            PrefabUtility.SaveAsPrefabAssetAndConnect(levelCar, PlayerCarPrefab, InteractionMode.AutomatedAction);
            EditorSceneManager.SaveScene(level1);
        }

        // 2. Copy the demo scene so the original asset stays untouched.
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(CityScene) == null && !AssetDatabase.CopyAsset(SourceScene, CityScene))
        {
            return "Could not copy demo scene";
        }

        var scene = EditorSceneManager.OpenScene(CityScene, OpenSceneMode.Single);

        // The "Overview" group is only an asset showcase placed far from the city.
        var overview = GameObject.Find("Overview");
        if (overview != null) Object.DestroyImmediate(overview);

        // 3. Spawn the player car on the straight road at x = -7.5, facing +Z.
        var car = GameObject.Find("PlayerCar");
        if (car == null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCarPrefab);
            car = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            car.name = "PlayerCar";
        }

        Vector3 spawn = new Vector3(-7.5f, 0f, -35f);
        Physics.SyncTransforms();
        if (Physics.Raycast(spawn + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore))
        {
            spawn.y = hit.point.y;
        }
        car.transform.SetPositionAndRotation(spawn + Vector3.up * 0.1f, Quaternion.identity);

        // 4. Main Camera drives from Cinemachine.
        var mainCam = Camera.main;
        if (mainCam == null) return "Main Camera not found";
        if (!mainCam.TryGetComponent(out CinemachineBrain brain)) brain = mainCam.gameObject.AddComponent<CinemachineBrain>();
        brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1f);

        // 5. Third-person Cinemachine camera (same tuning as Level1).
        var camGo = GameObject.Find("ThirdPersonCamera") ?? new GameObject("ThirdPersonCamera");
        if (!camGo.TryGetComponent(out CinemachineCamera cmCam)) cmCam = camGo.AddComponent<CinemachineCamera>();
        cmCam.Target.TrackingTarget = car.transform;
        cmCam.Lens.FieldOfView = 50f;
        cmCam.Lens.FarClipPlane = 500f;
        cmCam.Priority.Value = 10;

        if (!camGo.TryGetComponent(out CinemachineFollow follow)) follow = camGo.AddComponent<CinemachineFollow>();
        follow.FollowOffset = new Vector3(0f, 4.5f, -9f);
        var tracker = follow.TrackerSettings;
        tracker.BindingMode = BindingMode.LockToTargetWithWorldUp;
        tracker.PositionDamping = new Vector3(0.3f, 0.8f, 0.4f);
        tracker.RotationDamping = new Vector3(0f, 0.6f, 0f);
        follow.TrackerSettings = tracker;

        if (!camGo.TryGetComponent(out CinemachineRotationComposer composer)) composer = camGo.AddComponent<CinemachineRotationComposer>();
        composer.Damping = new Vector2(0.5f, 0.5f);
        composer.TargetOffset = new Vector3(0f, 1.2f, 0f);
        var composition = composer.Composition;
        composition.DeadZone.Enabled = false;
        composer.Composition = composition;

        camGo.transform.position = car.transform.TransformPoint(follow.FollowOffset);
        camGo.transform.LookAt(car.transform.position + composer.TargetOffset);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return $"OK spawn={car.transform.position} groundHit={(hit.collider != null ? hit.collider.name : "none")}";
    }
}
