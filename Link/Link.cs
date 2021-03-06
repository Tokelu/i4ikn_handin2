using System;
using System.IO.Ports;

/// <summary>
/// Link.
/// </summary>
namespace Linklaget
{
    /// <summary>
    /// Link.
    /// </summary>
    public class Link
    {
        /// <summary>
        /// The DELIMITER for slip protocol.
        /// </summary>
        const byte DELIMITER = (byte)'A';
        /// <summary>
        /// The buffer for link.
	    /// </summary>
	    private readonly byte[] buffer;
        /// <summary>
        /// The serial port.
        /// </summary>
        private static SerialPort serialPort = null;
        /// <summary>
        /// Initializes a new instance of the <see cref="link"/> class.
        /// </summary>


        //  Jeg ved ikke helt hvordan vi skulle bruge APP til at koble mellem vores klient/Server... 
        //  public Link (int BUFSIZE, string APP)

        public Link(int buffSize)
        {
            // Create a new SerialPort object with default settings.

            serialPort = serialPort ?? new SerialPort("/dev/ttyS1", 115200, Parity.None, 8, StopBits.One);

            if (!serialPort.IsOpen)
                serialPort.Open();

            //  Vi har delimiters ogs�, giver plads til 2 yderligere bytes. 
            buffer = new byte[(buffSize * 2) + 2];

            serialPort.ReadTimeout = 500;

            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
        }




        #region Rx/Tx
        /// <summary>
        /// Send the specified buf and size.
        /// </summary>
        /// <param name='buf'>
        /// Buffer.
        /// </param>
        /// <param name='size'>
        /// Size.
        /// </param>

        public void Send(byte[] buf, int size)
        {
            //  Send data til Framing
            var dataCharCount = Enframe(buf, size);
            //  Skub igennem seriel porten.
            serialPort.Write(buffer, 0, dataCharCount);
        }

        /// <summary>
        /// Receive the specified buf and size.
        /// </summary>
        /// <param name='buf'>
        /// Buffer.
        /// </param>
        /// <param name='size'>
        /// Size.
        /// </param>
        public int ReceiveData(ref byte[] buf)
        {
            //  Sender data til "Aframning"
            var dataSize = ReceiveData();
            // beregner st�rrelse p� data efter "aframning"
            var size = Deframe(ref buf, dataSize);
            return size;
        }

        private int ReceiveData()
        {
            //  Vent p� transmission af delimiter
            while (!InitiateSerialReception()) { }

            var dataSize = 0;

            while (dataSize < buffer.Length)
            {
                var dataIn = (byte)serialPort.ReadByte();
                buffer[dataSize++] = dataIn;
                //  Stop n�r vi n�r vores delimiter
                if (dataIn == DELIMITER) break;
            }
            return dataSize;
        }

        private bool InitiateSerialReception()
        {
            var dataIn = (byte)serialPort.ReadByte();
            return (dataIn == DELIMITER);
        }
        #endregion


        #region Frames


        // Ved at bruge "A" som delimiter skal vi have lagt vores data ind i frames. og for stadig at kunne forst� A (og B) skal vi have oversat A til BC og B til BD

        private int Enframe(byte[] buf, int size)
        {
            var replacedChars = 0;
            var framedChars = 0;

            //  se p� en char: 
            while (replacedChars < size)
            {
                //  Er char'en samme som vores delimiter (A) ?
                if (buf[replacedChars] == DELIMITER)
                {
                    //  Skift ud med BC
                    buffer[framedChars++] = (byte)'B';
                    buffer[framedChars++] = (byte)'C';
                }
                //  Er char'en B 
                else if (buf[replacedChars] == (byte)'B')
                {
                    //  Skift ud med BD
                    buffer[framedChars++] = (byte)'B';
                    buffer[framedChars++] = (byte)'D';
                }
                else
                {
                    //  ellers inds�t  den char vi er kommet til 
                    buffer[framedChars++] = buf[replacedChars];
                }
                //  NEXT!
                replacedChars++;
            }
            //  Inds�t vores delimiter
            buffer[framedChars++] = DELIMITER;
            return framedChars;
        }

        //  Vi skal ogs� kunne "Deframe" vores data

        private int Deframe(ref byte[] buf, int size)
        {
            var framedChars = 0;

            for (int i = 0; i < size - 1; i++)
            {
                //  Er char'en B
                if (buffer[i] == (byte)'B')
                {
                    //  Hvis ja, er n�ste char D? -> s� inds�t B
                    if (buffer[++i] == (byte)'D')
                    {
                        buf[framedChars++] = (byte)'B';
                    }
                    //  Ellers hvis den n�ste er C -> s� inds�t A
                    else if (buffer[++i] == (byte)'C')
                    {
                        buf[framedChars++] = (byte)'A';
                    }
                }
                // ellers inds�t den n�ste Char
                buf[framedChars] = buffer[i];
            }

            return framedChars;
        }

        #endregion


    }
}
