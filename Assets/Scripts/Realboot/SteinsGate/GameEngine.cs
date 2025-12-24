using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Reflection; 
using System.Diagnostics;

using TMPro;
using Debug = UnityEngine.Debug; // Avoid conflict with System.Diagnostics.Debug

namespace Realboot.SteinsGate
{
    public class GameEngine : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Realboot.UIReferences UI = new Realboot.UIReferences();
    
        [Header("Systems")]
        [SerializeField] private Realboot.SystemReferences system = new Realboot.SystemReferences();
        [SerializeField] private Macrosys macroSystem;
    
        [Header("Audio System")]
        [SerializeField] private Realboot.AudioReferences audio = new Realboot.AudioReferences();

    
        [Header("Settings")]
        public string startScript = "SG00_01"; // Temporary, will be set by the launcher
        [SerializeField] private float typingSpeed = 0.02f;
    
        // Engine State
        private bool isRunning = false;
        private bool isWaitingForInput = false;
    
        private bool InputTrigger
        {
            get { return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space); }
        }
    
        private Coroutine currentBgmFadeCoroutine;
        private Coroutine currentVoiceFadeCoroutine;
    
        void Start()
        {
            // Initialization
            if (UI.bgLayers != null)
            {
                for (int i = 0; i < UI.bgLayers.Length; i++)
                    if (UI.bgLayers[i] != null) UI.bgLayers[i].LoadBackground("NONE");
            }
    
            system.parser = new ScriptParser();
            system.memory = new StateManager();
            system.evaluator = new ExpressionEvaluator(system.memory);
            
            system.parser.LoadScript(startScript);
    
            isRunning = true;
            StartCoroutine(GameLoop());
        }
    
        void Update()
        {
            // Handle skip request during dialogue
            if (InputTrigger && system.dialogueSystem.isDisplaying)
            {
                system.dialogueSystem.RequestSkip();
            }
        }
    
        // Main Game Loop //
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
                
                // Parse next line
                if (!system.parser.HasMoreLines())
                {
                    Debug.Log("End of Script.");
                    isRunning = false;
                    break;
                }
    
                string rawLine = system.parser.GetCurrentLine();
                List<string> args = system.parser.ParseCommand(rawLine);
    
                if (args != null && args.Count > 0)
                {
                    string command = args[0];
                    yield return StartCoroutine(ExecuteCommand(command, args));
                }
    
                system.parser.NextLine();
            }
        }
    
        // Command Executor //
        IEnumerator ExecuteCommand(string commandName, List<string> args)
        {
            string methodName = commandName.Replace("#", "");
            MethodInfo method = this.GetType().GetMethod(methodName, 
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
    
            if (method != null)
            {
                List<string> parameters = new List<string>(args);
                parameters.RemoveAt(0);
    
                object result = method.Invoke(this, new object[] { parameters });
    
                IEnumerator coroutine = result as IEnumerator;
                if (coroutine != null) yield return StartCoroutine(coroutine);
            }
            else
            {
                if (methodName != "label") Debug.LogWarning("[ENGINE] Unknown Command: " + commandName);
            }
        }
    
        // ====================================
        //          AUDIO COMMANDS
        // ====================================
    
        // PLay a sound effect //
        public void playSE(List<string> args)
        {
            if (args.Count < 2) return; // At least channel + 1 file
    
            int channelIndex = int.Parse(args[0]); // First arg is channel
            
            // Channel bounds check
            if (channelIndex >= audio.seChannels.Count) {
                int chanCount = audio.seChannels.Count - 1;
                Debug.LogWarning("[AUDIO] Channel " + channelIndex + " not found. Max is " + chanCount);
                return;
            }
    
            // Check for flag
            bool loop = false;
            int lastArgIndex = args.Count - 1;
            
            if (args.Count > 2)
            {
                string lastArg = args[lastArgIndex].ToLower();
                if (lastArg == "true" || lastArg == "false")
                {
                    loop = bool.Parse(lastArg);
                    lastArgIndex--;
                }
            }
    
            // Select random audios if any
            List<string> potentialFiles = new List<string>();
            for (int i = 1; i <= lastArgIndex; i++)
            {
                potentialFiles.Add(args[i]);
            }
    
            string selectedFile = "";
            if (potentialFiles.Count > 0)
            {
                int randomIndex = Random.Range(0, potentialFiles.Count);
                selectedFile = potentialFiles[randomIndex];
            }
    
            // Call the play function
            PlaySoundEffect(channelIndex, selectedFile, loop);
        }
    
        // Play sound effect function //
        private void PlaySoundEffect(int channel, string filename, bool loop)
        {
            filename = filename.Replace("SGSE", "SE"); // file names correction
            AudioSource source = audio.seChannels[channel]; // Get the channels
    
            source.Stop();
    
            if (string.IsNullOrEmpty(filename) || filename == "0") return; // "0" flagf used to stop sound
    
            AudioClip clip = Resources.Load<AudioClip>("se/" + filename);
    
            if (clip != null)
            {
                source.clip = clip;
                source.loop = loop;
                source.Play();
            }
            else
            {
                Debug.LogWarning("[AUDIO] SE File not found: Resources/se/ " + filename);
            }
        }
    
        // stopSE channel //
        public void stopSE(List<string> args)
        {
            if (args.Count < 1) return;
    
            int channelIndex = int.Parse(args[0]);
    
            if (channelIndex >= 0 && channelIndex < audio.seChannels.Count)
            {
                AudioSource source = audio.seChannels[channelIndex];
                StartCoroutine(FadeOutAndStopSE(source, 0.5f)); // Fade out over 0.5 seconds
            }
        }
    
        // SE Fade out logic //
        private IEnumerator FadeOutAndStopSE(AudioSource source, float duration)
        {
            if (!source.isPlaying) yield break;
    
            float startVolume = source.volume;
            float timer = 0f;
    
            while (timer < duration)
            {
                timer += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }
    
            source.Stop();
            source.volume = 1f;
            source.clip = null;
        }
    
        // Background music function //
        public void playBGM(List<string> args)
        {
            if (args.Count < 1) return;
            string filename = args[0];
    
            if (audio.bgmSource.clip != null && audio.bgmSource.clip.name == filename && audio.bgmSource.isPlaying) 
                return;
    
            AudioClip clip = Resources.Load<AudioClip>("bgm/" + filename);
            if (clip == null)
            {
                Debug.LogWarning("[AUDIO] BGM File not found: Resources/bgm/ " + filename);
                return;
            }
    
            // Fade In
            if (currentBgmFadeCoroutine != null) StopCoroutine(currentBgmFadeCoroutine);
            currentBgmFadeCoroutine = StartCoroutine(FadeInNewBGM(clip, 1.0f));
        }
    
        // stop BGM function //
        public void stopBGM(List<string> args)
        {
            // On lance le processus de Fade Out
            if (currentBgmFadeCoroutine != null) StopCoroutine(currentBgmFadeCoroutine);
            currentBgmFadeCoroutine = StartCoroutine(FadeOutBGM(1.0f));
        }
    
        // BGM Fade In logic //
        private IEnumerator FadeInNewBGM(AudioClip newClip, float duration)
        {
            if (audio.bgmSource.isPlaying)
            {
                float startVol = audio.bgmSource.volume;
                float t = 0;
                while (t < 0.2f)
                {
                    t += Time.deltaTime;
                    audio.bgmSource.volume = Mathf.Lerp(startVol, 0f, t / 0.2f);
                    yield return null;
                }
            }
    
            audio.bgmSource.clip = newClip;
            audio.bgmSource.Play();
            audio.bgmSource.volume = 0f;
    
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                audio.bgmSource.volume = Mathf.Lerp(0f, 1f, timer / duration);
                yield return null;
            }
            audio.bgmSource.volume = 1f;
        }
    
        // BGM Fade Out logic //
        private IEnumerator FadeOutBGM(float duration)
        {
            if (!audio.bgmSource.isPlaying) yield break;
    
            float startVolume = audio.bgmSource.volume;
            float timer = 0f;
    
            while (timer < duration)
            {
                timer += Time.deltaTime;
                audio.bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }
    
            audio.bgmSource.Stop();
            audio.bgmSource.clip = null;
            audio.bgmSource.volume = 1f; // Reset volume
        }
    
        // Play voice with smooth transition //
        private IEnumerator PlayVoiceWithTransition(AudioClip clip)
        {
            if (clip == null) yield break;
    
            // If voice is already playing, fade it out first
            if (audio.voiceAudioSource.isPlaying)
            {
                if (currentVoiceFadeCoroutine != null) StopCoroutine(currentVoiceFadeCoroutine);
                currentVoiceFadeCoroutine = StartCoroutine(FadeOutVoice(0.15f));
                yield return currentVoiceFadeCoroutine;
            }
    
            // Play new voice with fade in
            audio.voiceAudioSource.clip = clip;
            audio.voiceAudioSource.Play();
            audio.voiceAudioSource.volume = 0f;
    
            if (currentVoiceFadeCoroutine != null) StopCoroutine(currentVoiceFadeCoroutine);
            currentVoiceFadeCoroutine = StartCoroutine(FadeInVoice(0.15f));
            yield return currentVoiceFadeCoroutine;
        }
    
        // Fade in voice //
        private IEnumerator FadeInVoice(float duration)
        {
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                audio.voiceAudioSource.volume = Mathf.Lerp(0f, 1f, timer / duration);
                yield return null;
            }
            audio.voiceAudioSource.volume = 1f;
        }
    
        // Fade out voice //
        private IEnumerator FadeOutVoice(float duration)
        {
            if (!audio.voiceAudioSource.isPlaying) yield break;
    
            float startVolume = audio.voiceAudioSource.volume;
            float timer = 0f;
    
            while (timer < duration)
            {
                timer += Time.deltaTime;
                audio.voiceAudioSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }
    
            audio.voiceAudioSource.Stop();
            audio.voiceAudioSource.volume = 1f;
        }
    
        // ====================================
        //          DIALOG COMMANDS 
        // ====================================
    
        // Generic message display
        public IEnumerator mes(List<string> args)
        {
            string text = string.Join(",", args.ToArray()).Replace("%p", "").Replace("「", "\"").Replace("」", "\"").Replace(",", ", ");
    
            yield return StartCoroutine(DisplayMessage("", text, null));
            isWaitingForInput = true;
        }
    
        // Message with voice and lip-sync
        public IEnumerator mes2v(List<string> args)
        {
            string voiceFile = args[0];
            string lipSyncExpression = args[1]; 
            string charID = args[2];
    
            // Text Assembly
            string fullText = "";
            for (int i = 3; i < args.Count; i++)
            {
                fullText += args[i];
                if (i < args.Count - 1) fullText += ",";
            }
    
            // Name parsing
            string name = "";
            string body = fullText;
            if (fullText.StartsWith("＠"))
            {
                int endNameIndex = fullText.IndexOf("＠", 1);
                if (endNameIndex != -1)
                {
                    name = fullText.Substring(1, endNameIndex - 1);
                    body = fullText.Substring(endNameIndex + 1);
                }
            }
    
            // Clean up body text
            body = body.Replace("%p", "").Replace("「", "\"").Replace("」", "\"").Replace(",", ", ");
    
            // Load Voice
            AudioClip clip = null;
            if (!string.IsNullOrEmpty(voiceFile))
            {
                clip = Resources.Load<AudioClip>("voice/" + voiceFile);
            }
    
            // Evaluate LipSync (work in progress)
            system.evaluator.Evaluate(lipSyncExpression); 
    
            // Display Message & wait for input
            yield return StartCoroutine(DisplayMessage(name, body, clip));
            isWaitingForInput = true;
        }
    
        // Close message window
        public IEnumerator messWindowCloseWait(List<string> args)
        {
            // Clear text with fade out
            yield return StartCoroutine(system.dialogueSystem.ClearText());
            
            // Hide text box
            yield return StartCoroutine(system.dialogueSystem.HideTextBox());
            
            UI.nameText.text = "";
            UI.bodyText.text = "";
            Debug.Log("[UI] Close Message Window");
        }
    
        // Open message window
        public IEnumerator messWindowOpenWait(List<string> args)
        {
            // Show text box with fade in
            yield return StartCoroutine(system.dialogueSystem.ShowTextBox());
            
            Debug.Log("[UI] Open Message Window");
        }
    
        // ====================================
        //           MISC COMMANDS
        // ====================================

        public IEnumerator call(List<string> args)
        {
            if (args.Count < 2) yield break; // At least system + 1 arg

            Debug.Log("[ENGINE] Calling Macro System with args: " + args[1]);
            if (args[0] == "macrosys")
            {
                // Only keep macro name
                string macroName = args[1];
                List<string> macroArgs = new List<string>();
                if(args.Count > 2)
                {
                    macroArgs = args.GetRange(2, args.Count - 2);
                }

                // Reflexion call
                MethodInfo method = macroSystem.GetType().GetMethod(macroName, 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (method != null)
                {
                    // Invoke macro
                    object result = method.Invoke(macroSystem, new object[] { macroArgs });

                    IEnumerator coroutine = result as IEnumerator;
                    if (coroutine != null)
                    {
                        yield return StartCoroutine(coroutine);
                    }
                }
                else
                {
                    Debug.LogWarning("[ENGINE] Unknown Macro: " + macroName); // RIP 
                }

            }

        }
    
        // Jump to label
        public void jump(List<string> args) 
        { 
            system.parser.GoToLabel(args[0]); 
            system.parser.currentLineIndex--; 
        }
        
        // Assign value to variable or flag
        public void assign(List<string> args)
        {
            string targetVar = args[0];
            int value = System.Convert.ToInt32(system.evaluator.Evaluate(args[1]));
    
            if (targetVar.StartsWith("$W"))
                system.memory.SetInt(targetVar.Substring(3, targetVar.Length - 4), value);
            else if (targetVar.StartsWith("$F"))
                system.memory.SetFlag(targetVar.Substring(3, targetVar.Length - 4), value != 0);
        }
        
        // Wait for specified frames
        public IEnumerator wait(List<string> args)
        {
            Debug.Log("Engine: wait for " + args[0] + " frames.");
            float frames = float.Parse(args[0]);
            yield return new WaitForSeconds(frames / 60f);
        }
        
        // Wait for specified milliseconds
        public IEnumerator mwait(List<string> args)
        {
            int val = system.evaluator.Evaluate(args[0]);
            float seconds = (float)val / 60.0f;
            
            if (seconds > 0)
                yield return new WaitForSeconds(seconds);
        }
    
        // ====================================
        //          GRAPHICS COMMANDS
        // ====================================
    
        // Load background into specified layer
        public void loadBG(List<string> args)
        {
            int layerIndex = int.Parse(args[0]);
            if (layerIndex >= 0 && layerIndex < UI.bgLayers.Length)
                UI.bgLayers[layerIndex].LoadBackground(args[1]);
        }
    
        // ====================================
        //           PRIVATE HELPERS
        // ====================================
    
        // Display message with typing effect
        private IEnumerator DisplayMessage(string name, string body, AudioClip voiceClip)
        {
            // Auto-open text box if not visible (professional VN behavior)
            if (!system.dialogueSystem.isTextBoxVisible)
            {
                yield return StartCoroutine(system.dialogueSystem.ShowTextBox());
            }
    
            // Play voice with smooth transition
            if (voiceClip != null)
            {
                yield return StartCoroutine(PlayVoiceWithTransition(voiceClip));
            }
    
            // Use new dialogue system
            yield return StartCoroutine(system.dialogueSystem.DisplayText(name, body, voiceClip, typingSpeed));
    
            // Wait for text to complete
            while (!system.dialogueSystem.isTextComplete)
            {
                yield return null;
            }
    
            // Wait for user input
            while (!InputTrigger)
            {
                yield return null;
            }
        }
    
        // ============================
        //       STUB COMMANDS
        // ============================
        public void label(List<string> args) { }
        public void playMovie(List<string> args) { }
        public void resetFlag(List<string> a) { }
        public void setFlag(List<string> a) { }
        public void FadeOut0(List<string> a) { }
        public void FadeIn0(List<string> a) { }
        public void InitGraph(List<string> a) { }
    }    
}
