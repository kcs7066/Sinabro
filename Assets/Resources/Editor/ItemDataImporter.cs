using ExcelDataReader; // ExcelDataReader 라이브러리 사용
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ItemDataImporter : Editor
{
    private static string spritesPath = "Assets/Resources/Sprites/Items/";
    [MenuItem("Tools/Import Item Data from Excel")]
    public static void ImportItemData()
    {

    string filePath = EditorUtility.OpenFilePanel("Select Excel Data", "", "xlsx");
        if (string.IsNullOrEmpty(filePath)) return;

        FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        DataSet dataSet = excelReader.AsDataSet();

        // 각 시트별로 데이터 처리
        ProcessSheet(dataSet.Tables["Equipment"], "Assets/Resources/ItemData/Equipment/", typeof(EquipmentData));
        ProcessSheet(dataSet.Tables["Consumables"], "Assets/Resources/ItemData/Consumables/", typeof(ConsumableData));
        ProcessSheet(dataSet.Tables["Materials"], "Assets/Resources/ItemData/Materials/", typeof(MaterialData));

        excelReader.Close();
        stream.Close();

        AssetDatabase.SaveAssets(); // 모든 변경사항을 실제 파일에 저장
        AssetDatabase.Refresh();    // 프로젝트 창 새로고침
        Debug.Log("모든 아이템 데이터 임포트/업데이트 완료!");
    }

    private static void ProcessSheet(DataTable sheet, string savePath, Type itemType)
    {
        if (sheet == null)
        {
            Debug.LogWarning(savePath + " 경로에 해당하는 시트를 찾을 수 없습니다.");
            return;
        }

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

    // 헤더(첫 번째 행)에서 컬럼 이름을 매핑
    Dictionary<string, int> columnMap = new Dictionary<string, int>();
    for (int i = 0; i < sheet.Columns.Count; i++)
    {
        columnMap[sheet.Rows[0][i].ToString().Trim()] = i;
    }

    // 첫 번째 행(헤더)은 건너뛰고 시작
    for (int i = 1; i < sheet.Rows.Count; i++)
        {
            DataRow row = sheet.Rows[i];

            try
            {
                string id = row[columnMap["itemID"]].ToString();
                if (string.IsNullOrEmpty(id)) continue;

                string assetPath = savePath + id + ".asset";
                ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);

                if (itemData == null)
                {
                    itemData = (ItemData)ScriptableObject.CreateInstance(itemType);
                    AssetDatabase.CreateAsset(itemData, assetPath);
                }

                // 공통 데이터 설정
                itemData.itemID = id;
                itemData.itemName = row[columnMap["itemName"]].ToString();
                itemData.description = row[columnMap["description"]].ToString();

                // 아이콘 이름으로 스프라이트 찾아서 연결
                string iconName = row[columnMap["itemIcon"]].ToString();
                if (!string.IsNullOrEmpty(iconName))
                {
                    // 경로 + 파일이름 + 확장자
                    string spriteFilePath = spritesPath + iconName + ".png"; // 확장자가 다르다면 수정
                    itemData.itemIcon = AssetDatabase.LoadAssetAtPath<Sprite>(spriteFilePath);

                    if (itemData.itemIcon == null)
                    {
                        Debug.LogWarning($"스프라이트를 찾을 수 없습니다: {spriteFilePath}");
                    }
                }




                // 타입별 특화 데이터 설정
                if (itemType == typeof(EquipmentData))
                {
                    EquipmentData equipData = (EquipmentData)itemData;
                    // TryParse를 사용하여 더 안전하게 변환
                    Enum.TryParse(row[columnMap["equipType"]].ToString(), out equipData.equipType);
                    int.TryParse(row[columnMap["level"]].ToString(), out equipData.level);
                    int.TryParse(row[columnMap["attackPower"]].ToString(), out equipData.attackPower);
                    float.TryParse(row[columnMap["attackSpeed"]].ToString(), out equipData.attackSpeed);
                    float.TryParse(row[columnMap["attackRange"]].ToString(), out equipData.attackRange);
                    float.TryParse(row[columnMap["moveSpeedBonus"]].ToString(), out equipData.moveSpeedBonus);
                    int.TryParse(row[columnMap["additionalAttackPower"]].ToString(), out equipData.additionalAttackPower);
                    float.TryParse(row[columnMap["additionalAttackSpeed"]].ToString(), out equipData.additionalAttackSpeed);
                    float.TryParse(row[columnMap["itemDropRateBonus"]].ToString(), out equipData.itemDropRateBonus);
                }
                else if (itemType == typeof(ConsumableData))
                {
                    ConsumableData consumableData = (ConsumableData)itemData;

                    Enum.TryParse(row[columnMap["consumableType"]].ToString(), out consumableData.consumableType);
                    float.TryParse(row[columnMap["value"]].ToString(), out consumableData.value);
                    float.TryParse(row[columnMap["duration"]].ToString(), out consumableData.duration);
                    int.TryParse(row[columnMap["maxStack"]].ToString(), out consumableData.maxStack);
                }
                else if (itemType == typeof(MaterialData))
                {
                    MaterialData materialData = (MaterialData)itemData;
                    int.TryParse(row[columnMap["maxStack"]].ToString(), out materialData.maxStack);
                }

                EditorUtility.SetDirty(itemData);
            }
            catch (Exception ex)
            {
                // 오류 발생 시, 어느 행에서 문제가 생겼는지 알려줌
                Debug.LogError($"'{sheet.TableName}' 시트의 {i + 1}번째 행 처리 중 오류 발생: {ex.Message}");
            }
        }
    }
}