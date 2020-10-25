using System.ComponentModel;
using System.IO;
using System.Linq;
using NAudio.Utils;
using NAudio.Wave;


namespace MSCognitiveService.Library
{
    public class AudioVoiceService
    {
        private byte[] _result;
        private Stream _memoryStream;

        private WaveInEvent _waveIn;
        public AudioVoiceService()
        {
            CreateService();
        }

        public void CreateService()
        {
            _waveIn = new WaveInEvent();
            _memoryStream = new MemoryStream();
            //var writer = new WaveFileWriter("out.wav", _waveIn.WaveFormat);
            var writer = new WaveFileWriter(new IgnoreDisposeStream(_memoryStream), this._waveIn.WaveFormat);
            _waveIn.DataAvailable += (s, a) =>
            {
                writer.Write(a.Buffer, 0, a.BytesRecorded);
                if (writer.Position > _waveIn.WaveFormat.AverageBytesPerSecond * 30)
                {
                    _waveIn.StopRecording();
                }
            };

            _waveIn.RecordingStopped += (s, a) =>
            {
                writer?.Dispose();
                writer = null;
            };
        }


        public void StartRecording()
        {
            _waveIn.StartRecording();
        }

        public void StopRecording()
        {
           _waveIn.StopRecording();
        }

        public byte[] GetRecordResult()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                _memoryStream.CopyTo(ms);
                _result= ms.ToArray();
            }

            return _result;
        }
    }
}
