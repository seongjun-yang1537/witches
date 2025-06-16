using UnityEngine;

namespace Ingame
{
    public class VFXModel : MonoBehaviour
    {
        public float duration;

        protected void Update()
        {
            duration -= Time.deltaTime;

            if (duration < 0f)
                Destroy(gameObject);
        }
    }
}