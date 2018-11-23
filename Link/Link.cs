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



        public Link(int buffSize)
        {
            // Create a new SerialPort object with default settings.

            serialPort = serialPort ?? new SerialPort("/dev/ttyS1", 115200, Parity.None, 8, StopBits.One);

            if (!serialPort.IsOpen)
                serialPort.Open();

            //  Vi har delimiters også, giver plads til 2 yderligere bytes. 
            buffer = new byte[(buffSize * 2) + 2];

            // Uncomment the next line to use timeout
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

        public void send(byte[] buf, int size)
        {
            //  Send data til Framing
            var dataCharCount = Enframe(buf, size);
            //  Skub igennem seriel porten.
            serialPort.Write(buf, 0, dataCharCount);
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
            // beregner størrelse på data efter "aframning"
            var size = Deframe(ref buf, dataSize);
            return size;
        }

        private int ReceiveData()
        {
            //  Vent på transmission af delimiter
            while (!InitiateSerialReception()) { }

            var dataSize = 0;

            while (dataSize < buffer.Length)
            {
                var dataIn = (byte)serialPort.ReadByte();
                buffer[dataSize++] = dataIn;
                //  Stop når vi når vores delimiter
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


        // Ved at bruge "A" som delimiter skal vi have lagt vores data ind i frames. og for stadig at kunne forstå A (og B) skal vi have oversat A til BC og B til BD

        private int Enframe(byte[] buf, int size)
        {
            var replacedChars = 0;
            var framedChars = 0;

            //  se på en char: 
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
                    //  ellers indsæt  den char vi er kommet til 
                    buffer[framedChars++] = buf[replacedChars];
                }
                //  NEXT!
                replacedChars++;
            }
            //  Indsæt vores delimiter
            buffer[framedChars++] = DELIMITER;
            return framedChars;
        }

        //  Vi skal også kunne "Deframe" vores data

        private int Deframe(ref byte[] buf, int size)
        {
            var framedChars = 0;

            for (int i = 0; i < size - 1; i++)
            {
                //  Er char'en B
                if (buffer[i] == (byte)'B')
                {
                    //  Hvis ja, er næste char D? -> så indsæt B
                    if (buffer[++i] == (byte)'D')
                    {
                        buf[framedChars++] = (byte)'B';
                    }
                    //  Ellers hvis den næste er C -> så indsæt A
                    else if (buffer[++i] == (byte)'C')
                    {
                        buf[framedChars++] = (byte)'A';
                    }
                }
                // ellers indsæt den næste Char
                buf[framedChars] = buffer[i];
            }

            return framedChars;
        }

        #endregion


    }
}
