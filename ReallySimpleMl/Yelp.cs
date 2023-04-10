using Microsoft.ML;
using Microsoft.ML.Data;
using Spectre.Console;
using Console = Spectre.Console.AnsiConsole;

namespace ReallySimpleMl
{
    public class Yelp
    {
        private readonly MLContext ctx;
        private readonly IDataView dataView;
        private readonly ITransformer model;

        public Yelp()
        {
            ctx = new MLContext();
            dataView = ctx.Data
                .LoadFromTextFile<SentimentData>("yelp.txt");
        }

        public void TrainMl()
        {
            // split data into testing set
            var splitDataView = ctx.Data
                .TrainTestSplit(dataView, testFraction: 0.2);

            // build model
            var estimator = ctx.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))
                .Append(ctx.BinaryClassification.Trainers.SdcaLogisticRegression("Features"));

            // Train model
            ITransformer model = default!;
            var rule = new Spectre.Console.Rule("Create and Train Model");
            Console
                .Live(rule)
                .Start(console =>
                {
                    // training happens here
                    model = estimator.Fit(splitDataView.TrainSet);
                    var predictions = model.Transform(splitDataView.TestSet);
                    rule.Title = "🏁 Training Complete, Evaluating Accuracy.";
                    console.Refresh();
                    // evaluate the accuracy of our model
                    var metrics = ctx.BinaryClassification.Evaluate(predictions);
                    var table = new Table()
                        .MinimalBorder()
                        .Title("💯 Model Accuracy");
                    table.AddColumns("Accuracy", "Auc", "F1Score");
                    table.AddRow($"{metrics.Accuracy:P2}", $"{metrics.AreaUnderRocCurve:P2}", $"{metrics.F1Score:P2}");
                    console.UpdateTarget(table);
                    console.Refresh();
                });
        }

        public void SaveModel() => // save to disk
            ctx.Model.Save(model, dataView.Schema, "model.zip");

        public void LoadModel(out DataViewSchema? schema) => // load from disk
            ctx.Model.Load("model.zip", out schema);

        public void ExecuteMl()
        {
            while (true)
            {
                var text = Console.Ask<string>("What's your [green]review text[/]?");
                var engine = ctx.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
                var input = new SentimentData { Text = text };
                var result = engine.Predict(input);
                var (color, emoji) = result.Prediction
                    ? (color: "green", emoji: "👍")
                    : (color: "red", emoji: "👎");
                Console.MarkupLine($"{emoji} [{color}]\"{text}\" ({result.Probability:P00})[/] ");
            }
        }
    }

    internal class SentimentData
    {
        [LoadColumn(0)] public string? Text;
        [LoadColumn(1), ColumnName("Label")] public bool Sentiment;
    }

    internal class SentimentPrediction : SentimentData
    {
        [ColumnName("PredictedLabel")] public bool Prediction { get; set; }
        public float Probability { get; set; }
        public float Score { get; set; }
    }
}
