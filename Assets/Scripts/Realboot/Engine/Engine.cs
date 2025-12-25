using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace Realboot
{
    
    public enum GameMode
    {
        SteinsGate,
        ChaosHead,
    }

    public class Engine : MonoBehaviour 
    {
        [Header("Game Configuration")]
        public GameMode currentGameMode;
        private GameModuleBase activeGameModule;

        [Header("UI References")]
        public Realboot.UIReferences UI = new Realboot.UIReferences();

        [Header("Systems")]
        public Realboot.SystemReferences System = new Realboot.SystemReferences();

        [Header("Audio System")]
        public Realboot.AudioReferences Audio = new Realboot.AudioReferences();

        [Header("Settings")]
        public string startScript;
        public float TypingSpeed = 0.02f;

        // State
        public bool isWaitingForInput { get; set; } 

        // Input Trigger
        public bool InputTrigger
        {
            get { return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space); }
        }

        // Engine State
        private bool isRunning = false;

        void Start()
        {
            LoadGameModule();

            // Initialize Systems
            System.parser = new ScriptParser();
            System.memory = new StateManager();
            System.evaluator = new ExpressionEvaluator(System.memory);
            System.parser.LoadScript(startScript);
    
            isRunning = true;

            // Initialize BG
            if (UI.bgLayers != null)
            {
                for (int i = 0; i < UI.bgLayers.Length; i++)
                    if (UI.bgLayers[i] != null) UI.bgLayers[i].LoadBackground("NONE");
            }

            StartCoroutine(GameLoop());
        }

        // Load game module based on currentGameMode //
        void LoadGameModule()
        {
            GameModuleBase oldModule = GetComponent<GameModuleBase>();
            if (oldModule != null) Destroy(oldModule);

            // Start game modules/commands
            switch (currentGameMode)
            {
                case GameMode.SteinsGate:
                    activeGameModule = gameObject.AddComponent<Realboot.SteinsGate.SteinsGateModule>();
                    startScript = "SG00_01";
                    break;
                case GameMode.ChaosHead:
                    activeGameModule = gameObject.AddComponent<Realboot.ChaosHead.ChaosHeadModule>();
                    startScript = "none";
                    break;
                default:
                    Debug.LogError("[Engine] Unsupported Game Mode: " + currentGameMode.ToString());
                    return;
            }

            // Initialize the active game module
            if(activeGameModule != null)
            {
                activeGameModule.Initialize(this);
                Debug.Log("Loaded module : " + currentGameMode.ToString());
            }
        }

        void Update()
        {
            if (InputTrigger && System.dialogueSystem.isDisplaying)
            {
                System.dialogueSystem.RequestSkip();
            }
        }

        IEnumerator GameLoop()
        {
            while (isRunning)
            {
                if (isWaitingForInput)
                {
                    if (InputTrigger) isWaitingForInput = false;
                    yield return null;
                    continue;
                }
                
                if (!System.parser.HasMoreLines())
                {
                    Debug.Log("End of Script.");
                    isRunning = false;
                    break;
                }
    
                string rawLine = System.parser.GetCurrentLine();
                List<string> args = System.parser.ParseCommand(rawLine);
    
                if (args != null && args.Count > 0)
                {
                    string command = args[0];
                    yield return StartCoroutine(ExecuteCommand(command, args));
                }
    
                System.parser.NextLine();
            }
        }

        // Reflexion-based command execution
        IEnumerator ExecuteCommand(string commandName, List<string> args)
        {
            if (activeGameModule == null)
            {
                Debug.LogError("Aucun module de jeu actif !");
                yield break;
            }

            string methodName = commandName.Replace("#", "");
            
            // Search method in active game module
            MethodInfo method = activeGameModule.GetType().GetMethod(methodName, 
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
    
            if (method != null)
            {
                List<string> parameters = new List<string>(args);
                parameters.RemoveAt(0);
    
                // Invoke mmethod on active game module
                object result = method.Invoke(activeGameModule, new object[] { parameters });
    
                IEnumerator coroutine = result as IEnumerator;
                if (coroutine != null) yield return StartCoroutine(coroutine);
            }
            else
            {
                if (methodName != "label") Debug.LogWarning("[ENGINE] Unknown command in " + currentGameMode + ": " + commandName);
            }
        }
    }
}