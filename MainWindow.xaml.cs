using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Globalization;
using System.Diagnostics;

namespace WpfApp_reska6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool checkConnetionTohostSendToSlack = false;

        private TcpClient tcpclnt = null;
        private IPAddress addr = null;
        private IPEndPoint localEndPoint = null;
        private Stream stm = null;
        private string Fare = "";
        string responseData = "";
        private bool isRunning = false;
        private CheckBox checkBox;
        bool isCard = true;
        bool CbFlags = false;
        bool isConnect = false;
        public MainWindow()
        {
            InitializeComponent();

        }

        private async Task StartClientAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        string serverIP = txtHost.Text;
                        int serverPort = Int16.Parse(txtport.Text);
                        addr = IPAddress.Parse(serverIP);
                        localEndPoint = new IPEndPoint(addr, serverPort);

                    });

                    while (isCard)
                    {
                        try
                        {

                            responseData = await SendToHostAsync("02");
                            generateLog("RESPONSE DATA " + responseData);

                            await this.Dispatcher.InvokeAsync(() =>
                            {
                                txtdataReceived.Text = responseData;
                            });

                            if (responseData.Contains("0200"))
                            {
                                splitData();
                                responseData = "";
                                Console.WriteLine("== check card break process ==");
                                // isCard = false;
                                break;
                            }
                            else if (responseData.Contains("0211"))
                            {
                                Debug.WriteLine("tap the card");
                                await this.Dispatcher.InvokeAsync(() =>
                                {
                                    lblStatus.Text = "Tap the Card";
                                    txtdataReceived.Text = string.Empty;
                                    txtBank.Text = string.Empty;
                                    txtSaldo.Text = string.Empty;
                                    txtNokartu.Text = string.Empty;
                                    txtTime.Text = string.Empty;
                                    txtdataReceivedDeduct.Text = string.Empty;
                                    txtNokartuDeduct.Text = string.Empty;
                                    txtSaldoDeduct.Text = string.Empty;
                                    txtBankDeduct.Text = string.Empty;
                                    txtTimeDeduct.Text = string.Empty;
                                    txtTranscode.Text = string.Empty;
                                });

                            }
                            else if (responseData.Contains("0210"))
                            {
                                await this.Dispatcher.InvokeAsync(() =>
                                {
                                    lblStatus.Text = "Please,Take the Card";
                                    txtdataReceived.Text = string.Empty;
                                    txtBank.Text = string.Empty;
                                    txtSaldo.Text = string.Empty;
                                    txtNokartu.Text = string.Empty;
                                    txtTime.Text = string.Empty;
                                    txtdataReceivedDeduct.Text = string.Empty;
                                    txtNokartuDeduct.Text = string.Empty;
                                    txtSaldoDeduct.Text = string.Empty;
                                    txtBankDeduct.Text = string.Empty;
                                    txtTimeDeduct.Text = string.Empty;
                                    txtTranscode.Text = string.Empty;
                                });

                                Debug.WriteLine("take the card");
                            }

                            else
                            {
                                await this.Dispatcher.InvokeAsync(() =>
                                {
                                    txtdataReceived.Text = string.Empty;
                                    txtBank.Text = string.Empty;
                                    txtSaldo.Text = string.Empty;
                                    txtNokartu.Text = string.Empty;
                                    txtTime.Text = string.Empty;
                                    txtdataReceivedDeduct.Text = string.Empty;
                                    txtNokartuDeduct.Text = string.Empty;
                                    txtSaldoDeduct.Text = string.Empty;
                                    txtBankDeduct.Text = string.Empty;
                                    txtTimeDeduct.Text = string.Empty;
                                    txtTranscode.Text = string.Empty;
                                });
                                await Task.Delay(3000);

                                generateLog("loop_up");
                                //await StartClientAsync();
                                // isCardPresent = false;
                            }
                        }
                        catch (Exception e)
                        {
                            generateLog("ERROR Unexpected exception WHILE: " + e.ToString());
                            if (checkConnetionTohostSendToSlack == false)
                            {
                                checkConnetionTohostSendToSlack = true;
                            }
                            tcpclnt = null;
                            addr = null;
                            localEndPoint = null;
                            stm = null;
                            //await Task.Delay(2000);
                            //await StartClientAsync();
                        }

                        await Task.Delay(2000);
                    }
                    await Application.Current.Dispatcher.Invoke(async () =>
                    {
                        if (Cb1.IsChecked == true)
                        {
                            await waiting();
                            Debug.WriteLine("card loop active");
                            isCard = true;

                        }
                        else
                        {
                            Debug.WriteLine("not loop");
                            //await StartClientAsync();
                        }
                    });
                    //await waiting();
                }
                catch (Exception e)
                {
                    generateLog("ERROR Unexpected exception: " + e.ToString());
                    if (checkConnetionTohostSendToSlack == false)
                    {
                        checkConnetionTohostSendToSlack = true;
                    }
                    isCard = false;
                }
            });
        }

        private async Task waiting()
        {
            await Task.Delay(3000);
            isCard = true;
            await StartClientAsync();
        }

        private async Task<string> SendToHostAsync(string data)
        {
            try
            {
                tcpclnt = new TcpClient();
                tcpclnt.Connect(localEndPoint);


                stm = tcpclnt.GetStream();

                ASCIIEncoding asen = new ASCIIEncoding();
                byte[] ba = asen.GetBytes(data);
                generateLog("proses Kirim Data => " + data);

                await stm.WriteAsync(ba, 0, ba.Length);

                var datax = new byte[2000];

                Int32 bytes = await stm.ReadAsync(datax, 0, datax.Length);
                return System.Text.Encoding.ASCII.GetString(datax, 0, bytes);
                //return responseData;
            }
            catch (Exception e)
            {
                Console.WriteLine("Response SendTohost => ERROR Unexpected exception: " + e.ToString());
                if (checkConnetionTohostSendToSlack == false)
                {
                    checkConnetionTohostSendToSlack = true;
                }
                isCard = true;
                //  await StartClientAsync();
                return "gagal";
            }
        }
        private void generateLog(string data)
        {
            //Thread.Sleep(200);
            string theDate = DateTime.Today.Date.ToString("dd-MM-yyyy");
            string path = @"C:\Datane\Nutech\RESKA\new\AppRFIDListener\AppRFIDListener\Log" + "LOG-" + theDate + ".txt";

            try
            {
                if (!File.Exists(path))
                {
                    string createText = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + " Interface : " + data + Environment.NewLine;
                    Console.WriteLine(createText);

                    using (StreamWriter sr = File.AppendText(path))
                    {
                        sr.WriteLine(createText.ToString());
                        sr.Close();
                    }
                }
                else
                {
                    string appendText = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") + " Interface : " + data + Environment.NewLine;
                    Console.WriteLine(appendText);

                    using (StreamWriter sr = File.AppendText(path))
                    {
                        sr.WriteLine(appendText.ToString());
                        sr.Close();
                    }
                }
            }
            catch (Exception e)
            {
                generateLog(data);
            }
        }
        private void splitData()
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                string input = txtdataReceived.Text;

                int StrCount = 0;
                foreach (char c in input)
                {
                    if (char.IsLetter(c))
                    {
                        StrCount++;
                        if (StrCount < 6)
                        {
                            string str = txtdataReceived.Text;
                            string[] strSplit = str.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            string strOne = strSplit[0];
                            string strBank = strOne.Remove(0, 4);
                            string strTwo = strSplit[1];
                            string NumbCard = strTwo.Substring(strTwo.Length - 16);
                            string Saldo = strTwo.Substring(0, 12);
                            string Saldomodif = Saldo.Remove(0, 8);
                            int SaldoInt = int.Parse(Saldomodif);
                            string SaldoRp = SaldoInt.ToString("C0", new CultureInfo("id-ID"));
                            string strWaktu = strSplit[2];
                            string dateNow = strWaktu;
                            string modifDate = dateNow.Substring(0, 4) + "/" +
                                dateNow.Substring(4, 2) + "/" +
                                dateNow.Substring(6, 2) + " " +
                                dateNow.Substring(8, 2) + ":" +
                                dateNow.Substring(10, 2) + ":" +
                                dateNow.Substring(12, 2);

                            Console.WriteLine("Issuer : " + strBank);
                            Console.WriteLine("Data : " + SaldoRp);
                            Console.WriteLine("Numb Card : " + NumbCard);
                            Console.WriteLine("Date time : " + modifDate);

                            txtBank.Text = strBank;
                            txtSaldo.Text = SaldoRp;
                            txtNokartu.Text = NumbCard;
                            txtTime.Text = modifDate;
                            lblStatus.Text = "CARD CHECK,SUCCESS";
                            await Task.Delay(3000);
                            //lblStatus.Text = " ";
                            await this.Dispatcher.InvokeAsync(() =>
                            {
                                lblStatus.Text = string.Empty;
                                txtdataReceived.Text = string.Empty;
                                txtBank.Text = string.Empty;
                                txtSaldo.Text = string.Empty;
                                txtNokartu.Text = string.Empty;
                                txtTime.Text = string.Empty;
                                txtdataReceivedDeduct.Text = string.Empty;
                                txtNokartuDeduct.Text = string.Empty;
                                txtSaldoDeduct.Text = string.Empty;
                                txtBankDeduct.Text = string.Empty;
                                txtTimeDeduct.Text = string.Empty;
                                txtTranscode.Text = string.Empty;
                            });
                            break;
                        }
                    }
                }
            });
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e) // Connect
        {
            try
            {
                //if (!isConnect)
                //{
                    btnConnect.Background = Brushes.Green;
                    //btnConnect.IsEnabled = false;
                    //btnDisconnect.IsEnabled = true;

                    //MessageBox.Show("Connected to the server.");
                await StartClientAsync();

               // isConnect = true;
                //  }
            }
            catch (Exception ex)
            {

            }
            
        }

        private async void btnDisconet(object sender, RoutedEventArgs e)
        {
            
            //try
            //{
            //    if (tcpclnt != null)
            //    {
            //        tcpclnt.Close();
            //        tcpclnt = null;
            //        isCard = false;
            //        Debug.WriteLine("disconent");
            //        btnConnect.Background = Brushes.WhiteSmoke;
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
            //isConnect = false;
            //btnConnect.IsEnabled = true;
            //btnDisconnect.IsEnabled = false;

            //MessageBox.Show("Disconnected from the server.");
            //btnConnect.Content = "Coctedppp";
        }

        private async void btnDeduct_Click(object sender, RoutedEventArgs e)
        {
            Fare = txtFare.Text;
            responseData = await SendToHostAsync("01" + Fare);
            SplitDataDeduct();
        }

        private async void SplitDataDeduct()
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    txtdataReceivedDeduct.Text = responseData.ToString();
                });
                //txtdataReceivedDeduct.Text = responseData.ToString();

                string input = responseData;

                int StrCount = 0;
                foreach (char c in input)
                {
                    if (char.IsLetter(c))
                    {
                        StrCount++;
                        if (StrCount < 6)
                        {
                            string str = responseData;
                            string[] strSplit = str.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                            string strOne = strSplit[0];
                            string strBank = strOne.Remove(0, 4);
                            string strTwo = strSplit[1];
                            string NumbCard = strTwo.Substring(strTwo.Length - 16);
                            string Saldo = strTwo.Substring(0, 12);
                            string Saldomodif = Saldo.Remove(0, 8);
                            int SaldoInt = int.Parse(Saldomodif);
                            string SaldoRp = SaldoInt.ToString("C0", new CultureInfo("id-ID"));
                            string strWaktu = strSplit[2].Remove(16);
                            string dateNow = strWaktu;
                            string modifDate = dateNow.Substring(0, 4) + "/" +
                                dateNow.Substring(4, 2) + "/" +
                                dateNow.Substring(6, 2) + " " +
                                dateNow.Substring(8, 2) + ":" +
                                dateNow.Substring(10, 2) + ":" +
                                dateNow.Substring(12, 2);

                            string transcode = strSplit[2].Remove(0, 14);

                            Console.WriteLine("----deduct------");
                            Console.WriteLine("Issuer : " + strBank);
                            Console.WriteLine("saldo : " + SaldoRp);
                            Console.WriteLine("No kartu : " + NumbCard);
                            Console.WriteLine("Date time : " + modifDate);

                            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {

                                txtBankDeduct.Text = strBank;
                                txtSaldoDeduct.Text = SaldoRp;
                                txtNokartuDeduct.Text = NumbCard;
                                txtTimeDeduct.Text = modifDate;
                                txtTranscode.Text = transcode;


                                lblStatus.Text = "CARD DEDUCT, SUCCESS";
                                // Task.Delay(2000);
                                //lblStatus.Text = " ";

                            }));
                            await Task.Delay(3000);
                            //await StartClientAsync();

                            break;
                        }
                        //await StartClientAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR COK SplitDataDeduct " + ex.Message);
            }
            await StartClientAsync();
        }

        private async void Cb1_Checked(object sender, RoutedEventArgs e)// card loop
        {
            lblCard.Text = "Auto Repeat";
            lblCard.Foreground = Brushes.Green;
            if (Cb1.IsChecked == true)
            {
                await Task.Run(async () =>
                {
                    // await waiting();
                    //await StartClientAsync();
                });
                isCard = true;
            }
        }

        private async void Cb2_Checked(object sender, RoutedEventArgs e) // deduct loop
        {
            lblDeduct.Text = "Auto Repeat";
            lblDeduct.Foreground = Brushes.Green;
            CbFlags = true;
            if (Cb2.IsChecked == true)
            {
                await Task.Run(async () =>
                {

                    while (isCard && CbFlags)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Fare = txtFare.Text;
                        });

                        if (isCard == true && responseData.Contains("0200"))
                        {
                            responseData = await SendToHostAsync("01" + Fare);
                            SplitDataDeduct();
                            Debug.WriteLine("dedeuct loop");
                            await Task.Delay(5000);
                        }
                        if (!CbFlags)
                        {
                            break; // Stop the function execution if the flag is false
                        }

                    }

                });
            }
        }


        private async void Cb1_Unchecked(object sender, RoutedEventArgs e)//card loop
        {
            lblCard.Text = "";
            isCard = false; // resk6
            await waiting();
        }
        private async void Cb2_Unchecked(object sender, EventArgs e) // deduct loop
        {
            lblDeduct.Text = " ";
            CbFlags = false;
        }


    }
}
//this done reska6