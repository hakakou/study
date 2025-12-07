using OpenAI.Files;
using OpenAI.FineTuning;
using System.ClientModel;
using System.Text.Json.Serialization;

namespace OpenAI;

public class FineTuningClientTests
{
    private FineTuningClient client;

    [SetUp]
    public void Setup()
    {
        client = new FineTuningClient(
            "sk-proj-XXX");
    }

    [Test]
    public async Task TestFileUpload()
    {
        using var jsonStream = File.OpenRead("FineTuneExample.json");
        //var job = await client.GetJobAsync(BinaryContent.Create(jsonStream), waitUntilCompleted: true);
        //Console.WriteLine(job.Dump());
    }

    [Test]
    public void BasicFineTuningJobOperationsWork()
    {
        // https://platform.openai.com/docs/guides/fine-tuning
        // https://platform.openai.com/docs/api-reference/fine-tuning
        // https://github.com/openai/openai-dotnet/blob/main/tests/FineTuning/FineTuningTests.cs

        void UploadFiles()
        {
            OpenAIFileClient fileClient = new OpenAIFileClient("sk-proj-XXX");

            BinaryData fileContent = BinaryData.FromString("""
    {"messages": [{"role": "system", "content": "Your task is to classify a piece of text into the following category labels: [\"Cancer_present\", \"Cancer_past\", \"Cancer_relative\", \"No_Cancer\"]."}, {"role": "user", "content": "She was diagnosed with stage 3 colon cancer last month."}, {"role": "assistant", "content": "Cancer_present"}]}
    {"messages": [{"role": "system", "content": "Your task is to classify a piece of text into the following category labels: [\"Cancer_present\", \"Cancer_past\", \"Cancer_relative\", \"No_Cancer\"]."}, {"role": "user", "content": "The oncologist confirmed that his symptoms were related to cancer."}, {"role": "assistant", "content": "Cancer_present"}]}
    {"messages": [{"role": "system", "content": "Your task is to classify a piece of text into the following category labels: [\"Cancer_present\", \"Cancer_past\", \"Cancer_relative\", \"No_Cancer\"]."}, {"role": "user", "content": "Her recent scans revealed malignant tumors in her lungs."}, {"role": "assistant", "content": "Cancer_present"}]}
    {"messages": [{"role": "system", "content": "Your task is to classify a piece of text into the following category labels: [\"Cancer_present\", \"Cancer_past\", \"Cancer_relative\", \"No_Cancer\"]."}, {"role": "user", "content": "She is currently undergoing chemotherapy for breast cancer."}, {"role": "assistant", "content": "Cancer_present"}]}
    {"messages": [{"role": "system", "content": "Your task is to classify a piece of text into the following category labels: [\"Cancer_present\", \"Cancer_past\", \"Cancer_relative\", \"No_Cancer\"]."}, {"role": "user", "content": "He received a positive test result confirming prostate cancer."}, {"role": "assistant", "content": "Cancer_present"}]}
    {"messages": [{"role": "system", "content": "Your task is to classify a piece of text into the following category labels: [\"Cancer_present\", \"Cancer_past\", \"Cancer_relative\", \"No_Cancer\"]."}, {"role": "user", "content": "She had cancer as a teenager, but she’s been in remission ever since."}, {"role": "assistant", "content": "Cancer_past"}]}
    {"messages": [{"role": "system", "content": "Your task is to classify a piece of text into the following category labels: [\"Cancer_present\", \"Cancer_past\", \"Cancer_relative\", \"No_Cancer\"]."}, {"role": "user", "content": "After five years of being cancer-free, he still goes for regular checkups."}, {"role": "assistant", "content": "Cancer_past"}]}
    {"messages": [{"role": "system", "content": "Your task is to classify a piece of text into the following category labels: [\"Cancer_present\", \"Cancer_past\", \"Cancer_relative\", \"No_Cancer\"]."}, {"role": "user", "content": "She survived cervical cancer and has been in good health since."}, {"role": "assistant", "content": "Cancer_past"}]}
    {"messages": [{"role": "system", "content": "Your task is to classify a piece of text into the following category labels: [\"Cancer_present\", \"Cancer_past\", \"Cancer_relative\", \"No_Cancer\"]."}, {"role": "user", "content": "His test results confirmed that he’s fully recovered from skin cancer."}, {"role": "assistant", "content": "Cancer_past"}]}
    {"messages": [{"role": "system", "content": "Your task is to classify a piece of text into the following category labels: [\"Cancer_present\", \"Cancer_past\", \"Cancer_relative\", \"No_Cancer\"]."}, {"role": "user", "content": "My mom battled breast cancer for two years but is now in remission."}, {"role": "assistant", "content": "Cancer_relative"}]}
    """);
            OpenAIFile uploadedFile = fileClient.UploadFile(fileContent, "data.json", FileUploadPurpose.FineTune);
            Console.WriteLine(uploadedFile.Dump());

            fileContent = BinaryData.FromString("""
            {"messages": [{"role": "system", "content": "Your task is to classify a piece of text into the following category labels: [\"Cancer_present\", \"Cancer_past\", \"Cancer_relative\", \"No_Cancer\"]."}, {"role": "user", "content": "His father passed away from lung cancer, which makes him worried about his own risk."}, {"role": "assistant", "content": "Cancer_relative"}]}
            {"messages": [{"role": "system", "content": "Your task is to classify a piece of text into the following category labels: [\"Cancer_present\", \"Cancer_past\", \"Cancer_relative\", \"No_Cancer\"]."}, {"role": "user", "content": "Her cousin was recently diagnosed with early-stage cervical cancer."}, {"role": "assistant", "content": "Cancer_relative"}]}
            {"messages": [{"role": "system", "content": "Your task is to classify a piece of text into the following category labels: [\"Cancer_present\", \"Cancer_past\", \"Cancer_relative\", \"No_Cancer\"]."}, {"role": "user", "content": "She’s worried because her sister was diagnosed with breast cancer last year."}, {"role": "assistant", "content": "Cancer_relative"}]}
            {"messages": [{"role": "system", "content": "Your task is to classify a piece of text into the following category labels: [\"Cancer_present\", \"Cancer_past\", \"Cancer_relative\", \"No_Cancer\"]."}, {"role": "user", "content": "His mother is currently fighting colon cancer, so he decided to get checked."}, {"role": "assistant", "content": "Cancer_relative"}]}
            {"messages": [{"role": "system", "content": "Your task is to classify a piece of text into the following category labels: [\"Cancer_present\", \"Cancer_past\", \"Cancer_relative\", \"No_Cancer\"]."}, {"role": "user", "content": "Her father’s history of prostate cancer has made her more cautious about her health."}, {"role": "assistant", "content": "Cancer_relative"}]}
            """);
            OpenAIFile uploadedFileValidation = fileClient.UploadFile(fileContent, "data-val.json", FileUploadPurpose.FineTune);
        }

        // https://platform.openai.com/docs/api-reference/fine-tuning/create

        string json = """
{ "training_file": "file-Y54PebyxrhwjWd9N1WKf6C", "validation_file": "file-8ukgBCEFibrua9MkHsxeTA",  "model": "gpt-3.5-turbo" }
""";
        BinaryData input = BinaryData.FromString(json);
        using BinaryContent content = BinaryContent.Create(input);

        //var operation = client.CreateFineTuningJob(content, waitUntilCompleted: false);
        //Console.WriteLine(operation.Dump());
    }

    public partial class Message
    {
        [JsonPropertyName("role")]
        public Role Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("weight")]
        public long? Weight { get; set; }
    }

    public enum Role
    { Assistant, System, User };
}