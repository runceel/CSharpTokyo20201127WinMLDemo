using MahApps.Metro.Controls;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Windows.AI.MachineLearning;

namespace Windows10APIs.Samples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly ObservableCollection<string> _results = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();
            inkCanvas.DefaultDrawingAttributes.Width = 24;
            inkCanvas.DefaultDrawingAttributes.Height = 24;
            inkCanvas.DefaultDrawingAttributes.Color = Colors.White;
            ResultsItemsControl.ItemsSource = _results;
        }

        private MnistModel _model;

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _results.Clear();
            inkCanvas.Strokes.Clear();
        }
        private async void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            // Mnist.onnx がまだ読み込まれていない場合は読み込む
            if (_model == null)
            {
                var mnistModelFilePath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Mnist.onnx");
                var modelFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(mnistModelFilePath);
                _model = await MnistModel.CreateFromStreamAsync(modelFile);
            }

            // 手書きの内容を画像に変換してモデルで評価する
            var visual = await Helper.GetHandWrittenImageAsync(inkCanvas);
            var result = await _model.EvaluateAsync(new MnistInput
            {
                Input3 = ImageFeatureValue.CreateFromVideoFrame(visual),
            });

            // 評価結果を解析して認識した数字を表示する
            var orderedScores = result.Plus214_Output_0.GetAsVectorView()
                .Select((score, index) => (score, index))
                .OrderByDescending(x => x.score);

            _results.Clear();
            foreach (var text in orderedScores.Select(x => $"{x.index}(score: {x.score})"))
            {
                _results.Add(text);
            }

        }
    }

}

