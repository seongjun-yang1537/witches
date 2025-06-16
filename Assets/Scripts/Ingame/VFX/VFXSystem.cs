using System.ComponentModel;
using Corelib.Utils;
using UnityEngine;

namespace Ingame
{
    public class VFXSystem : Singleton<VFXSystem>
    {
        public static VFXModel Spawn(string key, float duration)
        {
            GameObject go = Instantiate(VFXDB.Get(key));

            Transform tr = go.transform;
            tr.SetParent(Instance.transform);

            VFXModel model = go.GetComponent<VFXModel>();
            model.duration = duration;

            return model;
        }
    }
}