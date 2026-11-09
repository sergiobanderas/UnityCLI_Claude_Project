using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor builder: frictionless car collider + clear spawn point in the City scene.
public static class FixCity
{
    private const string MaterialPath = "Assets/Physics/CarNoFriction.physicsMaterial";
    private const string PlayerCarPrefab = "Assets/Prefabs/PlayerCar.prefab";

    public static string Run()
    {
        // 1. Frictionless physics material: the car slides along walls instead of sticking.
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(MaterialPath);
        if (mat == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Physics")) AssetDatabase.CreateFolder("Assets", "Physics");
            mat = new PhysicsMaterial("CarNoFriction")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                bounciness = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Minimum,
            };
            AssetDatabase.CreateAsset(mat, MaterialPath);
        }

        // 2. Apply it on the prefab, so every scene using PlayerCar gets it.
        var root = PrefabUtility.LoadPrefabContents(PlayerCarPrefab);
        root.GetComponent<BoxCollider>().sharedMaterial = mat;
        PrefabUtility.SaveAsPrefabAsset(root, PlayerCarPrefab);
        PrefabUtility.UnloadPrefabContents(root);

        // 3. Move the spawn to the bottom street (only parked van is in the opposite lane).
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/City.unity", OpenSceneMode.Single);
        var car = GameObject.Find("PlayerCar");
        Vector3 spawn = new Vector3(-30f, 0f, -65.5f);
        Physics.SyncTransforms();
        string ground = "none";
        if (Physics.Raycast(spawn + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore))
        {
            spawn.y = hit.point.y;
            ground = hit.collider.name;
        }
        car.transform.SetPositionAndRotation(spawn + Vector3.up * 0.1f, Quaternion.Euler(0f, 90f, 0f));

        // Snap the virtual camera behind the new spawn so there is no initial swoop.
        var camGo = GameObject.Find("ThirdPersonCamera");
        var follow = camGo.GetComponent<Unity.Cinemachine.CinemachineFollow>();
        camGo.transform.position = car.transform.TransformPoint(follow.FollowOffset);
        camGo.transform.LookAt(car.transform.position + Vector3.up * 1.2f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return $"OK spawn={car.transform.position} ground={ground}";
    }
}
