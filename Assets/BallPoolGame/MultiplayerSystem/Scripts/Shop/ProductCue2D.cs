using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkManagement;

namespace NetworkManagement
{
    public class ProductCue2D : NetworkManagement.ProductWithTexturs2D
    {
        public string cue2DURL{ get; private set; }
        public Texture cueDefault2DTexture{ get; private set; }
        public Texture mainCue2DTexture{ get; private set; }
        public Material cue2dMaterial;

        void Start()
        {
            cueDefault2DTexture = cue2dMaterial.mainTexture;
        }
        protected override IEnumerator SetSources()
        {
            yield return StartCoroutine(base.SetSources());
            if (sourcesURL != null && sourcesURL.Length > 0)
            {
                cue2DURL = sourcesURL[0];
            }
            else
            {
                cue2DURL = "";

            }
            mainCue2DTexture = materials[0].mainTexture;
        }
    }
}