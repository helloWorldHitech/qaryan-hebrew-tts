//    This file is part of Qaryan.
//
//    Qaryan is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Qaryan is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Qaryan.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Qaryan.Audio
{
    public class StreamAudioTarget: AudioTarget
    {
        Stream stream;

        public Stream Stream
        {
            get
            {
                return stream;
            }
            set
            {
                stream = value;
            }
        }

        public bool WriteHeader = true;

        WaveStreamWriter writer;

        protected override void Open(WaveFormat format)
        {
             writer = new WaveStreamWriter(Stream, format.Channels, format.SamplesPerSecond, format.AverageBytesPerSecond, format.BlockAlign, format.BitsPerSample, WriteHeader);
        }

        protected override void PlayBuffer(AudioBufferInfo buffer)
        {
            writer.WriteData(buffer.Data,0,buffer.Data.Length);
        }

        protected override void Close()
        {
            if (writer != null)
            {
                writer.Flush();
                writer.Close();
            }
        }

        public void PlaySync()
        {
            this.Join();
        }

        public override void Stop()
        {
//            throw new Exception("The method or operation is not implemented.");
        }
    }
}
