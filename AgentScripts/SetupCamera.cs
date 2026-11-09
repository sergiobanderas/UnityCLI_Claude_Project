using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor builder: sets up a smooth third-person Cinemachine camera following the cube.
public static class SetupCamera
{
    public static string Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity", OpenSceneMode.Single);

        var target = GameObject.Find("MyFirstCube");
        if (target == null) return "MyFirstCube not found";

        // 1. The Main Camera needs a CinemachineBrain: it copies the active Cinemachine camera.
        var mainCam = Camera.main;
        if (mainCam == null) return "Main Camera not found";
        var brain = mainCam.GetComponent<CinemachineBrain>();
        if (brain == null) brain = Undo.AddComponent<CinemachineBrain>(mainCam.gameObject);
        brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1f);

        // 2. Create (or reuse) the Cinemachine camera.
        var camGo = GameObject.Find("ThirdPersonCamera");
        if (camGo == null)
        {
            camGo = new GameObject("ThirdPersonCamera");
            Undo.RegisterCreatedObjectUndo(camGo, "Create ThirdPersonCamera");
        }

        var cmCam = camGo.GetComponent<CinemachineCamera>() ?? camGo.AddComponent<CinemachineCamera>();
        cmCam.Target.TrackingTarget = target.transform;
        cmCam.Lens.FieldOfView = 50f;
        cmCam.Priority.Value = 10;

        // 3. Position control: stay above and behind the cube, in world space,
        //    with damping so the camera lags slightly and moves smoothly.
        var follow = camGo.GetComponent<CinemachineFollow>() ?? camGo.AddComponent<CinemachineFollow>();
        follow.FollowOffset = new Vector3(0f, 6f, -8f);
        var tracker = follow.TrackerSettings;
        tracker.BindingMode = BindingMode.WorldSpace;
        tracker.PositionDamping = new Vector3(0.5f, 0.8f, 0.5f); // Y a bit softer so jumps don't shake the view.
        follow.TrackerSettings = tracker;

        // 4. Rotation control: keep looking at the cube, also damped.
        var composer = camGo.GetComponent<CinemachineRotationComposer>() ?? camGo.AddComponent<CinemachineRotationComposer>();
        composer.Damping = new Vector2(0.5f, 0.5f);
        composer.TargetOffset = new Vector3(0f, 0.5f, 0f);
        var composition = composer.Composition;
        composition.DeadZone.Enabled = true;
        composition.DeadZone.Size = new Vector2(0.1f, 0.1f); // Small moves don't rotate the camera.
        composer.Composition = composition;

        // Place it at the start pose so there is no initial swoop.
        camGo.transform.position = target.transform.position + follow.FollowOffset;
        camGo.transform.LookAt(target.transform.position + composer.TargetOffset);

        EditorUtility.SetDirty(camGo);
        EditorUtility.SetDirty(mainCam.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "OK";
    }
}
