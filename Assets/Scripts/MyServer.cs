using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Text;
using System;
using System.Globalization;

public class Room
{
    public bool isStart = false;
    public int MaxPlayerNum = 4;
    public List<Socket> sockets = new List<Socket>();
}

public class MyServer
{
    public static MyServer instance = new MyServer();

    Socket mainSock;
    //List<Socket> connectedClients = new List<Socket>();
    int m_port = 11000;
    public List<Socket> socketList = new List<Socket>();
    public List<Room> room = new List<Room>();
    //int roomCount = 0;

    public void Start()
    {
        
        try
        {
            mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, m_port);
            mainSock.Bind(serverEP);
            mainSock.Listen(50);
            mainSock.BeginAccept(AcceptCallback, null);
        }
        catch (Exception e)
        {
        }
    }


    public void Close()
    {
        if (mainSock != null)
        {
            mainSock.Close();
            mainSock.Dispose();
        }

        //foreach (Socket socket in connectedClients)
        //{
        //    socket.Close();
        //    socket.Dispose();
        //}
        //connectedClients.Clear();

        foreach (Socket socket in socketList)
        {
            socket.Close();
            socket.Dispose();
        }
        socketList.Clear();

        //mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
    }

    public class AsyncObject
    {
        public byte[] Buffer;
        public Socket WorkingSocket;
        public readonly int BufferSize;
        public AsyncObject(int bufferSize)
        {
            BufferSize = bufferSize;
            Buffer = new byte[(long)BufferSize];
        }

        public void ClearBuffer()
        {
            Array.Clear(Buffer, 0, BufferSize);
        }
    }

    void AcceptCallback(IAsyncResult ar)
    {
        try
        {
            AsyncObject obj = new AsyncObject(300);

            Socket client = mainSock.EndAccept(ar);
            obj.WorkingSocket = client;
            socketList.Add(client);

            //���� �濡 ������ ������� ��ġ
            if (room.Count <= 0)
            {
                room.Add(new Room());
                room[0].sockets.Add(client);

                Send("NUM:0", 0, 0);
                Send("TOTAL:1", 0);
            }
            else
            {
                for (int i = 0; i < room.Count; i++)
                {
                    if (room[i].sockets.Count < room[i].MaxPlayerNum)
                    {
                        room[i].sockets.Add(client);

                        Send("NUM:" + (room[i].sockets.Count - 1).ToString(), i, room[i].sockets.Count - 1);
                        //room[i].sockets[room[i].sockets.Count - 1].Send(Encoding.Default.GetBytes("NUM:" + (room[i].sockets.Count - 1).ToString()));
                        Send("TOTAL:" + room[i].sockets.Count, i);

                        //���� ���� ��ư Ȱ��ȭ ����
                        if (room[i].sockets.Count % 2 == 0)
                        {
                            Send("START_POSSIBILITY:1", i, 0);
                        }
                        else
                        {
                            Send("START_POSSIBILITY:0", i, 0);
                        }
                        

                        break;
                    }
                }
            }


            // ���� ������ ���� ���
            client.BeginReceive(obj.Buffer, 0, obj.Buffer.Length, 0, DataReceived, obj);
            mainSock.BeginAccept(AcceptCallback, null);

            Debug.Log("����");

            //AsyncObject obj = new AsyncObject(1920 * 1080 * 3);
            //obj.WorkingSocket = client;
            //connectedClients.Add(client);
            //client.BeginReceive(obj.Buffer, 0, 1920 * 1080 * 3, 0, DataReceived, obj);
            //string stringData = Encoding.Default.GetString(obj.Buffer);
            //Debug.Log(stringData);
            //mainSock.BeginAccept(AcceptCallback, null);
        }
        catch (Exception e)
        { Debug.LogError("Error in accept callback: " + e.Message); }
    }

    void DataReceived(IAsyncResult ar)
    {
        AsyncObject obj = (AsyncObject)ar.AsyncState;

        Debug.Log("������ ����");

        try
        {
            int bytesRead = obj.WorkingSocket.EndReceive(ar);
            if (bytesRead > 0)
            {
                byte[] receivedData = new byte[bytesRead];
                Array.Copy(obj.Buffer, 0, receivedData, 0, bytesRead);

                // ���⼭ receivedData�� Ȱ���Ͽ� �ʿ��� �۾� ����
                // ����: ���ڿ��� ��ȯ�Ͽ� ���
                string receivedString = Encoding.Default.GetString(receivedData);
                if (receivedString != "")
                {
                    string[] commands = receivedString.Split(":");
                    if (commands.Length > 0)
                    {
                        if (commands[0] == "MOVE")
                        {
                            Debug.Log("���� �Լ� Ȯ��1:" + GetMyRoomNum(obj.WorkingSocket)/* + "���� �Լ� Ȯ��2: "+ GetMyHostSocket(obj.WorkingSocket)*/);
                            GetMyHostSocket(obj.WorkingSocket).Send(receivedData);

                            //float moveX = float.Parse(commands[2]);
                            //float moveY = float.Parse(commands[3]);
                            Debug.Log(receivedData);

                        }
                        else if (commands[0] == "USER_DISCONNECTED")
                        {
                            //���ϸ���Ʈ���� ���������� ���� ����
                            socketList.Remove(obj.WorkingSocket);

                            //�븮��Ʈ���� ���������� ���� ����
                            int changeRoomIndex = 0;
                            for (int i = 0; i < room.Count; i++)
                            {
                                if (room[i].sockets.Remove(obj.WorkingSocket))
                                {
                                    changeRoomIndex = i;
                                    break;
                                }
                            }

                            //�ش� ���� ��������
                            obj.WorkingSocket.Close();

                            //������ Ŭ���̾�Ʈ�� �ִ� �뿡 �ڽ��� ��ȣ �ٽ� �ο�
                            for (int i = 0; i < room[changeRoomIndex].sockets.Count; i++)
                            {
                                Send("NUM:" + i.ToString(), changeRoomIndex, i);
                                //room[changeRoomIndex].sockets[i].Send(Encoding.Default.GetBytes("NUM:" + i.ToString()));
                                Send("TOTAL:" + room[changeRoomIndex].sockets.Count, changeRoomIndex);
                            }

                            //���� ���� ��ư Ȱ��ȭ ����
                            if (room[changeRoomIndex].sockets.Count % 2 == 0)
                            {
                                Send("START_POSSIBILITY", changeRoomIndex, 0);
                            }
                            else
                            {
                                Send("START_POSSIBILITY:0", changeRoomIndex, 0);
                            }
                        }

                        //for(int i=1; i < room[GetMyRoomNum(obj.WorkingSocket)].sockets.Count; i++)
                        //{
                        //    room[GetMyRoomNum(obj.WorkingSocket)].sockets[i]].send()

                                
                        //}
                        
                    }
                }
             //   Debug.Log("Received: " + receivedString);
            }

            // ���� ������ ���� ���
            obj.WorkingSocket.BeginReceive(obj.Buffer, 0, obj.Buffer.Length, 0, DataReceived, obj);

            //���� �� ���� ���
            Debug.Log("==========================");
            for(int i = 0;i<room.Count;i++)
            {
                Debug.Log(i + "�� �� : " + room[i].sockets.Count + "��, isStart : " + room[i].isStart);
            }
            Debug.Log("==========================");
        }
        catch (Exception e)
        {
            Debug.LogError("Error in DataReceived: " + e.Message);
        }
    }

    public void Send(byte[] msg)
    {
        for (int i = 0; i < socketList.Count; i++)
        {
            socketList[i].Send(msg);
        }
    }

    public void Send(string msg)
    {
        for (int i = 0; i < socketList.Count; i++)
        {
            socketList[i].Send(Encoding.Default.GetBytes(msg));
        }
    }

    public void Send(string msg, int roomNum)
    {
        for (int i = 0; i < room[roomNum].sockets.Count; i++)
        {
            room[roomNum].sockets[i].Send(Encoding.Default.GetBytes(msg));
        }
    }

    public void Send(string msg, int roomNum, int socketNum)
    {
        room[roomNum].sockets[socketNum].Send(Encoding.Default.GetBytes(msg));
    }

    int GetMyRoomNum(Socket mySocket)
    {
        for (int i = 0; i < room.Count; i++)
        {
            for (int j = 0; j < room[i].sockets.Count; j++)
            {
                if(room[i].sockets[j] == mySocket)
                {
                    return i;
                }
                
            }
        }
        return -1;
    }
    Socket GetMyHostSocket(Socket mySocket)
    {
      if(GetMyRoomNum(mySocket) != -1)
        return room[GetMyRoomNum(mySocket)].sockets[0];
      else
        return null;
       
    }
    float giveToClientData()
    {
        
        return -1;
    }
}


