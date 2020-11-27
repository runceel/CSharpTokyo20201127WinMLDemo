using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.AI.MachineLearning;

namespace Windows10APIs.Samples
{

    public sealed class MnistInput
    {
        public ImageFeatureValue Input3; // shape(1,1,28,28)
    }
    
    public sealed class MnistOutput
    {
        public TensorFloat Plus214_Output_0; // shape(1,10)
    }
    
    public sealed class MnistModel
    {
        private LearningModel model;
        private LearningModelSession session;
        private LearningModelBinding binding;
        public static async Task<MnistModel> CreateFromStreamAsync(IRandomAccessStreamReference stream)
        {
            MnistModel learningModel = new MnistModel();
            learningModel.model = await LearningModel.LoadFromStreamAsync(stream);
            learningModel.session = new LearningModelSession(learningModel.model);
            learningModel.binding = new LearningModelBinding(learningModel.session);
            return learningModel;
        }
        public async Task<MnistOutput> EvaluateAsync(MnistInput input)
        {
            binding.Bind("Input3", input.Input3);
            var result = await session.EvaluateAsync(binding, "0");
            var output = new MnistOutput();
            output.Plus214_Output_0 = result.Outputs["Plus214_Output_0"] as TensorFloat;
            return output;
        }
    }
}

