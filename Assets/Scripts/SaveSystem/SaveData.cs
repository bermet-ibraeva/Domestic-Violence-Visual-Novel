using System;

[Serializable]
public class SaveData
{
    public string episodePath;    // путь в Resources, например "Episodes/episode_1"
    public string currentNodeId;  // последний узел, например "scene_3_start"
    public int chapterNumber;     // номер эпизода/главы
}
