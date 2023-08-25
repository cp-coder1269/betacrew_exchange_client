
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using Newtonsoft.Json;
using System.Reflection;

public class StockTickerData
{
	public string Symbol { get; set; }
	public string BuySellIndicator { get; set; }
	public int Quantity { get; set; }
	public int Price { get; set; }
	public int PacketSequence { get; set; }
}

class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine("Client Started!!");
		string serverHost = Constants.HOST; // Replace with the actual server IP address
		int serverPort = Constants.PORT;
		try
		{
			List<StockTickerData> tickerDataList = new List<StockTickerData>();
			ArrayList packetSequenceList = new ArrayList();
			int maxPacketSequence = -1;
			ArrayList missingPacketSequenceList = new ArrayList();

			// Receive and process the response packets
			byte[] buffer = new byte[Constants.PACKET_SIZE]; // Packet size
				int bytesRead;

			TcpClient client = null;
			NetworkStream stream = null;
			try
			{
				client = new TcpClient(serverHost, serverPort);
				stream = client.GetStream();
							
				// Create a request payload for "Stream All Packets"
				byte[] requestPayload = new byte[] {0x01};

				// Send the request payload to the server
				stream.Write(requestPayload, 0, requestPayload.Length);

                Socket socket = client.Client;
                if(!socket.Poll(1000, SelectMode.SelectRead)){
                    stream.Write(requestPayload, 0, requestPayload.Length);
                }
                
                stream.Flush();

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
				{
					StockTickerData tickerData = Utils.getStockTickerDataFromBuffer( buffer );
					tickerDataList.Add(tickerData);
					
					packetSequenceList.Add(tickerData.PacketSequence);
					if( maxPacketSequence < tickerData.PacketSequence)
					{
						maxPacketSequence = tickerData.PacketSequence;
					}
				}
				
				missingPacketSequenceList = Utils.getMissingPacketSequenceList(packetSequenceList, maxPacketSequence);//make correct call
			 	stream.Close();
    			client.Close();
                Console.WriteLine("First Request Successful with "+packetSequenceList.Count +" packets out of "+maxPacketSequence+".");
			}
			catch (Exception ex)
			{
				// Handle exceptions here
				Console.WriteLine("An error occurred: " + ex.Message);
			}
			try
			{
				client = new TcpClient(serverHost, serverPort);
				stream = client.GetStream();

				//if missing->start connection->req each elt 1 by 1 -> close connection
				foreach (int missingPacket in missingPacketSequenceList)
				{
					Console.WriteLine("missing packet number: "+missingPacket);

					byte[] requestPayload = new byte[2];
					requestPayload[0] = 0x02;
					requestPayload[1] = Utils.getMostSignificantByte(missingPacket);

					// Create a request payload for "Resend Packet".
					stream.Write(requestPayload, 0, requestPayload.Length);

					if ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
					{
						StockTickerData tickerData = Utils.getStockTickerDataFromBuffer( buffer );
						tickerDataList.Add(tickerData);
						//Utils.DumpObject(tickerData);
					}

				}


				// Ensure sequential sequences
				tickerDataList.Sort((x, y) => x.PacketSequence.CompareTo(y.PacketSequence));

				// Generate JSON output
				string jsonOutput = JsonConvert.SerializeObject(tickerDataList, Formatting.Indented);

				string timestamp = DateTime.Now.ToString("HHmmss");
				string fileName = $"output_{timestamp}.json";

				File.WriteAllText(fileName, jsonOutput);
                Console.WriteLine("Second Requests successful.");
				Console.WriteLine("JSON output generated successfully with "+tickerDataList.Count+" packets.");
				stream.Close();
    			client.Close();
			}
			catch (Exception ex)
			{
				// Handle exceptions here
				Console.WriteLine("An error occurred: " + ex.Message);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("An error occurred: " + ex.Message);
		}
	}
}

