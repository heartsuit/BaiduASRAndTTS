# Constructure
![Presentation](https://github.com/heartsuit/BaiduASRAndTTS/raw/master/screenshot/classdiagram.png)

## Interface and Class
- SpeechModel: A class contains almost all the parameters the Baidu API need to achieve ASR and TTS.
- WavInfo: A struct contains the basic element of one Wave object.
- AutomaticSpeechRecognition: Class does the 'Get access token' and 'recognition' work, mainly send HTTP post request.
- ClassUtils: Some utility methods grouped together.

- ISpeechRecorder: Interface, define 3 methods to be implemented: SetFileName, StartRec, StopRec.
- DirectRecorder and NAudioRecorder: Two different implementation of ISpeechRecorder.

- Form: Handle most UI controls' action, response to button action, and other event.

## UI
![Presentation](https://github.com/heartsuit/BaiduASRAndTTS/raw/master/screenshot/speech.png)
### Features
1. Select a file to recognize.
2. Speech---> Text: Record from user and translate speech to text.
3. Text---> Speech: Read the text in textBox.
4. Pause and Stop.
5. Drag and drop text file into the text area.

## Note
- The key and secret key has been erased from the source code, so you need to add your own when the token expired.
- The token.dat in bin/Debug, contains the generated token(first line) and the time it was generated(second line).
- When using the DirectRecorder version to do record work, set Exception Settings(Ctrl+Alt+E)--->Managed Debugging Assistants--->LoaderLock(Uncheck this).


