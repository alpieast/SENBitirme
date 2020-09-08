﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

enum paketCozme
{
    BirinciDogrulama,
    İkinciDogrulama,
    Veriler
}

namespace BitirmeProjesiArayuzProjesi
{
    public partial class Analyse_page : Form
    {

        string Gelen = string.Empty;
        string[] ports = SerialPort.GetPortNames();
        // 1
        GraphPane myPane = new GraphPane();
        RollingPointPairList listPointsOne = new RollingPointPairList(40);
        LineItem myCurveOne;
        RollingPointPairList listPointsTwo = new RollingPointPairList(40);
        LineItem myCurveTwo;
        RollingPointPairList listPointsThree = new RollingPointPairList(40);
        LineItem myCurveThree;

        int hataliPaketSayisi = 0;
        int counter = 0;
        static Byte[] cozulen_paket = new Byte[8];
        SerialPort mySerialPort;
        static int gelenveri;
        static char gelen_veri;
        public SerialPort _serialPort;
        public Analyse_page()

        {

            InitializeComponent();
            zedGraphControl1.GraphPane.YAxis.Scale.Max = 1023;
            zedGraphControl1.GraphPane.YAxis.Scale.Min = 0;

            try
            {
                mySerialPort = new SerialPort();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }



        }





        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {

        SerialPort sp = (SerialPort)sender;
            try
            {
                mySerialPort.Read(cozulen_paket, 0, 8);
            }
            catch (Exception ex) {
                
                MessageBox.Show( (string)("Please wait..."));
                return;
                
            }
            paketCozme myVar = paketCozme.BirinciDogrulama;
            for(int i = 0; i < cozulen_paket.Length; i++)
            {
                switch (myVar)
                {
                    case paketCozme.BirinciDogrulama:
                        if (cozulen_paket[i] == 0xAB)
                        {
                            myVar = paketCozme.İkinciDogrulama;
                        }
                        else
                        {
                            myVar = paketCozme.BirinciDogrulama;
                        }
                        break;

                    case paketCozme.İkinciDogrulama:
                        if (cozulen_paket[i] == 0xFD)
                        {
                            myVar = paketCozme.Veriler;
                        }
                        else
                        {
                            myVar = paketCozme.BirinciDogrulama;
                        }
                        break;
                    case paketCozme.Veriler:
                        if(i >= 7)
                        {
                            hataliPaketSayisi++;
                        }
                        else
                        {

                            gelenveri = cozulen_paket[i];
                            gelenveri |= cozulen_paket[i + 1] << 8;
                        }


                        myVar = paketCozme.BirinciDogrulama;
                        break;
                }
            }



        }


        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void timer_arduino_Tick(object sender, EventArgs e)
        {
            labelGuncelleme();
            counter++;
            double x = Convert.ToDouble(counter);
            double y1 = Convert.ToDouble(gelenveri);
            listPointsOne.Add(x, y1);
            myCurveOne = myPane.AddCurve(null, listPointsOne, Color.Blue, SymbolType.Circle);
            zedGraphControl1.Invalidate();
            zedGraphControl1.AxisChange();
            zedGraphControl1.Refresh();
        }

        private void lbl_arduino_raw_data_Click(object sender, EventArgs e)
        {
           
        }
        private void labelGuncelleme()
        {
            lbl_arduino_raw_data.Text =   "X koordinati : " + gelenveri.ToString() ;
        }

        private void Analyse_page_Load(object sender, EventArgs e)
        {
            
                //1
                myPane = zedGraphControl1.GraphPane;

                myPane.XAxis.Title.Text = "Time";
                myPane.YAxis.Title.Text = "Data";
                zedGraphControl1.Invalidate();
                zedGraphControl1.AxisChange();
                zedGraphControl1.Refresh();
            

                /* Adding connected ports */
                foreach (string port in ports)
                {
                    cbComPort.Items.Add(port);
                }
                /* Baudrates are added */
                cbBaud.Items.Add("2400");
                cbBaud.Items.Add("4800");
                cbBaud.Items.Add("9600");
                cbBaud.Items.Add("19200");
                cbBaud.Items.Add("38400");
                cbBaud.Items.Add("57600");
                cbBaud.Items.Add("115200");
                cbBaud.SelectedIndex = 2;

            
        }

        private void btn_connect_Click(object sender, EventArgs e)
        {


            if (mySerialPort.IsOpen == false)
            {
                if (cbComPort.Text == "")
                    return;
                mySerialPort.PortName = cbComPort.Text;
                mySerialPort.BaudRate = Convert.ToInt32(cbBaud.Text);
                try
                {
                    mySerialPort.Parity = Parity.None;
                    mySerialPort.StopBits = StopBits.One;
                    mySerialPort.DataBits = 8;
                    mySerialPort.Handshake = Handshake.None;
                    mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                    mySerialPort.Open();
                }
                catch (Exception er)
                {
                    MessageBox.Show("Error:" + er.Message);
                }
            }
            else
            {
                //rtbEkran.Text = "seriport already open";
            }
        }

        private void Analyse_page_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (mySerialPort.IsOpen == true)
            {
                mySerialPort.Close();
            }
        }

        private void btn_disconnect_Click(object sender, EventArgs e)
        {
            if (mySerialPort.IsOpen == true)
            {
                mySerialPort.Close();
            }
        }
    }
}
