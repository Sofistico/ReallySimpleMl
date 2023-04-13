using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.TorchSharp;
using Microsoft.ML.TorchSharp.NasBert;
using System;
using Console = Spectre.Console.AnsiConsole;

namespace ReallySimpleMl
{
    public class Turtles
    {
        private readonly MLContext mlContext;
        private readonly IDataView data;

        public Turtles()
        {
            mlContext = new()
            {
                GpuDeviceId = 0,
                FallbackToCpu = true
            };

            Console.WriteLine("Loading data...");
            data = mlContext.Data.LoadFromTextFile<ModelTurtleInput>("turtles.tsv", separatorChar: '\t', hasHeader: false);
        }

        public ITransformer TrainModel()
        {
            // To evaluate the effectiveness of machine learning models we split them into a training set for fitting
            // and a testing set to evaluate that trained model against unknown data
            DataOperationsCatalog.TrainTestData dataSplit = mlContext.Data.TrainTestSplit(data, testFraction: 0.2, seed: 1234);
            IDataView trainData = dataSplit.TrainSet;
            IDataView testData = dataSplit.TestSet;

            Console.WriteLine("Traning model...");

            // Create a pipeline for training the model
            var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", "Label")
                .Append(mlContext.MulticlassClassification.Trainers.TextClassification(labelColumnName: "Label",
                    sentence1ColumnName: "Sentence",
                    architecture: BertArchitecture.Roberta))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue(
                    outputColumnName: "PredictedLabel",
                    inputColumnName: "PredictedLabel"));
            var model = pipeline.Fit(trainData);

            // Evaluate the model's performance against the TEST data set
            Console.WriteLine("Evaluating model performance...");
            // We need to apply the same transformations to our test set so it can be evaluated via the resulting model
            IDataView transformedTest = model.Transform(testData);
            MulticlassClassificationMetrics metrics = mlContext.MulticlassClassification.Evaluate(transformedTest);

            // Display Metrics
            Console.WriteLine($"Macro Accuracy: {metrics.MacroAccuracy}");
            Console.WriteLine($"Micro Accuracy: {metrics.MicroAccuracy}");
            Console.WriteLine($"Log Loss: {metrics.LogLoss}");
            Console.WriteLine();

            // List different intents with their class number
            Console.WriteLine("Classes:");
            foreach (TurtleIntents value in Enum.GetValues<TurtleIntents>())
            {
                Console.WriteLine($"{(int)value}: {value}");
            }

            // Generate the table for diagnostics
            Console.WriteLine(metrics.ConfusionMatrix.GetFormattedConfusionTable());

            return model;
        }

        public void ExecuteModel(ITransformer model)
        {
            // Generate a prediction engine
            Console.WriteLine("Creating prediction engine...");
            PredictionEngine<ModelTurtleInput, ModelTurtleOutput> engine =
                mlContext.Model.CreatePredictionEngine<ModelTurtleInput, ModelTurtleOutput>(model);

            Console.WriteLine("Ready to generate predictions.");
            string input;
            do
            {
                Console.WriteLine();
                input = Console.Ask<string>("What do you want to say about [bold green]turtles[/]? (Type Q to Quit)")!;

                // Get a prediction
                ModelTurtleInput sampleData = new(input);
                ModelTurtleOutput result = engine.Predict(sampleData);

                // Print classification
                float maxScore = result.Score[(uint)result.PredictedLabel];
                Console.WriteLine($"Matched intent {(TurtleIntents)result.PredictedLabel} with score of {maxScore:f2}");
                Console.WriteLine();
            } while (!string.IsNullOrWhiteSpace(input) && input?.ToLowerInvariant() != "q");

            Console.WriteLine("Have fun with turtles!");
        }
    }

    public class ModelTurtleInput
    {
        [LoadColumn(0)]
        [ColumnName("Sentence")]
        public string? Sentence { get; set; }

        [LoadColumn(1)]
        [ColumnName("Label")]
        public float Label { get; set; }

        public ModelTurtleInput(string sentence)
        {
            Sentence = sentence;
        }
    }

    public class ModelTurtleOutput
    {
        [ColumnName(@"Sentence")]
        public string Sentence { get; set; }

        [ColumnName(@"Label")]
        public uint Label { get; set; }

        [ColumnName(@"PredictedLabel")]
        public float PredictedLabel { get; set; }

        [ColumnName(@"Score")]
        public float[] Score { get; set; }
    }

    public enum TurtleIntents
    {
        EatTurtle = 0,
        LikeTurtle = 1,
        Unknown = 2,
        Ninjitsu = 3,
        TurtleCare = 4,
    }
}
