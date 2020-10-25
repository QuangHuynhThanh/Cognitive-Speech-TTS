using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using MSCognitiveService.Model;
using Newtonsoft.Json.Linq;

namespace MSCognitiveService.Library
{
    public class MsCognitiveServiceService
    {
        public MsCognitiveModel CognitiveModel;

        public string ServiceKey;

        private readonly ManualResetEvent _allDone = new ManualResetEvent(false);
        private JToken _serviceResult;

        public MsCognitiveServiceService(MsCognitiveModel model, string serviceKey)
        {
            CognitiveModel = model;
            ServiceKey = serviceKey;
        }

        public JToken CallApi(string filePath)
        {
            var request = SetupApiRequest();

            ReadFileStream(filePath, request);

            request.BeginGetRequestStream(GetRequestStreamCallback, request);
            return _serviceResult;
        }

        public JToken CallApi(byte[]voiceRecord)
        {
            var request = SetupApiRequest();

            ReadRecordStream(voiceRecord, request);

            request.BeginGetRequestStream(GetRequestStreamCallback, request);
            return _serviceResult;
        }

        private HttpWebRequest SetupApiRequest()
        {
            var request = (HttpWebRequest) WebRequest.Create(GenerateServiceUrl());
            request.SendChunked = true;
            request.Accept = @"application/json;text/xml";
            request.Method = "POST";
            request.ProtocolVersion = HttpVersion.Version11;
            //request.Host = authentication.GetHost();
            request.ContentType = @"audio/wav; codecs=audio/pcm; samplerate=16000";
            request.Headers["Ocp-Apim-Subscription-Key"] = ServiceKey;
            if (CognitiveModel.AssessmentModel.IsOn)
            {
                request.Headers["Pronunciation-Assessment"] = GenerateAssessmentHeader();
            }

            request.AllowWriteStreamBuffering = false;
            return request;
        }

        private static void ReadRecordStream(byte[] voiceStream, HttpWebRequest request)
        {
            using (var requestStream = request.GetRequestStream())
            {
                using (var writer = new BinaryWriter(requestStream))
                {
                    foreach (var value in voiceStream)
                    {
                        writer.Write(value);
                    }
                }
                requestStream.Flush();
            }
        }

        private static void ReadFileStream(string filePath, HttpWebRequest request)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // Open a request stream and write 1024 byte chunks in the stream one at a time.
                using (var requestStream = request.GetRequestStream())
                {
                    // Read 1024 raw bytes from the input audio file.
                    var buffer = new byte[checked((uint) Math.Min(1024, (int) fs.Length))];
                    int bytesRead;
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                    }

                    requestStream.Flush();
                }
            }
        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            var request = (HttpWebRequest) asynchronousResult.AsyncState;

            // End the operation
            var postStream = request.EndGetRequestStream(asynchronousResult);

            postStream.Close();

            // Start the asynchronous operation to get the response
            request.BeginGetResponse(GetResponseCallback, request);
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            var request = (HttpWebRequest) asynchronousResult.AsyncState;

            // End the operation
            var response = (HttpWebResponse) request.EndGetResponse(asynchronousResult);
            var streamResponse = response.GetResponseStream();
            if (streamResponse == null) 
                return;

            var streamRead = new StreamReader(streamResponse);
            var responseString = streamRead.ReadToEnd();
            Console.WriteLine(responseString);
            // Close the stream object
            streamResponse.Close();
            streamRead.Close();

            // Release the HttpWebResponse
            response.Close();
            _allDone.Set();

            _serviceResult = JToken.Parse(responseString);
        }

        private string GenerateAssessmentHeader()
        {
            var pronAssessmentParamsJson =
                $"{{\"ReferenceText\":\"{CognitiveModel.AssessmentModel.ReferenceText}\",\"GradingSystem\":\"{CognitiveModel.AssessmentModel.GradingSystem}\",\"Granularity\":\"{CognitiveModel.AssessmentModel.Granularity}\",\"Dimension\":\"{CognitiveModel.AssessmentModel.Dimension}\",\"EnableMiscue\":\"{CognitiveModel.AssessmentModel.EnableMiscue}\",\"ScenarioId\":\"{CognitiveModel.AssessmentModel.ScenarioId}\"}}";
            var pronScoreParamsBytes = Encoding.UTF8.GetBytes(pronAssessmentParamsJson);
            return Convert.ToBase64String(pronScoreParamsBytes);

        }

        private string GenerateServiceUrl()
        {
            return string.Format(CognitiveModel.ApiUrlFormat, CognitiveModel.Region, CognitiveModel.Language,
                CognitiveModel.AssessmentModel.IsAssessDetail ? "&format=detailed" : string.Empty);
        }

    }
}
