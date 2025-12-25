using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Reflection; 
using System.Diagnostics;
using Realboot;

using TMPro;
using Debug = UnityEngine.Debug; // Avoid conflict with System.Diagnostics.Debug

namespace Realboot.SteinsGate
{
    public class SteinsGateModule : Realboot.GameModuleBase
    {
        private Macrosys macroSystem;
    
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
