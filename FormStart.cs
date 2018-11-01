using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UniBusDataDetector.DataDetector;
using System.Threading;

namespace WindowsFormsApp1
{
    public partial class FormStart : Form
    {

        #region Transmisja_zmienne
        UInt16 My_address = 0x00, Dev_address = 0x00;
        UInt16 OR_size = 0, licz_ref = 0, licz_loop = 0;
        UInt32 TX_byte=0, RX_byte=0, ERR_byte=0, Prog=0;
        Byte[] FrameAskGet = new byte[300];
        #endregion

        #region Ustawienia_zmienne
        private UInt16 baud = 56000;
        private String port = "COM1";
        private UInt16 databits = 8;
        private UInt16 parity = 0;
        private UInt16 StopBits = 1;
        #endregion

        #region Struktura_zmienne
        public DataTable tabela = new DataTable();
        #endregion

        #region Flagi
        private Boolean flaga_err = false;
        private Boolean flaga_ref = false;
        #endregion

        #region Inicjalizacja_formy
        public FormStart()
        {
            InitializeComponent();
        }

        private void FormStart_Load(object sender, EventArgs e)
        {
            tabela.Columns.Add("Indeks");
            tabela.Columns.Add("Opis");
            tabela.Columns.Add("Jednostka");
            tabela.Columns.Add("Atrybut");
            tabela.Columns.Add("Typ");
            tabela.Columns.Add("Wartość");

            tabela.AcceptChanges();
            dataGridView1.DataSource = tabela;
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridView1.RowHeadersWidth = 30;
            dataGridView1.Columns["Indeks"].Width = 50;
            dataGridView1.Columns["Opis"].Width = 220;
            dataGridView1.Columns["Jednostka"].Width = 60;
            dataGridView1.Columns["Atrybut"].Width = 50;
            dataGridView1.Columns["Typ"].Width = 70;
            dataGridView1.Columns["Wartość"].Width = 60;

            object[] ports = System.IO.Ports.SerialPort.GetPortNames();
            comboBoxPorty.Items.AddRange(ports);
            comboBoxPorty.Text = "COM9";
            comboBoxBaud.Items.Add("9600");
            comboBoxBaud.Items.Add("56000");
            comboBoxBaud.Text = "56000";
        }
        #endregion

        #region Funkcje_poboczne
        private string Get_Type(UInt16 atrybut)
        {
            switch (atrybut & 0x0F)
            {
                case 1:
                    return "INT8 [1b]";
                case 2:
                    return "INT16 [2b]";
                case 3:
                    return "INT32 [4b]";
                case 4:
                    return "UINT8 [1b]";
                case 6:
                    return "UINT32 [4b]";
                case 7:
                    return "FLOAT [4b]";
                case 9:
                    return "DOUBLE [4b]";
                case 10:
                    return "BOOL [4b]";
                case 13:
                    return "INT64 [8b]";
                case 14:
                    return "UINT64 [8b]";
                default:
                    return "NIEZNANY";
            }
        }
        #endregion

        #region Zdarzenia_button
        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonZdarzenia_Click(object sender, EventArgs e)
        {
        }

        private void buttonUstawienia_Click(object sender, EventArgs e)
        {
            MainTab.SelectedTab = MainTab.TabPages[3];
        }

