using NAudio.Wave;

public class AudioRecorder
{
    private readonly WaveInEvent _waveIn;

    public AudioRecorder()
    {
        _waveIn = new WaveInEvent();
        _waveIn.WaveFormat = new WaveFormat(16000, 1);
    }

    public async Task<byte[]> RecordAudioAsync(TimeSpan duration)
    {
        using (var memoryStream = new MemoryStream())
        {
            bool isRecording = false;

            _waveIn.DataAvailable += (sender, e) =>
            {
                if (isRecording)
                {
                    memoryStream.Write(e.Buffer, 0, e.BytesRecorded);
                }
            };

            _waveIn.RecordingStopped += (sender, e) =>
            {
                isRecording = false;
            };

            _waveIn.StartRecording();
            isRecording = true;

            await Task.Delay(duration);

            _waveIn.StopRecording();

            if (memoryStream.Length == 0)
            {
                return null;
            }

            return memoryStream.ToArray();
        }
    }
}
