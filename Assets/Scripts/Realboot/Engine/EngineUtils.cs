using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

/**
Here is a utility script to hold common data structures used across multiple games
mostly avoid spaghetti code when multiple game support is needed.
**/
namespace Realboot
{
    [Serializable]
    public class UIReferences
    {
        // Text References
        public TMP_Text nameText;
        public TMP_Text bodyText;

        // BG and Sprites
        public BackgroundLayer[] bgLayers;
    }

    [Serializable]
    public class SystemReferences
    {
        public DialogueSystem dialogueSystem;
        public ScriptParser parser;
        public StateManager memory;
        public ExpressionEvaluator evaluator;
    }

    [Serializable]
    public class AudioReferences
    {
        public AudioSource voiceAudioSource;
        public AudioSource bgmSource;
        public List<AudioSource> seChannels = new List<AudioSource>();
        public int totalChannels = 8;
    }
}
