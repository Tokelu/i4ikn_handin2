using System;
using System.IO;
using System.Text;
using Transportlaget;
using Library;

namespace Application
{
	class file_server
	{
		/// <summary>
		/// The BUFSIZE
		/// </summary>
		private const int BUFSIZE = 1000;
		private const string APP = "FILE_SERVER";

		/// <summary>
		/// Initializes a new instance of the <see cref="file_server"/> class.
		/// </summary>
		private file_server ()
		{
		    Transport transport = new Transport(BUFSIZE);
		    byte[] buffer = new byte[BUFSIZE];
		    int size = 0;
		    while (true)
		    {
		        while ((size = transport.Receive(ref buffer)) == 0)
		        { };


		        if (size != 0)
		        {
		            string filePath = Encoding.UTF8.GetString(buffer, 0, size);

		            string tmp = Path.GetFullPath("File_Server_Home/" + filePath);
		            sendFile(tmp, transport);

		        }
		        transport = new Transport(BUFSIZE);
		    }
		}

        /// <summary>
        /// Sends the file.
        /// </summary>
        /// <param name='fileName'>
        /// File name.
        /// </param>
        /// <param name='fileSize'>
        /// File size.
        /// </param>
        /// <param name='tl'>
        /// Tl.
        /// </param>
        private void sendFile(string filePath, Transport transport)
		{
		    var fileSize = (int)LIB.check_File_Exists(@filePath);
		    var lengthToSend = Encoding.UTF8.GetBytes(fileSize.ToString());
		    transport.Send(lengthToSend, lengthToSend.Length);

		    if (fileSize != 0)
		    {

		        Console.WriteLine("Sending file: " + filePath + " to client");

		        var fileBuf = new byte[BUFSIZE];
		        var bytesRead = 0;

		        using (var fileStream = File.Open(@filePath, FileMode.Open))
		        {

		            while ((bytesRead = ReadChunk(fileStream, ref fileBuf)) != 0)
		            {
		                transport.Send(fileBuf, bytesRead);
		            }

		        }
		        Console.WriteLine("The requested file was sent");

		    }
		    else
		    {
		        Console.WriteLine("The requested file was not found on the server!");
		    }
		}


	    private int ReadChunk(FileStream stream, ref byte[] chunk)
	    {
	        int index = 0;
	        while (index < chunk.Length)
	        {
	            int bytesRead = stream.Read(chunk, index, chunk.Length - index);
	            if (bytesRead == 0)
	            {
	                break;
	            }
	            index += bytesRead;
	        }
	        return index;
	    }


        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name='args'>
        /// The command-line arguments.
        /// </param>
        public static void Main (string[] args)
		{
		    Console.WriteLine(APP);
		    file_server server = new file_server();
        }
	}
}