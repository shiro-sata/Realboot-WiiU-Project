using System.Collections.Generic;
using UnityEngine;

// Manages the game's memory state

namespace Realboot
{
    public class StateManager
    {
        // $W variables (Integers) - Ex: $W(LR_DATE)
        private Dictionary<string, int> intVariables = new Dictionary<string, int>();
    
        // $F flags (Booleans) - Ex: $F(SF_Phone_Open)
        private Dictionary<string, bool> boolFlags = new Dictionary<string, bool>();
    
        // $T variables (Thread/Temp)
        private Dictionary<int, int> threadVariables = new Dictionary<int, int>();
    
        public int GetInt(string key)
        {
            if (intVariables.ContainsKey(key)) return intVariables[key];
            return 0; // Default value
        }
    
        public void SetInt(string key, int value)
        {
            if (intVariables.ContainsKey(key)) intVariables[key] = value;
            else intVariables.Add(key, value);
            
            Debug.Log(string.Format("[MEMORY] Set Int: {0} = {1}", key, value));
        }
        
        public bool GetFlag(string key)
        {
            if (boolFlags.ContainsKey(key)) return boolFlags[key];
            return false;
        }
    
        public void SetFlag(string key, bool value)
        {
            if (boolFlags.ContainsKey(key)) boolFlags[key] = value;
            else boolFlags.Add(key, value);
    
            Debug.Log(string.Format("[MEMORY] Set Flag: {0} = {1}", key, value));
        }
    }
}
