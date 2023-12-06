using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Text;
using System;

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
            }
            else
            {
                for (int i = 0; i < room.Count; i++)
                {
                    if (room[i].sockets.Count < room[i].MaxPlayerNum)
                    {
                        room[i].sockets.Add(client);

                        room[i].sockets[room[i].sockets.Count - 1].Send(Encoding.Default.GetBytes("NUM:" + (room[i].sockets.Count - 1).ToString()));

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

        try
        {
            int bytesRead = obj.WorkingSocket.EndReceive(ar);

            if (socketList[0] == obj.WorkingSocket)
            {
                Debug.Log("�� ����");
            }
            else
            {
                Debug.Log("�� �ٸ�");
            }

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
                        if (commands[0] == "Move")
                        {

                            float moveX = float.Parse(commands[1]);
                            float moveY = float.Parse(commands[2]);


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
                            for(int i=0; i< room[changeRoomIndex].sockets.Count; i++)
                            {
                                room[changeRoomIndex].sockets[i].Send(Encoding.Default.GetBytes("NUM:" + i.ToString()));
                            }
                            
                        }
                    }
                }
                Debug.Log("Received: " + receivedString);
            }

            // ���� ������ ���� ���
            obj.WorkingSocket.BeginReceive(obj.Buffer, 0, obj.Buffer.Length, 0, DataReceived, obj);
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
        socketList[0].Send(msg);// ȣ��Ʈ�˷��ֱ�
    }
    int GetMyRoomNum(Socket mySocket)
    {
        for (int i = 0; i < room.Count; i++)
        {
            for (int j = 0; j < room[i].sockets.Count; j++)
            {

            }
        }
        int a = 0;
        return a;
    }
}


