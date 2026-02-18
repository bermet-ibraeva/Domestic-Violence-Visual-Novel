using System;
using System.Collections.Generic;

[Serializable]
public class EpisodeData
{
    public string episodeId;
    public string episodeTitle;

    public EpisodeVariables variables;
    public List<SceneData> scenes;
}

[Serializable]
public class EpisodeVariables
{
    public TrustVariables trust;
    public int risk;
    public int safety;
}

[Serializable]
public class TrustVariables
{
    public int Ainaz_Guldana;
}

[Serializable]
public class SceneData
{
    public string sceneId;
    public string background;

    public string leftCharacter;           // может быть null (summary)
    public List<string> rightCharacters;

    public string startNode;
    public List<DialogueNode> nodes;
}

[Serializable]
public class DialogueNode
{
    public string nodeId;
    public string background;
    public string character;   // как в JSON
    public string emotion;
    public string text;
    public string nextNode;

    public bool isThought;

    public List<Choice> choices;

    // пока оставим как было (если в некоторых эпизодах ещё старые эффекты)
    public NodeEffects effects;
    public RequirementData[] requirements;
}

[System.Serializable]
public class Choice
{
    public string text;
    public string nextNode;
    public List<EffectOp> effects;
}

[System.Serializable]
public class EffectOp
{
    public string op;
    public string key;
    public int value;
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
