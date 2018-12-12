using System;
using System.IO;
using System.Text;
using Transportlaget;
using Library;

namespace Application
{
	class file_client
	{
		/// <summary>
		/// The BUFSIZE.
		/// </summary>
		private const int BUFSIZE = 1000;
		private const string APP = "FILE_CLIENT";

		/// <summary>
		/// Initializes a new instance of the <see cref="file_client"/> class.
		/// 
		/// file_client metoden opretter en peer-to-peer forbindelse
		/// Sender en forspÃ¸rgsel for en bestemt fil om denne findes pÃ¥ serveren
		/// Modtager filen hvis denne findes eller en besked om at den ikke findes (jvf. protokol beskrivelse)
		/// Lukker alle streams og den modtagede fil
		/// Udskriver en fejl-meddelelse hvis ikke antal argumenter er rigtige
		/// </summary>
		/// <param name='args'>
		/// Filnavn med evtuelle sti.
		/// </param>
	    private file_client(String[] args)
	    {
	        if (args.Length == 1)
	        {
	            Transport transport = new Transport(BUFSIZE);
	            byte[] buffer = new byte[BUFSIZE];
	            string filePath = args[0];
	            Console.WriteLine("Requesting file: " + filePath + " from server");

	            transport.Send(Encoding.UTF8.GetBytes(filePath), filePath.Length);
	            int size = transport.Receive(ref buffer);
	            int fileSize = 0;
	            if (size != 0)
	            {
	                fileSize = int.Parse(Encoding.UTF8.GetString(buffer, 0, size));
	                Console.WriteLine("The requested file is found on the server");
	                if (fileSize > 0)
	                {
	                    ReceiveFile(filePath, fileSize, transport);
	                }
	            }
	            else
	                Console.WriteLine("The file does not exist on the server.");
	        }
	        else
	            Console.WriteLine("The arguments provided does not match : ", args.Length);
	    }

        /// <summary>
        /// Receives the file.
        /// </summary>
        /// <param name='fileName'>
        /// File name.
        /// </param>
        /// <param name='transport'>
        /// Transportlaget
        /// </param>
        private void ReceiveFile (string path, long fileSize, Transport transport)
		{
		    byte[] fileBuffer = new byte[BUFSIZE];
		    string fileName = LIB.extractFileName(path);
		    var readSize = 0;
		    var read = 0;

		    Console.WriteLine("Receiving file..");

		    FileStream newFile = new FileStream(fileName, FileMode.Create, FileAccess.Write);
		    while (read < fileSize && (readSize = transport.Receive(ref fileBuffer)) > 0)
		    {
		        newFile.Write(fileBuffer, 0, readSize);
		        read += readSize;
		    }

		    if (read == fileSize)
		        Console.WriteLine("The requested file was received");
		    else
		        Console.WriteLine("An error occured while downloading file..");

		    newFile.Close();
		}

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name='args'>
        /// First argument: Filname
        /// </param>
        public static void Main (string[] args)
		{
		    Console.WriteLine(APP);
		    file_client client = new file_client(args);
		    Console.Read();
		}
	}
}