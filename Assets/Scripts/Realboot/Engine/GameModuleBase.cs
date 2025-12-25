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
        public IEnumerator FadeInNewBGM(AudioClip newClip, float duration)
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
        public IEnumerator PlayVoiceWithTransition(AudioClip clip)
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

        // =================================
        //          MISC COMMANDS
        // =================================
        
        // Wait for specified frames
        public IEnumerator wait(List<string> args)
        {
            Debug.Log("Engine: wait for " + args[0] + " frames.");
            float frames = float.Parse(args[0]);
            yield return new WaitForSeconds(frames / 60f);
        }
    }
}