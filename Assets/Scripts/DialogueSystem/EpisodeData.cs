using System;
using System.Collections.Generic;

[Serializable]
public class EpisodeData
{
    public string episodeId;
    public string episodeTitle;

    public List<CharacterMeta> characters;
    public EpisodeVariables variables;
    public List<SceneData> scenes;
}

[Serializable]
public class CharacterMeta
{
    public string characterId;
    public string displayName;
    public string portraitKey;
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
    public int AG;
    public int JA;
}

[Serializable]
public class SceneData
{
    public string sceneId;
    public string background;
    public bool bgFade = false;
    public float bgFadeDuration = 0.5f;
    public string bgFx = "none";

    public string leftCharacterId;
    public List<string> rightCharactersId;

    public string startNode;
    public List<DialogueNode> nodes;
}

[Serializable]
public class DialogueNode
{
    public string nodeId;

    public string background;
    public bool bgFade = false;
    public float bgFadeDuration = 0.5f;
    public string bgFx = "none";
    public bool stopPreviousBgEffect = true;

    public string characterId;
    public string emotion;
    public string text;
    public string nextNode;
    public string action;

    public bool isThought;

    public List<Choice> choices;
    public NodeEffects effects;
    public RequirementData[] requirements;
    public NotificationData notification;
}

[Serializable]
public class Choice
{
    public string text;
    public string nextNode;
    public List<EffectOp> effects;
    public NotificationData notification;
}

[Serializable]
public class EffectOp
{
    public string op;
    public string key;
    public int value;
}

[Serializable]
public class RequirementData
{
    public string key;
    public string op;
    public int value;
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

[Serializable]
public class NotificationData
{
    public string id;
    public bool showOnce = true;
    public string mode;
    public string title;
    public string message;
}