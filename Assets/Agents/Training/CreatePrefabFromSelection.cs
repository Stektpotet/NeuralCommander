#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;
class CreatePrefabFromSelection
{
    /// <summary>
    /// Creates a prefab from the selected game object.
    /// </summary>
    [MenuItem("GameObject/Create Prefab From Selected")]
    static void CreatePrefab()
    {
        var objs = Selection.gameObjects;

        string pathBase = EditorUtility.SaveFolderPanel ("Choose save folder", "Assets", "");

        if (!string.IsNullOrEmpty(pathBase))
        {
            pathBase = pathBase.Remove(0, pathBase.IndexOf("Assets")) + Path.DirectorySeparatorChar;

            int i=0;
            foreach (var go in objs)
            {
                string localPath = AssetDatabase.GenerateUniqueAssetPath(($"{pathBase}{go.name}_{i}").Replace('\\', '/')) + ".prefab"; i++;
                Debug.Log(localPath);
                if (AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject)))
                {
                    if (EditorUtility.DisplayDialog("Are you sure?",
                            "The prefab already exists. Do you want to overwrite it?",
                            "Yes",
                            "No"))
                        CreateNew(go, localPath);
                }
                else
                    CreateNew(go, localPath);
            }
        }
    }

    static void CreateNew( GameObject obj, string localPath )
    {
        Object prefab = PrefabUtility.CreatePrefab (localPath, obj);

        Mesh m1 = obj.GetComponent<MeshFilter>()?.sharedMesh;
        Mesh cm = obj.GetComponent<MeshCollider>()?.sharedMesh;
        if (m1) AssetDatabase.CreateAsset(m1, localPath.Replace(".prefab","_mesh.asset")); //save aside mesh
        if (cm) AssetDatabase.CreateAsset(cm, localPath.Replace(".prefab", "_collider.asset")); //save aside mesh

        PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ConnectToPrefab);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Validates the menu.
    /// </summary>
    /// <remarks>The item will be disabled if no game object is selected.</remarks>
    [MenuItem("GameObject/Create Prefab From Selected", true)]
    static bool ValidateCreatePrefab() => Selection.activeGameObject != null;
}
#endif