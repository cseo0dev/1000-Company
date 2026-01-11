//코드 담당자: 유호정
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class FindPrefabsWithAudioSource
{

    [MenuItem("Tools/Find Prefabs with AudioSource in Folder...")]
    private static void FindPrefabsInFolder()
    {
        string absolutePath = EditorUtility.OpenFolderPanel("Search in Folder", Application.dataPath, "");


        if (string.IsNullOrEmpty(absolutePath) || !absolutePath.StartsWith(Application.dataPath))
        {
            if (!string.IsNullOrEmpty(absolutePath))
            {
                Debug.LogWarning("검색은 'Assets' 폴더 안에서만 가능합니다.");
            }
            return;
        }

        string relativePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);

        Debug.Log($"--- '{relativePath}' 폴더에서 AudioSource를 가진 프리팹 검색 시작 ---");

        List<GameObject> prefabsFound = new List<GameObject>();

        string[] searchInFolders = new[] { relativePath };


        string[] guids = AssetDatabase.FindAssets("t:Prefab", searchInFolders);

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                if (prefab.GetComponentInChildren<AudioSource>(true))
                {
                    prefabsFound.Add(prefab);
                }
            }
        }

        if (prefabsFound.Count > 0)
        {
            Debug.Log($"[성공] 총 {prefabsFound.Count}개의 프리팹에서 AudioSource를 찾았습니다.");
            foreach (GameObject pf in prefabsFound)
            {
                Debug.Log($"▶ {pf.name}", pf);
            }
        }
        else
        {
            Debug.LogWarning($"[결과] '{relativePath}' 폴더에서 AudioSource를 가진 프리팹을 찾지 못했습니다.");
        }

        Debug.Log("--- 검색 완료 ---");
    }
}