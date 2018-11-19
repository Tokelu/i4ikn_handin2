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




        /// <summary>
        /// Send the specified buf and size.
        /// </summary>
        /// <param name='buf'>
        /// Buffer.
        /// </param>
        /// <param name='size'>
        /// Size.
        /// </param>
        public void send (byte[] buf, int size)
		{
	    	// TO DO Your own code
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
		public int receive (ref byte[] buf)
		{
	    	// TO DO Your own code
			return 0;
		}
	}
}
