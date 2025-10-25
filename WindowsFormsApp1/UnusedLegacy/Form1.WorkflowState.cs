using CrystalTable.Models;

namespace CrystalTable
{
    public partial class Form1
    {
        private enum MapWorkflowStage
        {
            Idle,
            Draft,
            Ready,
            WaferActive
        }

        private MapWorkflowStage workflowStage = MapWorkflowStage.Idle;
        private MapMetadata currentMapMetadata;
        private MapMetadata draftMapMetadata;
        private WaferRunMetadata currentWaferMetadata;
        private string currentTemplateName;
        private string draftTemplateName;
        private bool draftDirty;
        private TemplateManager templateManager;
        private int nextWaferSequence = 1;
    }
}
