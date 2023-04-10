using Microsoft.ML.Data;
using System;
using Microsoft.ML;
using Microsoft.ML.Transforms.Text;

namespace ReallySimpleMl
{
    // Definição das classes para os dados de entrada e saída
    public class SentimentoInput
    {
        [LoadColumn(0)]
        public string Text { get; set; }

        [LoadColumn(1)]
        public bool Sentimento { get; set; }
    }

    public class SentimentoOutput
    {
        [ColumnName("PredictedLabel")]
        public bool Sentimento { get; set; }
    }

    public class Test
    {
        public void Ml()
        {
            // Criação do pipeline de processamento de texto
            var pipeline = new LearningPipeline();
            pipeline.Add(new TextLoader<SentimentoInput>("sentimentos.csv", separator: ","));
            pipeline.Add(new TextFeaturizer("Features", "Text"));
            pipeline.Add(new FastTreeBinaryClassifier());

            // Treinamento do modelo
            var model = pipeline.Train<SentimentoInput, SentimentoOutput>();

            // Teste do modelo
            var prediction = model.Predict(new SentimentoInput { Text = "Eu adorei esse filme!" });
            Console.WriteLine($"Sentimento: {prediction.Sentimento}");
        }
    }
}
