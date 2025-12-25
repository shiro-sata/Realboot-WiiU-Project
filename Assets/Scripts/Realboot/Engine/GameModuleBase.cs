using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Realboot
{
    public abstract class GameModuleBase : MonoBehaviour
    {
        protected Engine coreEngine;

        // call core Engine
        public virtual void Initialize(Engine engine)
        {
            this.coreEngine = engine;
        }

        // ======================================
        //          COREE ENGINE ACCESSORS
        // ======================================

        protected Realboot.UIReferences UI 
        { 
            get { return coreEngine.UI; } 
        }

        protected Realboot.SystemReferences system 
        { 
            get { return coreEngine.System; } 
        }

        protected Realboot.AudioReferences audio 
        { 
            get { return coreEngine.Audio; } 
        }

        /** protected Macrosys macroSystem 
        { 
            get { return coreEngine.macroSystem; } 
        } **/

        protected float typingSpeed 
        { 
            get { return coreEngine.TypingSpeed; } 
        }
        protected Coroutine currentBgmFadeCoroutine;
        protected Coroutine currentVoiceFadeCoroutine;

        protected bool isWaitingForInput
        {
            get { return coreEngine.isWaitingForInput; }
            set { coreEngine.isWaitingForInput = value; }
        }

        protected bool InputTrigger
        {
            get { return coreEngine.InputTrigger; }
        }
        // ==========================================

        // Global commands here, so it prevent form copying and pasting same commands logics over GameEngines
    }
}