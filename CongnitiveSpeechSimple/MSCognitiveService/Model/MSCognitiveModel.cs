namespace MSCognitiveService.Model
{
    public class MsCognitiveModel
    {

        public MsCognitiveAssessmentModel AssessmentModel { get; set; }

        public string Region { get; set; }

        public string Language { get; set; }

        public string ApiUrlFormat =
            "https://{0}.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1?language={1}{2}";
        

    }
}
