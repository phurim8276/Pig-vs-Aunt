using System.Collections.Generic;
using UnityEngine;

public static class GoogleSheetParser
{
    public static ItemData ConvertToItemData(GoogleSheetData sheet)
    {
        if (sheet.parsedRows.Count == 0)
        {
            Debug.LogWarning("GoogleSheetParser: No data found in sheet.");
            return null;
        }

        ItemData itemData = new ItemData();

        foreach (var row in sheet.parsedRows)
        {
            if (row.cells.Count < 2) continue;

            string name = row.cells[0].Trim();

            Debug.Log($"Row name: '{name}'");

            StatBlock stat = new StatBlock
            {
                Amount = GetCell(row, 1),
                Damage = GetCell(row, 2),
                HP = GetCell(row, 3),
                MissedChance = GetCell(row, 4),
                Sec = GetCell(row, 5),
                WindForce = GetCell(row, 6)
            };

            switch (name)
            {
                case "Player HP": itemData.PlayerHP = stat; break;
                case "Enemy HP(easy)": itemData.EnemyHP_Easy = stat; break;
                case "Enemy HP(normal)": itemData.EnemyHP_Normal = stat; break;
                case "Enemy HP(hard)": itemData.EnemyHP_Hard = stat; break;
                case "Normal Attack": itemData.NormalAttack = stat; break;
                case "Small Attack": itemData.SmallAttack = stat; break;
                case "Power Throw": itemData.PowerThrow = stat; break;
                case "Double Attack": itemData.DoubleAttack = stat; break;
                case "Heal": itemData.Heal = stat; break;
                case "Time to think": itemData.TimeToThink = stat; break;
                case "Time to Warning": itemData.TimeToWarning = stat; break;
                case "Wind Values": itemData.WindValues = stat; break;
                case "Normal Enemy Activate <=": itemData.NormalEnemyActivate = stat; break;
                case "Hard Enemy Activate >=": itemData.HardEnemyActivate = stat; break;
            }
        }

        return itemData;
    }

    private static string GetCell(GoogleSheetData.Row row, int index)
    {
        return (row.cells.Count > index) ? row.cells[index].Trim() : "";
    }
}
