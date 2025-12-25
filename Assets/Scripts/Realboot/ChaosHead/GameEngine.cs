using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Reflection; 
using System.Diagnostics;

using TMPro;
using Debug = UnityEngine.Debug; // Avoid conflict with System.Diagnostics.Debug

namespace Realboot.ChaosHead
{
    public class ChaosHeadModule : Realboot.GameModuleBase
    {
        // ============ IDK ===============
        public void SetPreGN(List<string> args){ }
        public void ChkPreGN(List<string> args){ }
        public void SetTitle(List<string> args){ }
        public void SetComment(List<string> args){ }
        public void Request(List<string> args){ }
        public void Delete(List<string> args){ }
        public void DeleteAll(List<string> args){ }
        public void ClearAll(List<string> args){ }
        public void MovePA(List<string> args){ }
        public void MovePO(List<string> args){ }
        public void Move(List<string> args){ }
        public void Zoom(List<string> args){ }

        // ================================

        // ====================================
        //              AUDIO
        // ====================================

        // -- Sound Effect --
        public void CreateSE(List<string> args){ }

        // -- Sound Managing --
        public void SoundStop(List<string> args){ }
        public void SoundStop2(List<string> args){ }
        public void SetVolume(List<string> args){ }
        public void StopSound(List<string> args){ }

        // -- Voice --
        public void SetVoice(List<string> args){ }
        public void CreateVOICE(List<string> args){ }

        // -- BGM --
        public void MusicStart(List<string> args){ }
        public void BGMPlay360(List<string> args){ }
        public void BGMPlay360Suspend(List<string> args){ }

        // ====================================
        //          TEXTURES & VISUAL
        // ====================================

        // -- Background --
        public void Fade(List<string> args){ }
        public void FadeDelete(List<string> args){ }

        // -- Textures --
        public void CreateColor(List<string> args){ }
        public void CreateTexture360(List<string> args){ }
        public void CreateTextureEX(List<string> args){ }
        public void CreateMovie360(List<string> args){ }

        // -- Takumi Room --
        public void CubeRoom(List<string> args){ }
        public void CubeRoom2(List<string> args){ }
        public void CubeRoom3(List<string> args){ }
        public void CubeRoom4(List<string> args){ }
        public void MoveCube(List<string> args){ }
        public void Rotate(List<string> args){ }
    }
}