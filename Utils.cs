using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;
using System.Threading.Tasks;
class Utils
{

	public static void DumpObject(object obj)
	{
		Type type = obj.GetType();
		PropertyInfo[] properties = type.GetProperties();

		foreach (PropertyInfo property in properties)
		{
			object value = property.GetValue(obj);
			Console.WriteLine($"{property.Name}: {value}");
		}
	}
     public static byte getMostSignificantByte( int number){
                        byte[] bytes = new byte[4];

                        bytes[0] = (byte)(number >> 24);
                        bytes[1] = (byte)(number >> 16);
                        bytes[2] = (byte)(number >> 8);
                        bytes[3] = (byte)number;
                        return bytes[3];
                    }

	public static ArrayList getMissingPacketSequenceList( ArrayList packetSequenceList, int  maxPacketSequence)
	{
					
					ArrayList missingPacketSequenceList = new ArrayList();
					packetSequenceList.Sort();
					for(int i=1; i<=maxPacketSequence; i++)
					{
						if (packetSequenceList.BinarySearch( i ) < 0)
						{
							missingPacketSequenceList.Add( i );
						}
					
					}
					return missingPacketSequenceList;
				}
	public static StockTickerData getStockTickerDataFromBuffer( byte[] buffer )
	{
		if (buffer == null || buffer.Length < 17)
		{
			return null; // Return null if the buffer is not valid
		}
		StockTickerData tickerData = new StockTickerData
					{
						Symbol = Encoding.ASCII.GetString(buffer, 0, 4),
						BuySellIndicator = Encoding.ASCII.GetString(buffer, 4, 1),
						Quantity = ToInt32BigEndian(buffer, 5),
						Price = ToInt32BigEndian(buffer, 9),
						PacketSequence = ToInt32BigEndian(buffer, 13)
					};
		return tickerData;
	}

    private static int ToInt32BigEndian(byte[] buf, int i)
	{
		return (buf[i]<<24) | (buf[i+1]<<16) | (buf[i+2]<<8) | buf[i+3];
	}
}