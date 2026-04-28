using System;
using System.Collections.Generic;


[Serializable]
public class EpisodeData
{
    public string episodeId;
    public string episodeTitleKey;

    public List<CharacterMeta> characters;
    public List<SceneData> scenes;
}

[Serializable]
public class CharacterMeta
{
    public string characterId;
    public string displayNameKey;
    public string portraitKey;
}

[Serializable]
public class SceneData
{
    public string sceneId;

    // Background
    public string background;
    public bool bgFade = false;
    public float bgFadeDuration = 0.5f;
    public string bgFx = "none";

    // Characters
    public string leftCharacterId;
    public List<string> rightCharacterIds;

    public string startNode;
    public List<DialogueNode> nodes;

}

[Serializable]
public class DialogueNode
{
    public string nodeId;

    // Background Override
    public string background;
    public bool bgFade = false;
    public float bgFadeDuration = 0.5f;
    public string bgFx = "none";
    public bool stopPreviousBgEffect = true;

    // Dialogue
    public string characterId;
    public string emotion;
    public string textKey;
    public string nextNode;

    public bool isThought;

    // Logic
    public List<Choice> choices;
    public NodeEffects effects;
    public RequirementData[] requirements;
    public NotificationData notification;
    public NodeAction action;
    public AudioData audio;

}

[Serializable]
public class Choice
{
    public string textKey;
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
    public string titleKey;
    public string messageKey;
}

[Serializable]
public class NodeAction
{
    public string type;     // "unlock_note"
    public string noteId;   // "note_rumors_01"
    public string status;   // "unlocked" (на будущее)
}