using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    Vector3 movePosition;
    public int speed = 3;
    Rigidbody rb;
    int playerNum;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        InputMove();

        Debug.Log(rb.velocity);

        DataSend();


    }
    void InputMove()
    {
        float mouse_X = Input.GetAxis("Mouse X");
        float mouse_Y = Input.GetAxis("Mouse Y");

        if ( MyClient.instance != null)
        {
            //Debug.Log(MyClient.client + " " + MyClient.client.Connected);
            movePosition = new Vector3(mouse_X, 0, mouse_Y);
            //Debug.Log(rb.velocity);
            //transform.position += movePosition * Time.deltaTime * speed;

            try
            {
                if (MyClient.instance.playerNum == 0)
                {
                        rb.velocity = movePosition.normalized * speed;
                        //Debug.Log("ȣ��Ʈ��!");
                        //byte[] buf = Encoding.Default.GetBytes("MOVE:" + MyClient.instance.playerNum + ":" + mouse_X + ":" + mouse_Y + ":");
                        //�Է¹� ���
                        //MyClient.client.GetStream().Write(buf, 0, buf.Length);
                        //MyClient.client.GetStream().Flush();
                        //MyClient.instance.Send(buf);
                }
                else
                {
                    //�Է¹� ���
                    byte[] buf = Encoding.Default.GetBytes("MOVE:" + MyClient.instance.playerNum + ":" + mouse_X + ":" + mouse_Y + ":");
                    //MyClient.client.GetStream().Write(buf, 0, buf.Length);
                    //MyClient.client.GetStream().Flush();
                    MyClient.instance.Send(buf);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Exception during network write: " + e.Message);
            }
            //MyClientclient.GetStream().Write(buf, 0, buf.Length);
        }

    }

    public void DataSend()
    {
        //�� ������ ������
        

        //�÷��̾� ������ ������
    }

    public void SetPlayerNum(int num)
    {
        playerNum = num;
    }

    void HostData()
    {

    }
    void ClientData()
    {

    }

    private void OnTriggerEnter(Collider other)
    {

    }
}
