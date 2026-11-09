using UnityEngine;
using UnityEditor.SceneManagement;
var composer = GameObject.Find("ThirdPersonCamera").GetComponent<Unity.Cinemachine.CinemachineRotationComposer>();
var c = composer.Composition; c.DeadZone.Enabled = false; composer.Composition = c;
UnityEditor.EditorUtility.SetDirty(composer);
EditorSceneManager.MarkSceneDirty(composer.gameObject.scene);
return EditorSceneManager.SaveScene(composer.gameObject.scene);