        private void buttonStruktura_Click(object sender, EventArgs e)
        {
            MainTab.SelectedTab = MainTab.TabPages[0];
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                serialPort1.BaudRate = baud;
                serialPort1.PortName = port;
                serialPort1.Parity = (System.IO.Ports.Parity)parity;
                serialPort1.DataBits = databits;
                UInt16.TryParse(textBoxDevAddr.Text, out Dev_address);
                UInt16.TryParse(textBoxSoAddr.Text, out My_address);

                try
                {
                    serialPort1.Open();
                    buttonConnect.Visible = false;
                    buttonDisconnect.Visible = true;
                    groupBoxUstawienia.Enabled = false;
                }
                catch
                {
                    MessageBox.Show("Próba otwarcia połączenia COM nie udana.", "UniBus Data Detector", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Czy jesteś pewny że chcesz zakończyć program ?", "UniBus Data Detector", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialog == DialogResult.Yes)
            {
                this.Close();
            } else return;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                Set_TimeRep(UInt16.Parse(textBoxDelRx.Text));
                licz_ref = 4;
                flaga_ref = true;
                MainTab.SelectedTab = MainTab.TabPages[0];
            }
            else
            {
                MessageBox.Show("Brak dostępnego portu COM. Zestaw połączenie i spróbuj ponownie.", "UniBus Data Detector", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Close();
                buttonConnect.Visible = true;
                buttonDisconnect.Visible = false;
                groupBoxUstawienia.Enabled = true;
            }
            catch
            {
                MessageBox.Show("Próba zamknięcia połączenia COM nie udana.", "UniBus Data Detector", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }
        }
        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                Set_TimeRep(UInt16.Parse(textBoxDelRx.Text));
                flaga_ref = true;
                buttonRefresh.Enabled = false;
                MainTab.SelectedTab = MainTab.TabPages[0];
            }
            else
            {
                MessageBox.Show("Brak dostępnego portu COM. Zestaw połączenie i spróbuj ponownie.", "UniBus Data Detector", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }

        }
        #endregion

        #region Obsługa_błedów
        private void Err_task()
        {
            try
            {
                serialPort1.Close();
                serialPort1.Open();
            }
            catch
            {

            };
            buttonRefresh.Enabled = true;
            buttonDane.Enabled = false;
            switch (licz_ref)
            {
                case 1:
                    MessageBox.Show("Niepowodzenie odczytu identyfikatora urządzenia o adresie: " + Dev_address.ToString(), "UniBus Data Detector", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    break;
                case 2:
                    MessageBox.Show("Niepowodzenie odczytu rozmiaru tablicy OR urządzenia o adresie: " + Dev_address.ToString(), "UniBus Data Detector", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    break;
                case 3:
                    MessageBox.Show("Niepowodzenie odczytu tablicy OR urządzenia o adresie: " + Dev_address.ToString(), "UniBus Data Detector", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    break;
                case 4:
                    MessageBox.Show("Niepowodzenie odczytu wartości parametrów tablicy OR urządzenia o adresie: " + Dev_address.ToString(), "UniBus Data Detector", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    break;
                default:
                    MessageBox.Show("Brak odpowiedzi urządzenia o adresie: " + Dev_address.ToString(), "UniBus Data Detector", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    break;
            }
        }
        #endregion

        #region Ładowanie_struktury
        private void Ref_task()
        {

            if (flaga_ref == true && timer1.Enabled == false)
            {
                if (flaga_err == true)
                {
                    flaga_err = false;
                    flaga_ref = false;
                    ERR_byte++;
                    Ref_label();
                    tabela.RejectChanges();
                    Err_task();
                    licz_ref = 0;
                    return;
                }
                byte[] tab_pom = new byte[2] { 0, 0 };
                byte[] tab_frame = new byte[20];
                Prog = licz_ref;
                switch (licz_ref)
                {
                    case 0:
                        buttonDane.Enabled = false;
                        tabela.RejectChanges();
                        licz_ref++;
                        break;
                    case 1:
                        tab_frame = CreateFrame.Create_ComFrame(My_address, Dev_address, 0x14);
                        serialPort1.Write(tab_frame, 0, 11);
                        TX_byte += 11;
                        timer1.Enabled = true;
                        licz_ref++;
                        break;
                    case 2:
                        tab_frame = CreateFrame.Create_ComFrame(My_address, Dev_address, 0x10);
                        serialPort1.Write(tab_frame, 0, 11);
                        TX_byte += 11;
                        timer1.Enabled = true;
                        licz_ref++;
                        break;
                    case 3:
                        tab_pom[0] = (byte)licz_loop;
                        tab_pom[1] = 0;
                        tab_frame = CreateFrame.Create_DataFrame(My_address, Dev_address, 0x11, tab_pom);
                        serialPort1.Write(tab_frame, 0, 13);
                        TX_byte += 13;
                        timer1.Enabled = true;
                        licz_loop++;
                        if (licz_loop == OR_size)
                        {
                            licz_loop = 0;
                            licz_ref++;
                        }
                        break;
                    case 4:
                        tab_pom[0] = (byte)licz_loop;
                        tab_pom[1] = 0;
                        tab_frame = CreateFrame.Create_DataFrame(My_address, Dev_address, 0x12, tab_pom);
                        serialPort1.Write(tab_frame, 0, 13);
                        TX_byte += 13;
                        timer1.Enabled = true;
                        licz_loop++;
                        if (licz_loop == OR_size)
                        {
                            licz_loop = 0;
                            licz_ref++;
                        }
                        break;
                    case 5:
                        buttonRefresh.Enabled = true;
                        buttonDane.Enabled = true;
                        flaga_ref = false;
                        licz_ref = 0;
                        Prog = 0;
                        Ref_label();

                        break;
                    default:
                        break;
                }

            }
        }
        #endregion

        #region Timer_obsługa
        private void timer1_Tick_1(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            flaga_err = true;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            Ref_task();
        }

        private void Set_TimeRep(UInt16 interval)
        {
            if (interval < 100) interval = 100;
            if (interval > 2000) interval = 2000;
            timer1.Interval = interval;
        }
        #endregion

        #region Odbiornik
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            var tab_pom = new byte[2];
            UInt16 len = 0;
            if (serialPort1.BaudRate == 9600)
                Thread.Sleep(75);
            else if (serialPort1.BaudRate == 56000) Thread.Sleep(10);
            serialPort1.Read(FrameAskGet, 0, 3);
            RX_byte += 3;
            if (FrameAskGet[0] == 0x78)
            {
                Array.Copy(FrameAskGet, 1, tab_pom, 0, 2);
                len = BitConverter.ToUInt16(tab_pom, 0);
                try
                {
                    serialPort1.Read(FrameAskGet, 3, len + 3);
                }
                catch
                {
                    return;
                };
                serialPort1.DiscardInBuffer();
                RX_byte += (UInt16)(len + 8);
                if (len > 4 && len < 100)
                {
                    DataFrame Data = new DataFrame(FrameAskGet);
                    if (Data.device_addr == My_address)
                    {
                        if (Data.crc16_xmodem == Crc16.CalcCRC16(FrameAskGet, 1, (UInt16)(len + 2)))
                        {
                            Ref_label();
                            timer1.Enabled = false;
                            try
                            {
                                Switch_task(Data);
                            }
                            catch
                            {

                            }
                        }
                        else
                        {
                            ERR_byte++;
                            Ref_label();
                        }
                    }

                }
            }
        }
        #endregion

        #region Switch_task
        private void Switch_task(DataFrame Data)
        {
            switch (Data.command)
            {
                case 0x20:
                    labelAvabileParam.Text = BitConverter.ToUInt16(Data.DataBufGet, 0).ToString();
                    OR_size = BitConverter.ToUInt16(Data.DataBufGet, 0);
                    break;
                case 0x21:
                    OR_Table taba = new OR_Table(Data.DataBufGet);
                    DataRow WorkRow = tabela.NewRow();
                    WorkRow["Indeks"] = taba.indeks;
                    WorkRow["Opis"] = taba.opis;
                    WorkRow["Jednostka"] = taba.jednostka;
                    if ((taba.atrybut & 16) == 16) WorkRow["Atrybut"] += "R";
                    if ((taba.atrybut & 32) == 32) WorkRow["Atrybut"] += "W";
                    WorkRow["Typ"] = Get_Type(taba.atrybut); 
                    WorkRow["Wartość"] = null;

                    tabela.Rows.Add(WorkRow);
                    break;
                case 0x22:
                    Byte[] tab_pom = new byte[8];
                    Array.Copy(Data.DataBufGet, 0, tab_pom, 0, 2);
                    UInt16 index = BitConverter.ToUInt16(tab_pom, 0);
                    UInt16 size = 0;
                    string typ = (tabela.Rows[index]["Typ"].ToString());
                    switch (typ)
                    {
                        case "INT8 [1b]":
                            size = 1;
                            Array.Copy(Data.DataBufGet, 2, tab_pom, 0, size);
                            Int16 data1 = BitConverter.ToInt16(tab_pom, 0);
                            tabela.Rows[index]["Wartość"] = data1;
                            break;
                        case "INT16 [2b]":
                            size = 2;
                            Array.Copy(Data.DataBufGet, 2, tab_pom, 0, size);
                            Int16 data2 = BitConverter.ToInt16(tab_pom, 0);
                            tabela.Rows[index]["Wartość"] = data2;
                            break;
                        case "INT32 [4b]":
                            size = 4;
                            Array.Copy(Data.DataBufGet, 2, tab_pom, 0, size);
                            Int32 data3 = BitConverter.ToInt32(tab_pom, 0);
                            tabela.Rows[index]["Wartość"] = data3;
                            break;
                        case "UINT8 [1b]":
                            size = 1;
                            Array.Copy(Data.DataBufGet, 2, tab_pom, 0, size);
                            UInt16 data4 = BitConverter.ToUInt16(tab_pom, 0);
                            tabela.Rows[index]["Wartość"] = data4;
                            break;
                        case "UINT32 [4b]":
                            Array.Copy(Data.DataBufGet, 2, tab_pom, 0, size);
                            UInt32 data5 = BitConverter.ToUInt32(tab_pom, 0);
                            tabela.Rows[index]["Wartość"] = data5;
                            size = 4;
                            break;
                        case "FLOAT [4b]":
                            size = 4;
                            Array.Copy(Data.DataBufGet, 2, tab_pom, 0, size);
                            Single data6 = BitConverter.ToSingle(tab_pom, 0);
                            tabela.Rows[index]["Wartość"] = data6;
                            break;
                        case "DOUBLE [4b]":
                            size = 4;
                            Array.Copy(Data.DataBufGet, 2, tab_pom, 0, size);
                            double data7 = BitConverter.ToDouble(tab_pom, 0);
                            tabela.Rows[index]["Wartość"] = data7;
                            break;
                        case "BOOL [4b]":
                            size = 4;
                            Array.Copy(Data.DataBufGet, 2, tab_pom, 0, size);
                            Boolean data8 = BitConverter.ToBoolean(tab_pom, 0);
                            tabela.Rows[index]["Wartość"] = data8;
                            break;
                        case "INT64 [8b]":
                            size = 8;
                            Array.Copy(Data.DataBufGet, 2, tab_pom, 0, size);
                           Int64 data9 = BitConverter.ToInt64(tab_pom, 0);
                            tabela.Rows[index]["Wartość"] = data9;
                            break;
                        case "UINT64 [8b]":
                            size = 8;
                            Array.Copy(Data.DataBufGet, 2, tab_pom, 0, size);
                            UInt64 data10 = BitConverter.ToUInt64(tab_pom, 0);
                            tabela.Rows[index]["Wartość"] = data10;
                            break;
                        default:
                            tabela.Rows[index]["Wartość"] = null;
                            break;
                    }
                    break;
                case 0x23:
                    break;
                case 0x24:
                    richTextBoxIdent.Text = System.Text.Encoding.UTF8.GetString(Data.DataBufGet, 0, Data.DataBufGet.Length);
                    break;
                default:
                    break;
            }

        }
        #endregion

        #region Odświeżenia_kontenerów
        delegate void delegat();
        private void Ref_label()
        {

            if (this.labelTX.InvokeRequired)
            {
                delegat wsk_reflabel = new delegat(Ref_label);
                this.Invoke(wsk_reflabel);
            }
            else
            {
                progressBar1.Maximum = 4;
                progressBar1.Value = (Int16)(Prog);
                labelTX.Text = TX_byte.ToString() + " Byte";
                labelRX.Text = RX_byte.ToString() + " Byte";
                labelERRCRC.Text = ERR_byte.ToString();
            }        }
        #endregion

        #region ustawienia
        private void radioButtonBits5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonBits5.Checked) databits = 5;
        }

        private void radioButtonBits6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonBits6.Checked) databits = 6;
        }

        private void radioButtonBits7_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonBits7.Checked) databits = 7;
        }

