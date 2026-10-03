using System;
using UnityEngine;

namespace NullSignal.Story
{
    public sealed class ObjectiveSystem : MonoBehaviour
    {
        public string Title { get; private set; } = "ESCAPE THE MAINTENANCE SECTOR";
        public string Hint { get; private set; } = "WASD move / E interact / Space dodge";
        public string Area { get; private set; } = "ASTRA-7 / MAINTENANCE";
        public float ChangedAt { get; private set; }
        public event Action Changed;
        public void Set(string title, string hint, string area = null)
        {
            Title = title; Hint = hint; if (area != null) Area = area;
            ChangedAt = Time.unscaledTime; Changed?.Invoke();
        }
    }
}
