using UnityEngine;
using UnityEditor;
using System.IO;

public class ItemDataImporter : Editor
{
    // 스프라이트 파일들이 저장된 경로 (본인 프로젝트에 맞게 수정)
    private static string spritesPath = "Assets/Resources/Image/";
    private static string dataSavePath = "Assets/Resources/ItemData/"; // 에셋 저장 경로를 변수로 분리

    [MenuItem("Tools/Import Item Data from CSV")]
    public static void ImportItemData()
    {
        string filePath = EditorUtility.OpenFilePanel("Select Item CSV", "", "csv");
        if (string.IsNullOrEmpty(filePath)) return;

        string[] allLines = File.ReadAllLines(filePath);

        // 첫 번째 줄(헤더)은 건너뛰고 시작
        for (int i = 1; i < allLines.Length; i++)
        {
            string[] data = allLines[i].Split(',');

            // CSV 데이터 파싱
            string id = data[0];

            // 1. 기존 에셋 파일이 있는지 경로를 확인
            string assetPath = dataSavePath + id + ".asset";
            ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);

            // 2. 파일이 없다면 새로 생성, 있다면 기존 파일에 덮어쓰기
            if (itemData == null)
            {
                itemData = ScriptableObject.CreateInstance<ItemData>();
                AssetDatabase.CreateAsset(itemData, assetPath);
            }

            // 3. 데이터 할당 (새 파일이든 기존 파일이든 동일하게 적용)
            itemData.itemName = data[1];
            itemData.description = data[3];
            itemData.maxStack = int.Parse(data[4]);

            // 아이콘 이름으로 스프라이트 찾아서 연결
            string spriteFilePath = spritesPath + data[2] + ".png"; // 확장자는 .png 등으로 맞게 수정
            itemData.itemIcon = AssetDatabase.LoadAssetAtPath<Sprite>(spriteFilePath);

            // 변경사항을 디스크에 저장하도록 표시
            EditorUtility.SetDirty(itemData);
        }
        AssetDatabase.SaveAssets(); // 모든 변경사항을 실제 파일에 저장
        AssetDatabase.Refresh();    // 프로젝트 창 새로고침
        Debug.Log("Item Data import complete!");
    }
}