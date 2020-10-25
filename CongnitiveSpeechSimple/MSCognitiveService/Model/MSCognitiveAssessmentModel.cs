using System;

namespace MSCognitiveService.Model
{
    public class MsCognitiveAssessmentModel
    {
        public string ReferenceText { get; set; }
        public GradingSystem GradingSystem = GradingSystem.FivePoint;
        public Granularity Granularity = Granularity.Phoneme;
        public Dimension Dimension = Dimension.Basic;
        public EnableMiscue EnableMiscue = EnableMiscue.True;
        public Guid ScenarioId { get; set; }
        public bool IsOn = false;
        public bool IsAssessDetail = false;
    }

    public enum GradingSystem
    {
        FivePoint,
        HundredMark,
    };

    public enum Granularity
    {
        Phoneme,
        FullText,
    };

    public enum Dimension
    {
        Basic,
        Comprehensive,
    };

    public enum EnableMiscue
    {
        False,
        True,
    };
}