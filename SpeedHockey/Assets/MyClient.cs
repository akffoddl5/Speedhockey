using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class MyClient : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        Debug.Log("Ŭ���̾�Ʈ�ܼ�â. \n\n\n");

        TcpClient client = new TcpClient();

        client.Connect("172.16.6.5", 5000);

        //�Է¹� ���
        byte[] buf = Encoding.Default.GetBytes("Ŭ���̾�Ʈ : �����մϴ�");
        client.GetStream().Write(buf, 0, buf.Length);

        client.Close();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
