using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace Realboot.ChaosHead
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
        public string startScript; // Temporary, will be set by the launcher
        [SerializeField] private float typingSpeed = 0.02f;
    
        // Engine State
        private bool isRunning = false;
        private bool isWaitingForInput = false;
    
        private bool InputTrigger
        {
            get { return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space); }
        }
    }
}