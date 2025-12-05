using System;
using System.Collections.Generic;

[Serializable]
public class EpisodeData
{
    public string episode;
    public Variables variables;
    public List<DialogueNode> nodes;
}

[Serializable]
public class Variables
{
    public int Сострадание;
    public int Послушание;
    public int Сопротивление;
    public int Тревога;
    public int Доверие;
}

[Serializable]
public class DialogueNode
{
    public string nodeId;
    public string background;   // "1", "2", "3", "4", "5"
    public string character;    // "Автор", "Айназ", ...
    public string emotion;      // "Calm", "Sad", "Scared", "Happy"
    public string text;
    public List<Choice> choices;
    public Effects effects;
    public string nextNode;
    public Requirement[] requirements;
}

[Serializable]
public class Choice
{
    public string text;
    public string nextNode;
}

[Serializable]
public class Effects
{
    public int Сострадание;
    public int Послушание;
    public int Сопротивление;
    public int Тревога;
    public int Доверие;
}

[Serializable]
public class Requirement
{
    public string condition;
    public string ending;
}
