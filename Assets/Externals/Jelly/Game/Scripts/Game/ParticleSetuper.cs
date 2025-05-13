#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.JellyMerge
{
    public class ParticleSetuper : MonoBehaviour
    {
        [SerializeField] private List<ParticleSystemRenderer> renderers;

        public void SetColor(ColorType colorType)
        {
            foreach (var psr in renderers)
                psr.material = BoardController.Instance.GetTargetMaterial((int)colorType);
        }
    }
}