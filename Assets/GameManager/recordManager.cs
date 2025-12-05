using UnityEngine;
using System.IO;

public static class RecordManager
{
    private static string filePath => Path.Combine(Application.persistentDataPath, "records.json");

    public static RecordData Load()
    {
        if (!File.Exists(filePath))
        {
            return new RecordData();
        }

        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<RecordData>(json);
    }

    public static void Save(RecordData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
    }

    public static void AddRecord(float time)
    {
        RecordData records = Load();
        records.bestTimes.Add(time);

        // Ordenar de menor a mayor (mejor tiempo = el más bajo)
        records.bestTimes.Sort();

        // Mantener TOP 10 (o los que quieras)
        if (records.bestTimes.Count > 10)
            records.bestTimes.RemoveAt(10);

        Save(records);
    }

    public static float GetBestRecord()
    {
        RecordData records = Load();
        if (records.bestTimes.Count == 0)
            return float.MaxValue;

        return records.bestTimes[0];
    }
}
