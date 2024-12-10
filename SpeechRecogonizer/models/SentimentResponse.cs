
namespace SpeechRecogonizer.models
{
    public class SentimentResponse
    {
        public string Kind { get; set; }
        public SentimentResults Results { get; set; }
    }

    public class SentimentResults
    {
        public List<SentimentDocument> Documents { get; set; }
        public List<object> Errors { get; set; }
        public string ModelVersion { get; set; }
    }

    public class SentimentDocument
    {
        public string Id { get; set; }
        public string Sentiment { get; set; }
        public ConfidenceScores ConfidenceScores { get; set; }
        public List<SentimentSentence> Sentences { get; set; }
        public List<object> Warnings { get; set; }
    }
    public class ConfidenceScores
    {
        public double Positive { get; set; }
        public double Neutral { get; set; }
        public double Negative { get; set; }
    }

    // Sentences within a document, including sentiment and confidence scores
    public class SentimentSentence
    {
        public string Sentiment { get; set; }
        public ConfidenceScores ConfidenceScores { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public string Text { get; set; }
        public List<SentimentTarget> Targets { get; set; }
        public List<SentimentAssessment> Assessments { get; set; }
    }

    public class SentimentTarget
    {
        public string Sentiment { get; set; }
        public ConfidenceScores ConfidenceScores { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public string Text { get; set; }
        public List<Relation> Relations { get; set; }
    }

    public class Relation
    {
        public string RelationType { get; set; }
        public string Ref { get; set; }
    }

    public class SentimentAssessment
    {
        public string Sentiment { get; set; }
        public ConfidenceScores ConfidenceScores { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public string Text { get; set; }
        public bool IsNegated { get; set; }
    }
}
