using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Skritty.Tools.Audio
{
    [System.Serializable]
    public class Subtitles
    {
        public float startTime;
        public bool customDuration = false;
        [ShowIf("@customDuration")]
        public float duration;
        [TextArea]
        public string subtitles;
        public bool overrideOtherSubs = true;
    }
}