        private void radioButtonBits8_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonBits8.Checked) databits = 8;
        }

        private void radioNone_CheckedChanged(object sender, EventArgs e)
        {
            if (radioNone.Checked) parity = 0;
        }

        private void radioOdd_CheckedChanged(object sender, EventArgs e)
        {
            if (radioOdd.Checked) parity = 1;
        }

        private void radioEven_CheckedChanged(object sender, EventArgs e)
        {
            if (radioEven.Checked) parity = 2;
        }

        private void radioMark_CheckedChanged(object sender, EventArgs e)
        {
            if (radioMark.Checked) parity = 3;
        }

        private void radioButtonStop1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonStop1.Checked) StopBits = 1;
        }

        private void textBoxSoAddr_Leave(object sender, EventArgs e)
        {
            try
            {
                UInt16.Parse(textBoxSoAddr.Text);
            }
            catch
            {
                textBoxSoAddr.Text = "00";
            };
        }

        private void textBoxDevAddr_Leave(object sender, EventArgs e)
        {
            try
            {
                UInt16.Parse(textBoxDevAddr.Text);
            }
            catch
            {
                textBoxDevAddr.Text = "00";
            };
        }

        private void buttonZapisz_Click(object sender, EventArgs e)
        {
            
        }

        private void Indeks_box_Leave(object sender, EventArgs e)
        {

        }

        private void War_box_Leave(object sender, EventArgs e)
        {

        }

        private void radioButtonStop2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonStop2.Checked) StopBits = 2;
        }

        private void comboBoxPorty_SelectedValueChanged(object sender, EventArgs e)
        {
            port = comboBoxPorty.Text;
        }

        private void comboBoxBaud_SelectedValueChanged(object sender, EventArgs e)
        {
            baud = Convert.ToUInt16(comboBoxBaud.Text);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                textBoxDevAddr.Enabled = false;
                textBoxDevAddr.Text = "00";
            }
            else
            {
                textBoxDevAddr.Enabled = true;
            }
        }

        private void textBoxDelRx_Leave(object sender, EventArgs e)
        {
            try
            {
                if (UInt16.Parse(textBoxDelRx.Text) < 200) textBoxDelRx.Text = "200";
                if (UInt16.Parse(textBoxDelRx.Text) > 2000) textBoxDelRx.Text = "2000";
            }
            catch
            {
                textBoxDelRx.Text = "200";
            };

        }

        private void textBoxDelTx_Leave(object sender, EventArgs e)
        {
            try
            {
                if (UInt16.Parse(textBoxDelTx.Text) < 100) textBoxDelTx.Text = "200";
                if (UInt16.Parse(textBoxDelTx.Text) > 2000) textBoxDelTx.Text = "2000";
            }
            catch
            {
                textBoxDelTx.Text = "200";
            };
        }
        #endregion
    }
}
