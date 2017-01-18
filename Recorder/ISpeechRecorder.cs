using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechProcessing.Recorder
{
    public interface ISpeechRecorder
    {
        void SetFileName(string fileName);
        void StartRec();
        void StopRec();
    }

    public interface CopyOfISpeechRecorder
    {
        void SetFileName(string fileName);
        void StartRec();
        void StopRec();
    }
}
