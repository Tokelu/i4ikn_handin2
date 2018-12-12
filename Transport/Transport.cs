using System;
using Linklaget;

/// <summary>
/// Transport.
/// </summary>
namespace Transportlaget
{
    /// <summary>
    /// Transport.
    /// </summary>
    public class Transport
    {
        /// <summary>
        /// The link.
        /// </summary>
        private Link link;
        /// <summary>
        /// The 1' complements checksum.
        /// </summary>
        private Checksum checksum;
        /// <summary>
        /// The buffer.
        /// </summary>
        private byte[] buffer;
        /// <summary>
        /// The seq no.
        /// </summary>
        private byte seqNo;
        /// <summary>
        /// The error count.
        /// </summary>
        private int errorCount;
        /// <summary>
        /// The DEFAULT_SEQNO.
        /// </summary>
        private const int DEFAULT_SEQNO = 2;
        /// <summary>
        /// The number of data the recveived.
        /// </summary>
        private int recvSize = 0;

        //  til introduktion af bitfejl - simuleret fejl. 
        private int bitErrorIntroduction = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transport"/> class.
        /// </summary>
        public Transport(int BUFSIZE)
        {
            link = new Link(BUFSIZE + (int)TransSize.ACKSIZE);
            checksum = new Checksum();
            buffer = new byte[BUFSIZE + (int)TransSize.ACKSIZE];
            seqNo = 0;
            errorCount = 0;
        }

        /// <summary>
        /// Receives the ack.
        /// </summary>
        /// <returns>
        /// The ack.
        /// </returns>
        private byte receiveAck()
        {
            byte[] buffer = new byte[(int)TransSize.ACKSIZE];
            int size = link.ReceiveData(ref buffer);

            if (size != (int)TransSize.ACKSIZE) return DEFAULT_SEQNO;

            if (!checksum.checkChecksum(buffer, (int)TransSize.ACKSIZE)
                || buffer[(int)TransCHKSUM.SEQNO] != seqNo
                || buffer[(int)TransCHKSUM.TYPE] != (int)TransType.ACK)
                return DEFAULT_SEQNO;

            return seqNo;
        }

        /// <summary>
        /// Sends the ack.
        /// </summary>
        /// <param name='ackType'>
        /// Ack type.
        /// </param>
        private void SendAck(bool ackType)
        {
            byte[] ackBuf = new byte[(int)TransSize.ACKSIZE];
            ackBuf[(int)TransCHKSUM.SEQNO] = (byte) (ackType 
                ? buffer[(int)TransCHKSUM.SEQNO] 
                : (byte)(buffer[(int)TransCHKSUM.SEQNO] + 1) % 2);

            ackBuf[(int)TransCHKSUM.TYPE] = (byte)(int)TransType.ACK;
            checksum.calcChecksum(ref ackBuf, (int)TransSize.ACKSIZE);

            if (++bitErrorIntroduction == 2)
            {
                ackBuf[0]++;
                bitErrorIntroduction = 0;
            }

            link.Send(ackBuf, (int)TransSize.ACKSIZE);
        }

        /// <summary>
        /// Send the specified buffer and size.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        /// <param name='size'>
        /// Size.
        /// </param>
        public void Send(byte[] buf, int size)
        {
            buffer[(int)TransCHKSUM.SEQNO] = seqNo;
            buffer[(int)TransCHKSUM.TYPE] = (int)TransType.DATA;
            for (int i = 0; i < size; i++)
            {
                buffer[i + (int)TransSize.ACKSIZE] = buf[i];
            }
            size = size + (int)TransSize.ACKSIZE;
            checksum.calcChecksum(ref buffer, size);

            while (errorCount < 5)
            {
                try
                {
                    do
                    {
                        link.Send(buffer, size);
                    } while (receiveAck() != seqNo);

                    seqNo = (byte)((seqNo + 1) % 2);
                    break;
                }

                catch (TimeoutException)
                {
                    ++errorCount;
                }
            }
            errorCount = 0;
        }

        /// <summary>
        /// Receive the specified buffer.
        /// </summary>
        /// <param name='buffer'>
        /// Buffer.
        /// </param>
        public int Receive(ref byte[] buf)
        {
            // TO DO Your own code
            var readSize = 0;

            while (readSize == 0 && errorCount < 5)
            {
                try
                {
                    while ((readSize = link.ReceiveData(ref buffer)) > 0)
                    {
                        if (checksum.checkChecksum(buffer, readSize))
                        {
                            SendAck(true);
                            if (buffer[(int)TransCHKSUM.SEQNO] == seqNo)
                            {
                                seqNo = (byte)((seqNo + 1) % 2);
                                readSize = buf.Length < readSize - (int)TransSize.ACKSIZE
                                    ? buf.Length
                                    : readSize - (int)TransSize.ACKSIZE;
                                Array.Copy(buffer, (int)TransSize.ACKSIZE, buf, 0, readSize);
                                break;
                            }
                        }
                        SendAck(false);
                    }
                }
                catch (TimeoutException)
                {
                    readSize = 0;
                    ++errorCount;
                }
            }
            errorCount = 0;
            return readSize;
        }
    }
}