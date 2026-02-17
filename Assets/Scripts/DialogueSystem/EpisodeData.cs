using System;
using System.Collections.Generic;

[Serializable]
public class EpisodeData
{
    public string episode;
    public List<DialogueNode> nodes;
}

[Serializable]
public class DialogueNode
{
    public string nodeId;
    public string background;
    public string character;
    public string emotion;
    public string text;
    public string nextNode;

    public List<Choice> choices;

    public NodeEffects effects;
    public RequirementData[] requirements;
}

[Serializable]
public class Choice
{
    public string text;
    public string nextNode;
}

[Serializable]
public class RequirementData
{
    public string ending;
}

[Serializable]
public class NodeEffects
{
    public int trustAG;
    public int trustJA;
    public int risk;
    public int safety;
    public int sparks;
}
