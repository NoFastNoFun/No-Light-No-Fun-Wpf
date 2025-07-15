using System;
using System.Text;

namespace Core.Messages {
    public class RemoteControlMessage {
        public byte CommandCode {
            get;
        }
        public string CursorName {
            get;
        }
        public string ClipName {
            get;
        }
        public byte LoopMode {
            get;
        }
        public ushort BPM {
            get;
        }

        RemoteControlMessage(byte code, string cursor, string clip, byte loop, ushort bpm) {
            CommandCode = code;
            CursorName = cursor;
            ClipName = clip;
            LoopMode = loop;
            BPM = bpm;
        }

        public static RemoteControlMessage Parse(byte[] buffer, int offset = 6) {
            byte code = buffer[offset++];
            ushort cLen = BitConverter.ToUInt16(buffer, offset);
            offset += 2;
            string cursor = cLen > 0
                ? Encoding.ASCII.GetString(buffer, offset, cLen)
                : string.Empty;
            offset += cLen;

            ushort pLen = BitConverter.ToUInt16(buffer, offset);
            offset += 2;
            string clip = pLen > 0
                ? Encoding.ASCII.GetString(buffer, offset, pLen)
                : string.Empty;
            offset += pLen;

            byte loop = 0;
            ushort bpm = 0;
            if (code == 1) {
                loop = buffer[offset++];
                bpm = BitConverter.ToUInt16(buffer, offset);
            }

            return new RemoteControlMessage(code, cursor, clip, loop, bpm);
        }
    }
}
