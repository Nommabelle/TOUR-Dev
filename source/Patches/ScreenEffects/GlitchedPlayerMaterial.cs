using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TownOfUs.Patches.ScreenEffects
{
    public class GlitchedPlayerMaterial : TemporaryMaterial
    {
        public GlitchedPlayerMaterial(GameObject target) : base(target) { }
        public GlitchedPlayerMaterial(Renderer target) : base(target) { }
        public override Material mat => TownOfUs.bundledAssets.Get<Material>("GlitchedPlayer");
        public override float duration => 5f;
    }
}