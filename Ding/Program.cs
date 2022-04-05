using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
//using System.Diagnostics;

namespace Ding
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ding!");
            if (args.Length <= 0)
            {
                noArgsMode();
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    //Console.WriteLine(args);

                    if (args[i] == "-s")
                    {
                        Console.WriteLine("Running as a server.");
                        startServer();
                    }
                    else if (args[i] == "-c")
                    {
                        Console.WriteLine("Running as a client.");
                        ClientConnection(args);
                    }
                }
            }
            Console.WriteLine("Exiting...");
            //Console.ReadLine();
        } //End Main

        public static void ClientConnection(string[] args)
        {
            String serverIP = "";
            for (int i = 0; i < args.Length; i++) //Find -a in args
            {
                if (args[i] == "-a")
                {
                    try
                    {
                        i++; //whatever comes after the -a, which [should] be a properly formed IP address...
                        IPAddress addr = IPAddress.Parse(args[i]);
                        serverIP = Convert.ToString(addr);
                    }
                    catch
                    {
                        Console.WriteLine("Missing or Invalid IP address."); //because this is bound to happen.
                        break; //Exit the for loop.
                    }
                    Console.WriteLine("Attempting to connect to: {0}...", serverIP);
                    Connect(serverIP, "Ding! Ding! Ding!", "Default");
                }
                else
                {

                }
            }
            Console.WriteLine("Disconnecting...");

        }

        public static void startServer()
        {
            TcpListener server = null;
            try
            {
                bool initialMessageSent = false;
                Int32 port = 7777;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1"); // this is something maybe
                server = new TcpListener(localAddr, port);
                Byte[] bytes = new Byte[256];
                string data = null;

                server.Start();
                Console.WriteLine("Initializing...");
                while (true)
                {
                    //Console.Write("Waiting for a connection...");
                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    if (!initialMessageSent)
                    {
                        Console.WriteLine("listening...");
                        initialMessageSent = true;
                    }
                    TcpClient client = server.AcceptTcpClient();
                    //Console.WriteLine("Connected");

                    data = null;
                    NetworkStream stream = client.GetStream();
                    int i;

                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("{0}", data);

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                        stream.Write(msg, 0, msg.Length);
                        //Console.WriteLine("Sent: {0}", data); //
                    }
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                server.Stop();
            }
            Console.WriteLine("\nHit enter to continue...");
            Console.ReadLine();
        }

        static bool Connect(String server, String message, String userName)
        {
            try
            {
                message = Convert.ToString(userName + ":" + message); //causes an error.
                Int32 port = 7777;
                TcpClient client = new TcpClient(server, port);
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                NetworkStream stream = client.GetStream();

                stream.Write(data, 0, data.Length);
                Console.Write(" Sending...");

                data = new Byte[256];
                String responceData = String.Empty;
                Int32 bytes = stream.Read(data, 0, data.Length);
                responceData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine(" Recived by server.");

                stream.Close();
                client.Close();
                return true;
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
                return false;
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                Console.WriteLine("Connection Refused by specified IP address.");
                return false;
            }
        }

        /*static void startServerInNewWindow()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "Ding.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = false,
                Arguments = "-s -a 127.0.0.1",

            };

            var process = new Process { StartInfo = startInfo };

            process.Start();
            process.StandardInput.WriteLine("Hello World.");
            process.StandardOutput.ReadLine();
            process.StandardInput.WriteLine("exit");

            process.WaitForExit();
        }*/

        static void noArgsMode()
        {
            string pickedMode = "";
            string userInput = "";
            string userName = "";
            do
            {
                Console.WriteLine("-c to run as a client \n-s to run as a server \n-exit to close the program.");
                Console.Write("Ding: ");
                pickedMode = Console.ReadLine();

                if (pickedMode == "-c")
                {
                    Console.Write("server IP address: ");

                    ConnectionInfo sessionConnectionInfo = new ConnectionInfo();

                    sessionConnectionInfo.ServerIPAddress = Console.ReadLine();

                    if (sessionConnectionInfo.IsIPAddressValid())
                    {
                        Console.WriteLine("Username:");
                        sessionConnectionInfo.Username = Console.ReadLine();

                        do
                        {

                            Console.Write("Message: ");
                            userInput = Console.ReadLine();
                            //something to make sure that user input isn't mallicious.
                            if (userInput != "" && userInput != "-exit")
                            {
                                //Connect(serverIP, userInput);
                                Stopwatch sw = new Stopwatch();
                                sw.Start();
                                while (!Connect(sessionConnectionInfo.ServerIPAddress, userInput, sessionConnectionInfo.Username))
                                {
                                    if (sw.ElapsedMilliseconds > 3000)
                                    {
                                        Console.WriteLine("There was an error sending the message.");
                                        break;
                                    }
                                }
                            }
                        } while (userInput != "-exit");
                    } // end if valid IP
                } // end picked mode -c
                if (pickedMode == "-s")
                {
                    startServer();
                }

            } while (pickedMode != "-exit");
        } // end no argsMode

    } // end public class program

    public class ConnectionInfo
    {
        Regex rgxIpv4 = new Regex(@"\A(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\z$"); //IPV4 address
        //Regex rgxUsername = new Regex();

        private string serverIPAddress = null;
        private string username = null;

        public string ServerIPAddress
        {
            get
            {
                return serverIPAddress;
            }
            set
            {
                if (rgxIpv4.IsMatch(value))
                {
                    try
                    {
                        IPAddress addr = IPAddress.Parse(value);
                        value = Convert.ToString(addr);
                        serverIPAddress = value;
                    }
                    catch
                    {
                        throw new InvalidDataException($"{nameof(value)} The IP address couldn't be assigned.");
                    }
                }
                else
                {
                    Console.WriteLine("Missing or Invalid IP address.");
                }
            }
        }

        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
            }
        }

        public bool IsIPAddressValid()
        {
            if (serverIPAddress != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsUsernameValid()
        {
            if (username != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    } // public class ConnectionInfo

} // end namespace Ding
