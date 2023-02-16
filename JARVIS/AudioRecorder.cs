using NAudio.Wave;

public class AudioRecorder
{
    private readonly WaveInEvent _waveIn;
    private readonly MemoryStream _memoryStream;

    public AudioRecorder()
    {
        _waveIn = new WaveInEvent();
        _waveIn.WaveFormat = new WaveFormat(16000, 1);
        _waveIn.BufferMilliseconds = 10;

        _memoryStream = new MemoryStream();
    }

    public async Task<byte[]> RecordAudioAsync(int noiseGateThreshold = 1000)
    {
        _memoryStream.Seek(0, SeekOrigin.Begin);
        _memoryStream.SetLength(0);

        bool isRecording = false;
        bool isSilent = false;
        int silentFrames = 0;

        _waveIn.DataAvailable += (sender, e) =>
        {
            if (isRecording)
            {
                for (int i = 0; i < e.BytesRecorded; i += 2)
                {
                    short sample = BitConverter.ToInt16(e.Buffer, i);
                    if (Math.Abs((double)sample) > noiseGateThreshold)
                    {
                        _memoryStream.Write(e.Buffer, i, 2);
                        isSilent = false;
                        silentFrames = 0;
                    }
                    else
                    {
                        _memoryStream.WriteByte(0);
                        _memoryStream.WriteByte(0);
                        isSilent = true;
                    }
                }
            }
        };

        _waveIn.RecordingStopped += (sender, e) =>
        {
            isRecording = false;
        };

        _waveIn.StartRecording();
        isRecording = true;

        while (true)
        {
            await Task.Delay(10);

            if (isSilent)
            {
                silentFrames++;

                if (silentFrames >= 20) // 200ms of silence
                {
                    break;
                }
            }
            else
            {
                silentFrames = 0;
            }
        }

        _waveIn.StopRecording();

        if (_memoryStream.Length == 0)
        {
            return null;
        }

        return _memoryStream.ToArray();
    }
}