using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor builder: invisible walls around the city so the car cannot fall off the map.
public static class CityBounds
{
    public static string Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/City.unity", OpenSceneMode.Single);
        var old = GameObject.Find("CityBounds");
        if (old != null) Object.DestroyImmediate(old);

        const float halfX = 60f, halfZ = 75f, height = 10f, thickness = 1f;
        var root = new GameObject("CityBounds");
        void Wall(string name, Vector3 center, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = center;
            go.AddComponent<BoxCollider>().size = size;
            go.isStatic = true;
        }
        Wall("North", new Vector3(0f, height / 2f, halfZ + thickness / 2f), new Vector3(halfX * 2f + thickness * 2f, height, thickness));
        Wall("South", new Vector3(0f, height / 2f, -halfZ - thickness / 2f), new Vector3(halfX * 2f + thickness * 2f, height, thickness));
        Wall("East", new Vector3(halfX + thickness / 2f, height / 2f, 0f), new Vector3(thickness, height, halfZ * 2f));
        Wall("West", new Vector3(-halfX - thickness / 2f, height / 2f, 0f), new Vector3(thickness, height, halfZ * 2f));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "OK";
    }
}
