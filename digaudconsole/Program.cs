using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DirectX.AudioVideoPlayback;
using System.IO;
using System.Xml;

namespace digaudconsole
{
    class Program
    {
        public static wavefile waveIn;
        public static timefreq stftRep;
        public static float[] pixelArray;
        public static musicnote[] sheetmusic;
        public enum pitchConv { C, Db, D, Eb, E, F, Gb, G, Ab, A, Bb, B };
        public static double bpm = 70;

        static void Main(string[] args)
        {
            start();
        }

        static void start()
        {
            string filename = "C:\\stuff\\jupiter.wav";
            string xmlfile = "C:\\stuff\\jupiter.xml";
            loadWave(filename);
            freqDomain();
            sheetmusic = readXML(xmlfile);
           
        }

        public static musicnote[] readXML(string filename)
        {

            List<string> stepList = new List<string>(100);
            List<int> octaveList = new List<int>(100);
            List<int> durationList = new List<int>(100);
            List<int> alterList = new List<int>(100);
            int noteCount = 0;
            bool sharp;
            musicnote[] scoreArray;

            FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
            if (file == null)
            {
                System.Console.Write("Failed to Open File!");
            }

            XmlTextReader reader = new XmlTextReader(filename);

            bool finished = false;

            while (finished == false)
            {
                sharp = false;
                while ((!reader.Name.Equals("note") || reader.NodeType == XmlNodeType.EndElement) && !finished)
                {
                    reader.Read();
                    if (reader.ReadState == ReadState.EndOfFile)
                    {
                        finished = true;
                    }
                }

                reader.Read();
                reader.Read();
                if (reader.Name.Equals("rest"))
                {
                }
                else if (reader.Name.Equals("pitch"))
                {

                    while (!reader.Name.Equals("step"))
                    {
                        reader.Read();
                    }
                    reader.Read();
                    stepList.Add(reader.Value);
                    while (!reader.Name.Equals("octave"))
                    {
                        if (reader.Name.Equals("alter") && reader.NodeType == XmlNodeType.Element)
                        {
                            reader.Read();
                            alterList.Add(int.Parse(reader.Value));
                            sharp = true;
                        }
                        reader.Read();
                    }
                    reader.Read();
                    if (!sharp)
                    {
                        alterList.Add(0);
                    }
                    sharp = false;
                    octaveList.Add(int.Parse(reader.Value));
                    while (!reader.Name.Equals("duration"))
                    {
                        reader.Read();
                    }
                    reader.Read();
                    durationList.Add(int.Parse(reader.Value));
                    //System.Console.Out.Write("Note ~ Pitch: " + stepList[noteCount] + alterList[noteCount] + " Octave: " + octaveList[noteCount] + " Duration: " + durationList[noteCount] + "\n");
                    noteCount++;

                }

            }

            scoreArray = new musicnote[noteCount];

            double c0 = 16.351625;

            for (int nn = 0; nn < noteCount; nn++)
            {
                int step = (int)Enum.Parse(typeof(pitchConv), stepList[nn]);

                double freq = c0 * Math.Pow(2, octaveList[nn]) * (Math.Pow(2, ((double)step + (double)alterList[nn]) / 12));
                scoreArray[nn] = new musicnote(freq, (double)durationList[nn] * 60 * waveIn.SampleRate / (4 * bpm));

            }

            return scoreArray;
        }

        public static void freqDomain()
        {
            stftRep = new timefreq(waveIn.wave, 2048);
            pixelArray = new float[stftRep.timeFreqData[0].Length * stftRep.wSamp / 2];
            for (int jj = 0; jj < stftRep.wSamp / 2; jj++)
            {
                for (int ii = 0; ii < stftRep.timeFreqData[0].Length; ii++)
                {
                    pixelArray[jj * stftRep.timeFreqData[0].Length + ii] = stftRep.timeFreqData[jj][ii];
                }
            }

        }

        public static void loadWave(string filename)
        {
            // Sound File
            FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
            if (file == null)
            {
                System.Console.Write("Failed to Open File!");
            }
            else
            {
                waveIn = new wavefile(file);
            }

        }
    }
}
