using UnityEngine;
using UnityEngine.Rendering;

namespace NullSignal.Presentation
{
    /// <summary>Scene-owned full-resolution forward URP preset; restores the project's selection on exit.</summary>
    [DefaultExecutionOrder(-200)]
    public sealed class GameplayRenderSettings : MonoBehaviour
    {
        [SerializeField] private RenderPipelineAsset pipeline;
        private RenderPipelineAsset previous;
        private bool applied;
        public void Configure(RenderPipelineAsset value) => pipeline = value;
        private void OnEnable()
        {
            if (pipeline == null) return;
            previous = QualitySettings.renderPipeline; QualitySettings.renderPipeline = pipeline; applied = true;
        }
        private void OnDisable()
        {
            if (applied && QualitySettings.renderPipeline == pipeline) QualitySettings.renderPipeline = previous;
            applied = false;
        }
    }
}
